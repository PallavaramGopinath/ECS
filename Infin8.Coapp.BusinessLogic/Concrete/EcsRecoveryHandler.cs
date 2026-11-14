using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using Infin8.Coapp.Utility.Enums;

namespace Infin8.Coapp.BusinessLogic
{
    /// <summary>
    /// ECS Recovery Appropriation (spec 13): per pay office of a posted demand, split the employer's remittance across
    /// the demand (<see cref="EcsDemandAppropriator"/>), save it as receipts with one voucher, or undo either step.
    /// </summary>
    public class EcsRecoveryHandler : IEcsRecoveryHandler
    {
        private const double Tolerance = 0.005;
        private const int VocTypeAdjustment = 2;
        private const int VocTrnTypeAdjustment = 2;
        private static readonly DateTime NoDate = new(1900, 1, 1);

        readonly IUnitOfWork _unitOfWork;

        public EcsRecoveryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // =====================================================================
        // State
        // =====================================================================

        public async Task<EcsRecoveryStateVM> GetStateAsync(EcsUserContextVM ctx)
        {
            var state = new EcsRecoveryStateVM { BusinessDay = ctx.CurrentDate.Date };
            state.Message = await CheckBusinessDayAsync(ctx);
            state.IsOpenBusinessDay = state.Message == null;

            var masters = await _unitOfWork.EcsDemand.GetLiveMastersAsync(ctx.BrCode, 24);
            var posted = masters.Where(m => m.IsDemandUpdated && m.DemandDate != null).ToList();
            // Demands still being recovered, plus the latest fully saved one (so its saves can be deleted).
            var shown = posted.Where(m => !m.IsRecoveryUpdated).ToList();
            var lastSaved = posted.FirstOrDefault(m => m.IsRecoveryUpdated);
            if (lastSaved != null) shown.Add(lastSaved);

            var names = await _unitOfWork.EcsRecovery.GetOfficeNamesAsync(ctx.BrCode);
            foreach (var m in shown.OrderByDescending(m => m.DemandDate))
            {
                var statuses = await _unitOfWork.EcsDemand.GetStatusRowsForUpdateAsync(m.Demand_Id, ctx.BrCode);
                var stats = (await _unitOfWork.EcsRecovery.GetOfficeStatsAsync(m.Demand_Id, ctx.BrCode)).ToDictionary(s => s.OfficeId);
                var vocNos = await _unitOfWork.EcsRecovery.GetVoucherNosAsync(statuses.Select(s => s.Voc_Id), ctx.BrCode);
                state.Demands.Add(new EcsRecoveryDemandVM
                {
                    DemandId = m.Demand_Id,
                    DueDate = m.DemandDate!.Value.Date,
                    IsRecoveryAppropriated = m.IsRecoveryAppropriated,
                    IsRecoveryUpdated = m.IsRecoveryUpdated,
                    HasLaterDemand = masters.Any(x => x.DemandDate > m.DemandDate),
                    Offices = statuses.OrderBy(s => s.Office_Id)
                                      .Select(s => ToOfficeVM(s, stats.GetValueOrDefault(s.Office_Id), names, vocNos)).ToList()
                });
            }
            return state;
        }

        public async Task<EcsRecoveryOfficeDetailVM?> GetOfficeAsync(decimal demandId, decimal officeId, EcsUserContextVM ctx)
        {
            var brCode = ctx.BrCode;
            var masters = await _unitOfWork.EcsDemand.GetLiveMastersAsync(brCode, 1000);
            var master = masters.FirstOrDefault(m => m.Demand_Id == demandId);
            if (master?.DemandDate == null) return null;
            var status = (await _unitOfWork.EcsDemand.GetStatusRowsForUpdateAsync(demandId, brCode)).FirstOrDefault(s => s.Office_Id == officeId);
            if (status == null) return null;

            var names = await _unitOfWork.EcsRecovery.GetOfficeNamesAsync(brCode);
            var stats = (await _unitOfWork.EcsRecovery.GetOfficeStatsAsync(demandId, brCode)).FirstOrDefault(s => s.OfficeId == officeId);
            var vocNos = await _unitOfWork.EcsRecovery.GetVoucherNosAsync(new[] { status.Voc_Id }, brCode);
            var lines = await _unitOfWork.EcsRecovery.GetOfficeLinesAsync(demandId, officeId, brCode, forUpdate: false);
            var members = await _unitOfWork.EcsRecovery.GetOfficeMembersAsync(officeId, brCode);

            var labels = await _unitOfWork.EcsDemand.GetAccountLabelsAsync(lines.Where(l => l.Demand_Type != "A"), brCode);
            foreach (var kv in await _unitOfWork.EcsRecovery.GetLoanLabelsAsync(lines.Where(l => l.Demand_Type == "A").Select(l => l.Loan_Id), brCode))
                labels[$"L:{kv.Key}"] = kv.Value;

            var detail = new EcsRecoveryOfficeDetailVM
            {
                DemandId = demandId,
                DueDate = master.DemandDate.Value.Date,
                Office = ToOfficeVM(status, stats, names, vocNos)
            };

            bool revised = status.Is_Appropriated;
            var byMember = lines.ToLookup(l => l.Mem_Id);
            var memberRows = members.ToDictionary(m => m.MemId);
            var memIds = members.Select(m => m.MemId).Union(lines.Select(l => l.Mem_Id)).Distinct();
            foreach (var memId in memIds)
            {
                memberRows.TryGetValue(memId, out var mr);
                var ml = byMember[memId].ToList();
                var vm = new EcsRecoveryMemberVM
                {
                    MemId = memId,
                    MemberNo = mr?.MemberNo ?? memId.ToString(),
                    PerNo = mr?.PerNo,
                    MemberName = mr?.MemberName ?? "(closed member)",
                    Stopped = ml.Count > 0 && ml.Where(IsDemandLine).All(l => l.Stop_Demand),
                    Demand = R(ml.Where(l => IsDemandLine(l) && !l.Stop_Demand).Sum(l => revised ? l.Total_Demand2 : l.Total_Demand)),
                    Recovery = R(ml.Sum(l => l.Total_Collection)),
                    Loan = R(ml.Where(l => l.Demand_Type == "L").Sum(l => l.Loan_Total_Collection)),
                    Deposit = R(ml.Where(l => l.Demand_Type == "D").Sum(l => l.Deposit_Total_Collection)),
                    Other = R(ml.Where(l => l.Demand_Type is "S" or "R").Sum(l => l.Debtor_Total_Collection + l.RD_Total_Receipt)),
                    Advance = R(ml.Where(l => l.Demand_Type == "A").Sum(l => l.Loan_Total_Collection)),
                    Excess = R(ml.Where(l => l.Demand_Type == "B").Sum(l => l.Creditor_Collection)),
                    Lines = ml.Select(l => ToLineVM(l, labels, revised)).ToList()
                };
                detail.Members.Add(vm);
            }
            detail.Members = detail.Members.OrderBy(m => m.MemberNo, StringComparer.OrdinalIgnoreCase).ToList();
            detail.Totals = HeadTotals(lines);

            detail.Ledgers = (await _unitOfWork.EcsRecovery.GetLedgersAsync(brCode))
                .Select(l => new EcsRecoveryLedgerVM { LedId = l.LedId, LedName = l.LedName, IsBank = l.IsBank }).ToList();
            detail.DefaultDebitLedId = await _unitOfWork.EcsRecovery.GetLastDebitLedgerAsync(officeId, brCode) ?? 0;

            if (status.Is_Recovery_Saved)
            {
                detail.DeleteSavedBlockedReasons = await DeleteSavedBlockersAsync(master, status, masters, brCode);
                detail.CanDeleteSaved = detail.DeleteSavedBlockedReasons.Count == 0;
            }
            return detail;
        }

        // =====================================================================
        // Appropriate (§4)
        // =====================================================================

        public async Task<EcsRecoveryResultVM> AppropriateAsync(EcsAppropriateRequestVM request, EcsUserContextVM ctx)
        {
            var brCode = ctx.BrCode;
            var guard = await CommonGuardsAsync(ctx);
            if (guard != null) return Fail(guard);

            _unitOfWork.BeginTransaction();
            try
            {
                var (master, status, statuses, error) = await LoadOfficeAsync(request.DemandId, request.OfficeId, brCode);
                if (error != null) { _unitOfWork.RollBack(); return Fail(error); }
                if (status!.Is_Recovery_Saved) { _unitOfWork.RollBack(); return Fail("The recovery of this office is already saved."); }
                if (status.Is_Appropriated) { _unitOfWork.RollBack(); return Fail("The recovery of this office is already appropriated. Delete the appropriation first."); }
                var masters = await _unitOfWork.EcsDemand.GetLiveMastersAsync(brCode, 1000);
                if (masters.Any(m => m.DemandDate > master!.DemandDate && m.IsDemandUpdated))
                {
                    _unitOfWork.RollBack();
                    return Fail("A later demand is already posted, so this demand can no longer be appropriated.");
                }

                // --- the remittance list ---
                var names = await _unitOfWork.EcsRecovery.GetOfficeNamesAsync(brCode);
                var (amounts, reasons) = await ResolveItemsAsync(request, names, brCode);
                if (reasons.Count > 0)
                {
                    _unitOfWork.RollBack();
                    return Fail("The recovery list has errors. Nothing was appropriated.", reasons);
                }
                if (amounts.Count == 0)
                {
                    _unitOfWork.RollBack();
                    return Fail("Enter at least one recovery amount. (To record that the office remitted nothing, save it without appropriating amounts.)");
                }

                var priorities = (await _unitOfWork.EcsRecovery.GetPrioritiesAsync(brCode))
                    .Where(p => !string.IsNullOrWhiteSpace(p.PriorityType))
                    .Select(p => new EcsPriorityHead(p.PriorityNo ?? 0, p.PriorityType!.Trim().ToUpperInvariant(),
                        p.Scheme_Id ?? 0, p.Deposit_Id ?? 0, p.PriorityLed_Id ?? 0)).ToList();
                if (priorities.Count == 0)
                {
                    _unitOfWork.RollBack();
                    return Fail("No collection priority (mem_collection_priority) is set up for this branch.");
                }

                // --- lines, revised demand (§4.1) ---
                var alloc = await _unitOfWork.EcsRecovery.CreateIdAllocatorAsync(brCode);
                var zero = alloc.ZeroId;
                var all = await _unitOfWork.EcsRecovery.GetOfficeLinesAsync(request.DemandId, request.OfficeId, brCode, forUpdate: true);
                foreach (var stale in all.Where(l => l.Demand_Type is "A" or "B")) stale.Is_Active = false;
                var lines = all.Where(IsDemandLine).ToList();
                var inputs = await LoadRevisionInputsAsync(lines, amounts.Keys, brCode);
                foreach (var l in lines.Where(l => !l.Stop_Demand)) ReviseLine(l, inputs);

                // --- split (§4.2) ---
                var day = ctx.CurrentDate.Date;
                var recoveryDate = DateOnly.FromDateTime(day);
                int recoveryId = await _unitOfWork.EcsRecovery.GetMaxRecoveryIdAsync(brCode) + 1;
                var byMember = lines.ToLookup(l => l.Mem_Id);
                var newLines = new List<Mem_Demand>();
                var unappropriated = new List<string>();
                var memberNos = (await _unitOfWork.EcsRecovery.GetOfficeMembersAsync(request.OfficeId, brCode)).ToDictionary(m => m.MemId, m => m.MemberNo);

                foreach (var memId in byMember.Select(g => g.Key).Union(amounts.Keys).Distinct())
                {
                    var ml = byMember[memId].ToList();
                    double amount = amounts.GetValueOrDefault(memId);
                    if (amount <= 0)
                    {
                        foreach (var l in ml) EcsDemandAppropriator.ClearCollections(l);
                        continue;
                    }
                    var inDemand = ml.Where(l => l.Demand_Type == "L" && !l.Stop_Demand).Select(l => l.Loan_Id).ToHashSet();
                    var notInDemand = inputs.OpenLoans.Where(x => x.MemId == memId && !inDemand.Contains(x.LoanId))
                        .Select(ToAppropriationLoan).ToList();
                    var result = EcsDemandAppropriator.Appropriate(amount, ml, inputs.AppropriationLoans, notInDemand,
                        priorities, zero, type => NewLine(type, memId, request.DemandId, recoveryId, recoveryDate, zero, alloc, ctx));
                    newLines.AddRange(result.NewLines);
                    if (result.Unappropriated > Tolerance)
                        unappropriated.Add($"Member {memberNos.GetValueOrDefault(memId) ?? memId.ToString()}: {result.Unappropriated:N2} left over");
                }
                if (unappropriated.Count > 0)
                {
                    _unitOfWork.RollBack();
                    return Fail("Some recovery could not be appropriated — the branch has no DBY (excess) collection priority.", unappropriated);
                }

                // --- writes (§4.3) ---
                foreach (var l in lines.Where(l => !l.Stop_Demand))
                {
                    l.Recovery_Id = recoveryId;
                    l.Recovery_Date = recoveryDate;
                    l.Is_Appropriated = true;
                }
                await _unitOfWork.EcsRecovery.AddLinesAsync(newLines);
                status.Is_Appropriated = true;
                master!.IsRecoveryAppropriated = statuses!.All(s => s.Is_Appropriated);

                await _unitOfWork.CompleteAsync();
                _unitOfWork.CommitTransaction();

                double total = R(lines.Sum(l => l.Total_Collection) + newLines.Sum(l => l.Total_Collection));
                return new EcsRecoveryResultVM
                {
                    Success = true,
                    Message = $"Recovery of {names.GetValueOrDefault(request.OfficeId) ?? "the office"} appropriated: {amounts.Count} member(s), {total:N2}"
                              + (newLines.Count(l => l.Demand_Type == "B") is > 0 and var ex ? $", {ex} excess." : ".")
                };
            }
            catch (Exception ex)
            {
                _unitOfWork.RollBack();
                throw Wrap(ex, "Nothing was appropriated.");
            }
        }

        private Mem_Demand NewLine(string type, decimal memId, decimal demandId, int recoveryId, DateOnly recoveryDate,
            decimal zero, EcsRecoveryIdAllocator alloc, EcsUserContextVM ctx) => new()
        {
            Id = alloc.NextLineId(),
            Demand_Id = demandId,
            Demand_Type = type,
            Mem_Id = memId,
            Calculated_Date = recoveryDate,
            Recovery_Date = recoveryDate,
            Recovery_Id = recoveryId,
            Loan_Id = zero,
            Debtor_Led_Id = zero,
            TD_Id = zero,
            DueBy_Led_Id = zero,
            Creditor_Led_Id = zero,
            RD_PI_Application_Date2 = NoDate,
            Voc_Id = zero,
            Usr_Id = ctx.UsrId,
            Yr_Id = ctx.YrId,
            BrCode = ctx.BrCode,
            Is_Active = true,
            Is_Appropriated = true
        };

        /// <summary>Maps the remittance list to member ids; every problem is reported (spec 13 §4).</summary>
        private async Task<(Dictionary<decimal, double> amounts, List<string> reasons)> ResolveItemsAsync(
            EcsAppropriateRequestVM request, Dictionary<decimal, string> officeNames, string brCode)
        {
            var reasons = new List<string>();
            var amounts = new Dictionary<decimal, double>();
            bool byPerNo = string.Equals(request.MatchBy, "P", StringComparison.OrdinalIgnoreCase);
            string what = byPerNo ? "Per no" : "Member no";
            static string Norm(string? s) => (s ?? "").Trim().ToUpperInvariant();

            var members = await _unitOfWork.EcsRecovery.GetBranchMembersAsync(brCode);
            var index = members.Where(m => !string.IsNullOrWhiteSpace(byPerNo ? m.PerNo : m.MemberNo))
                               .ToLookup(m => Norm(byPerNo ? m.PerNo : m.MemberNo));
            var seen = new HashSet<string>();
            int row = 0;
            foreach (var item in request.Items)
            {
                row++;
                var key = Norm(item.Key);
                if (key.Length == 0)
                {
                    if (item.Amount != 0) reasons.Add($"Row {row}: {what} is missing.");
                    continue;
                }
                if (!seen.Add(key)) { reasons.Add($"{what} {key} appears more than once."); continue; }
                if (item.Amount < 0 || double.IsNaN(item.Amount)) { reasons.Add($"{what} {key}: the amount cannot be negative."); continue; }
                var found = index[key].ToList();
                if (found.Count == 0) { reasons.Add($"{what} {key} is not an open member of this branch."); continue; }
                if (found.Count > 1) { reasons.Add($"{what} {key} matches {found.Count} members."); continue; }
                var m = found[0];
                if (m.OfficeId != request.OfficeId)
                {
                    reasons.Add($"{what} {key} {m.MemberName} belongs to {officeNames.GetValueOrDefault(m.OfficeId) ?? "another office"}.");
                    continue;
                }
                if (R(item.Amount) > 0) amounts[m.MemId] = R(item.Amount);
            }
            return (amounts, reasons);
        }

        // =====================================================================
        // Delete appropriation (§5)
        // =====================================================================

        public async Task<EcsRecoveryResultVM> DeleteAppropriationAsync(decimal demandId, decimal officeId, EcsUserContextVM ctx)
        {
            var guard = await CommonGuardsAsync(ctx);
            if (guard != null) return Fail(guard);

            _unitOfWork.BeginTransaction();
            try
            {
                var (master, status, _, error) = await LoadOfficeAsync(demandId, officeId, ctx.BrCode);
                if (error != null) { _unitOfWork.RollBack(); return Fail(error); }
                if (!status!.Is_Appropriated) { _unitOfWork.RollBack(); return Fail("The recovery of this office is not appropriated."); }
                if (status.Is_Recovery_Saved) { _unitOfWork.RollBack(); return Fail("The recovery is saved in the ledgers. Delete the saved recovery first."); }

                var lines = await _unitOfWork.EcsRecovery.GetOfficeLinesAsync(demandId, officeId, ctx.BrCode, forUpdate: true);
                int removed = 0;
                foreach (var l in lines)
                {
                    if (l.Demand_Type is "A" or "B")
                    {
                        l.Is_Active = false;
                        removed++;
                        continue;
                    }
                    EcsDemandAppropriator.ClearCollections(l);
                    l.Recovery_Id = 0;
                    l.Recovery_Date = null;
                    l.Is_Appropriated = false;
                }
                status.Is_Appropriated = false;
                master!.IsRecoveryAppropriated = false;

                await _unitOfWork.CompleteAsync();
                _unitOfWork.CommitTransaction();
                return new EcsRecoveryResultVM
                {
                    Success = true,
                    Message = $"Appropriation deleted{(removed > 0 ? $" ({removed} advance/excess line(s) removed)" : "")}."
                };
            }
            catch (Exception ex)
            {
                _unitOfWork.RollBack();
                throw Wrap(ex, "Nothing was deleted.");
            }
        }

        // =====================================================================
        // Save recovery (§6)
        // =====================================================================

        public async Task<EcsRecoveryResultVM> SaveAsync(EcsSaveRecoveryRequestVM request, EcsUserContextVM ctx)
        {
            var brCode = ctx.BrCode;
            var guard = await CommonGuardsAsync(ctx);
            if (guard != null) return Fail(guard);

            _unitOfWork.BeginTransaction();
            try
            {
                var (master, status, statuses, error) = await LoadOfficeAsync(request.DemandId, request.OfficeId, brCode);
                if (error != null) { _unitOfWork.RollBack(); return Fail(error); }
                if (status!.Is_Recovery_Saved) { _unitOfWork.RollBack(); return Fail("The recovery of this office is already saved."); }

                var alloc = await _unitOfWork.EcsRecovery.CreateIdAllocatorAsync(brCode);
                var zero = alloc.ZeroId;
                var due = master!.DemandDate!.Value.Date;
                var lines = await _unitOfWork.EcsRecovery.GetOfficeLinesAsync(request.DemandId, request.OfficeId, brCode, forUpdate: true);
                var names = await _unitOfWork.EcsRecovery.GetOfficeNamesAsync(brCode);
                var officeName = names.GetValueOrDefault(request.OfficeId) ?? "Office";

                // Not appropriated: allowed only as a nil recovery (§6.4).
                if (!status.Is_Appropriated)
                {
                    if (request.Debits.Any(d => d.Amount != 0))
                    {
                        _unitOfWork.RollBack();
                        return Fail("Appropriate the recovery before saving it. (Save without amounts to record a nil recovery.)");
                    }
                    status.Is_Appropriated = true;
                    master.IsRecoveryAppropriated = statuses!.All(s => s.Is_Appropriated);
                }

                // §6.1 recheck the revised demand
                if (status.Is_Appropriated && lines.Any(l => l.Total_Collection != 0))
                {
                    // Only members who recovered something: a later transaction on anyone else changes nothing.
                    var recovering = lines.Where(l => l.Total_Collection > 0).Select(l => l.Mem_Id).ToHashSet();
                    var demandLines = lines.Where(l => IsDemandLine(l) && !l.Stop_Demand && recovering.Contains(l.Mem_Id)).ToList();
                    var inputs = await LoadRevisionInputsAsync(demandLines, Array.Empty<decimal>(), brCode);
                    var changed = new List<string>();
                    foreach (var l in demandLines)
                    {
                        var before = EcsDemandAppropriator.RevisedFigures(l);
                        ReviseLine(l, inputs);
                        if (!before.Zip(EcsDemandAppropriator.RevisedFigures(l)).All(p => Math.Abs(p.First - p.Second) < Tolerance))
                            changed.Add(l.Demand_Type == "L" ? $"Loan id {l.Loan_Id} of member id {l.Mem_Id}" : $"{l.Demand_Type} line of member id {l.Mem_Id}");
                    }
                    if (changed.Count > 0)
                    {
                        _unitOfWork.RollBack();
                        return Fail("The ledgers have changed since the recovery was appropriated. Delete the appropriation and appropriate again.",
                            changed.Take(20).ToList());
                    }
                }

                var collected = lines.Where(l => !l.Stop_Demand && l.Total_Collection > 0).ToList();
                double total = R(collected.Sum(l => l.Total_Collection));
                var debits = request.Debits.Where(d => d.LedId != 0 || d.Amount != 0).ToList();

                if (total <= 0)
                {
                    if (debits.Any(d => d.Amount != 0))
                    {
                        _unitOfWork.RollBack();
                        return Fail("Nothing was recovered from this office, so no remittance amount can be entered.");
                    }
                    MarkSaved(lines, status, statuses!, master, zero);
                    await _unitOfWork.CompleteAsync();
                    _unitOfWork.CommitTransaction();
                    return new EcsRecoveryResultVM { Success = true, Message = $"Nil recovery of {officeName} saved." };
                }

                // --- debit (remittance) lines ---
                var ledgers = (await _unitOfWork.EcsRecovery.GetLedgersAsync(brCode)).ToDictionary(l => l.LedId);
                var reasons = new List<string>();
                if (debits.Count == 0) reasons.Add("Enter the ledger the remittance was received in.");
                foreach (var d in debits)
                {
                    if (!ledgers.TryGetValue(d.LedId, out var led)) { reasons.Add("A remittance ledger is not a ledger of this branch."); continue; }
                    if (d.Amount <= 0) reasons.Add($"{led.LedName}: enter an amount.");
                    if (led.IsBank && string.IsNullOrWhiteSpace(d.ChequeNo)) reasons.Add($"{led.LedName}: enter the cheque / reference no.");
                }
                if (Math.Abs(R(debits.Sum(d => d.Amount)) - total) > Tolerance)
                    reasons.Add($"The remittance ({R(debits.Sum(d => d.Amount)):N2}) must equal the appropriated recovery ({total:N2}).");

                // --- credit ledgers ---
                var loanIds = collected.Where(l => l.Demand_Type is "L" or "A").Select(l => l.Loan_Id).Distinct().ToList();
                var loans = await _unitOfWork.EcsRecovery.GetLoansAsync(loanIds, brCode);
                var schemes = await _unitOfWork.EcsRecovery.GetSchemesAsync(loans.Values.Select(l => l.SchemeId), brCode);
                var depositLedgers = await _unitOfWork.EcsRecovery.GetDepositLedgersAsync(brCode);
                var rdLedgers = await _unitOfWork.EcsRecovery.GetRdLedgersAsync(collected.Where(l => l.Demand_Type == "R").Select(l => l.TD_Id), brCode);
                var lastLoanDates = await _unitOfWork.EcsRecovery.GetLastLoanTrnDatesAsync(loanIds, brCode);

                var credits = new Dictionary<decimal, double>();
                void Credit(decimal ledId, double amount, string what)
                {
                    if (amount <= 0) return;
                    if (ledId == 0 || ledId == zero || !ledgers.ContainsKey(ledId)) { reasons.Add($"{what} has no valid ledger."); return; }
                    credits[ledId] = R(credits.GetValueOrDefault(ledId) + amount);
                }
                foreach (var l in collected)
                {
                    switch (l.Demand_Type)
                    {
                        case "L":
                        case "A":
                            if (!loans.TryGetValue(l.Loan_Id, out var ln) || !schemes.TryGetValue(ln.SchemeId, out var s))
                            {
                                reasons.Add($"Loan id {l.Loan_Id}: loan or scheme not found.");
                                break;
                            }
                            Credit(s.PILed_Id, l.loan_PI_Arrear_Collection + l.Loan_PI_Current_Collection, $"Scheme {s.Scheme_Name} penal interest");
                            Credit(s.IntLed_Id, l.Loan_Int_Arrear_Collection + l.Loan_Int_Current_Collection, $"Scheme {s.Scheme_Name} interest");
                            Credit(s.PrlLed_Id, l.Loan_Prl_Arrear_Collection + l.Loan_Prl_Current_Collection, $"Scheme {s.Scheme_Name} principal");
                            break;
                        case "D":
                            var (depLed, depPiLed) = depositLedgers.GetValueOrDefault(l.Deposit_Id);
                            Credit(depLed, l.Deposit_Arrear_Collection + l.Deposit_Current_Collection, $"Deposit {l.Deposit_Id}");
                            Credit(depPiLed, l.Deposit_Arrear_PI_Collection + l.Deposit_Current_PI_Collection, $"Deposit {l.Deposit_Id} penal interest");
                            break;
                        case "S":
                            Credit(l.Debtor_Led_Id, l.Debtor_Total_Collection, "A due-to line");
                            break;
                        case "B":
                            Credit(l.Creditor_Led_Id, l.Creditor_Collection, "The excess recovery");
                            break;
                        case "R":
                            Credit(rdLedgers.GetValueOrDefault(l.TD_Id), l.RD_Total_Receipt, $"RD id {l.TD_Id}");
                            break;
                    }
                }
                if (reasons.Count == 0 && Math.Abs(R(credits.Values.Sum()) - total) > Tolerance)
                    reasons.Add($"The receipts ({R(credits.Values.Sum()):N2}) do not add up to the recovery ({total:N2}).");
                if (reasons.Count > 0)
                {
                    _unitOfWork.RollBack();
                    return Fail("The recovery cannot be saved.", reasons.Distinct().ToList());
                }

                // --- build (§6.2, §6.3) ---
                await alloc.PrimeSerialsAsync(loanIds, collected.Select(l => l.Mem_Id), collected.Where(l => l.Demand_Type == "R").Select(l => l.TD_Id));
                var (rptSlNo, pmtSlNo) = await _unitOfWork.EcsLoanDisbursement.GetNextVoucherSlNosAsync(ctx.YrId, brCode);
                var vocId = alloc.NextVocId();
                var day = ctx.CurrentDate.Date;
                var narration = Trunc($"{officeName} Factory recovery {due:MMM yyyy}", 100);
                var p = new EcsRecoverySavePosting
                {
                    Voucher = new Fin_Voucher
                    {
                        Voc_Id = vocId,
                        Voc_SlNo = 0,
                        Voc_Date = day,
                        Voc_Type = VocTypeAdjustment,
                        Voc_Mode = "",
                        Voc_Rpt_SlNo = rptSlNo,
                        Voc_Rpt_No = $"AR/{rptSlNo}",
                        Voc_Rpt_Mode = "AR",
                        Voc_Pmt_SlNo = pmtSlNo,
                        Voc_Pmt_No = $"VR/{pmtSlNo}",
                        Voc_Pmt_Mode = "VR",
                        Voc_No = $"VR/{pmtSlNo}-AR/{rptSlNo}",
                        Voc_Amt = total,
                        Voc_Narration = narration,
                        Voc_Bus = true,
                        Voc_Delete = false,
                        Mem_Id = zero,
                        Usr_Id = ctx.UsrId,
                        Yr_Id = ctx.YrId,
                        BrCode = brCode,
                        Voc_Status = "V"
                    }
                };

                Fin_Voucher_Trn VocLine(decimal ledId, double rpt, double pmt) => new()
                {
                    Voc_Trn_Id = alloc.NextVocTrnId(),
                    Voc_Id = vocId,
                    Led_Id = ledId,
                    Voc_Rpt = R(rpt),
                    Voc_Pmt = R(pmt),
                    Voc_Trn_Type = VocTrnTypeAdjustment,
                    Voc_Narr = narration,
                    Voc_Cash_Adj_Id = 0,
                    FinVocTr_Delete = false,
                    Status = "",
                    Description = "",
                    Mem_Id = zero,
                    Loan_Id = zero,
                    Usr_Id = ctx.UsrId,
                    Yr_Id = ctx.YrId,
                    BrCode = brCode,
                    Voc_Status = "V"
                };

                foreach (var d in debits)
                {
                    p.VoucherTrns.Add(VocLine(d.LedId, 0, d.Amount));
                    if (!ledgers[d.LedId].IsBank) continue;
                    p.BankRows.Add(new Fin_Voucher_Bank
                    {
                        Fvb_Id = alloc.NextFvbId(),
                        Fvb_Date = day,
                        Mem_Id = zero,
                        Fvb_Bank_Mode = "O",
                        Fvb_Bank_Name = string.IsNullOrWhiteSpace(d.BankName) ? ledgers[d.LedId].LedName : d.BankName.Trim(),
                        Voc_Id = vocId,
                        Led_Id = d.LedId,
                        Fvb_Amount = R(d.Amount),
                        Fvb_Cheque_No = d.ChequeNo?.Trim(),
                        Fvb_Cheque_Date = d.ChequeDate?.Date ?? day,
                        Fvb_Real_Amount = 0,
                        Fvb_Cheque_Return = false,
                        Fvb_Return_Voc_Id = zero,
                        Fvb_Delete = false,
                        Usr_Id = ctx.UsrId,
                        Yr_Id = ctx.YrId,
                        BrCode = brCode,
                        Voc_Status = "V"
                    });
                }
                foreach (var c in credits.OrderBy(c => c.Key)) p.VoucherTrns.Add(VocLine(c.Key, c.Value, 0));

                foreach (var l in collected)
                {
                    switch (l.Demand_Type)
                    {
                        case "L":
                        case "A":
                            var ln = loans[l.Loan_Id];
                            var date = new[] { due, ln.SanDate?.Date ?? due, lastLoanDates.TryGetValue(l.Loan_Id, out var last) ? last.Date : due }.Max();
                            p.LoanTrns.Add(new Loan_Trn
                            {
                                Trn_Id = alloc.NextLoanTrnId(),
                                Loan_Id = l.Loan_Id,
                                Demand_Id = request.DemandId,
                                Trn_Status = "R",
                                Trn_Date = date,
                                PIColl_Amt = R(l.loan_PI_Arrear_Collection + l.Loan_PI_Current_Collection),
                                IntColl_Amt = R(l.Loan_Int_Arrear_Collection + l.Loan_Int_Current_Collection),
                                PrlColl_Amt = R(l.Loan_Prl_Arrear_Collection + l.Loan_Prl_Current_Collection),
                                Trn_SlNo = alloc.NextLoanTrnSlNo(l.Loan_Id),
                                TrnTr_Oe = false,
                                TrnTr_Delete = false,
                                Voc_Id = vocId,
                                Usr_Id = ctx.UsrId,
                                Yr_Id = ctx.YrId,
                                BrCode = brCode,
                                Voc_Status = "V"
                            });
                            break;
                        case "D":
                            p.DepositTrns.Add(new Deposit_Trn
                            {
                                Trn_Id = alloc.NextDepositTrnId(),
                                Deposit_Id = l.Deposit_Id,
                                Mem_Id = l.Mem_Id,
                                Demand_Id = request.DemandId,
                                Status = "",
                                Trn_Date = due,
                                Receipt_Amount = R(l.Deposit_Arrear_Collection + l.Deposit_Current_Collection),
                                Penal_Interest_Received_Amount = R(l.Deposit_Arrear_PI_Collection + l.Deposit_Current_PI_Collection),
                                Is_Opening_Entry = false,
                                Is_Active = true,
                                Trn_SlNo = alloc.NextDepositTrnSlNo(l.Mem_Id, l.Deposit_Id),
                                Voc_Id = vocId,
                                Usr_Id = ctx.UsrId,
                                Yr_Id = ctx.YrId,
                                BrCode = brCode
                            });
                            break;
                        case "S":
                            p.MemTrns.Add(MemTrn(1, l.Mem_Id, l.Debtor_Led_Id, due, l.Debtor_Total_Collection, vocId, zero, alloc, ctx));
                            break;
                        case "B":
                            p.MemTrns.Add(MemTrn(2, l.Mem_Id, l.Creditor_Led_Id, due, l.Creditor_Collection, vocId, zero, alloc, ctx));
                            break;
                        case "R":
                            int inst = l.RD_Instalment2 > 0 ? l.RD_Instalment2 : (int)Math.Round(l.RD_Instalment, MidpointRounding.AwayFromZero);
                            p.RdTrns.Add(new TermDeposit_Trn
                            {
                                TDTrn_Id = alloc.NextRdTrnId(),
                                Trn_Date = due,
                                TD_Id = l.TD_Id,
                                DepositReceiptAmount = R(l.RD_Receipt),
                                PenalReceivedAmount = R(l.RD_PI_Receipt),
                                NoOfInstalments = inst > 0 ? (int)Math.Round(l.RD_Receipt / inst) : 0,
                                InstalmentAmount = inst,
                                Demand_Id = request.DemandId,
                                TDCalc_Id = 0,
                                TD_OE = false,
                                TD_Delete = false,
                                Trn_SlNo = alloc.NextRdTrnSlNo(l.TD_Id),
                                Voc_Id = vocId,
                                Usr_Id = ctx.UsrId,
                                Yr_Id = ctx.YrId,
                                BrCode = brCode,
                                Voc_Status = "V"
                            });
                            break;
                    }
                }

                await _unitOfWork.EcsRecovery.AddSaveAsync(p);
                MarkSaved(lines, status, statuses!, master, vocId);

                await _unitOfWork.CompleteAsync();
                _unitOfWork.CommitTransaction();
                return new EcsRecoveryResultVM
                {
                    Success = true,
                    VocNo = p.Voucher.Voc_No,
                    Message = $"Recovery of {officeName} saved: {total:N2}, voucher {p.Voucher.Voc_No} "
                              + $"({p.LoanTrns.Count} loan, {p.DepositTrns.Count} deposit, {p.MemTrns.Count} member and {p.RdTrns.Count} RD receipts)."
                };
            }
            catch (Exception ex)
            {
                _unitOfWork.RollBack();
                throw Wrap(ex, "Nothing was saved.");
            }
        }

        private static Mem_Trn MemTrn(byte type, decimal memId, decimal ledId, DateTime date, double amount, decimal vocId,
            decimal zero, EcsRecoveryIdAllocator alloc, EcsUserContextVM ctx) => new()
        {
            Mem_Trn_Id = alloc.NextMemTrnId(),
            Trn_Type = type,
            Mem_Id = memId,
            Led_Id = ledId,
            Trn_Date = date,
            Rpt_Amt = R(amount),
            Pmt_Amt = 0,
            MemTrn_OE = false,
            MemTrn_Delete = false,
            Voc_Id = vocId,
            Usr_Id = ctx.UsrId,
            Yr_Id = ctx.YrId,
            Trn_SlNo = alloc.NextMemTrnSlNo(memId, type, ledId),
            Status = "",
            PbleMaster_Id = zero,
            Acc_Id = zero,
            BrCode = ctx.BrCode,
            Voc_Status = "V"
        };

        private static void MarkSaved(List<Mem_Demand> lines, Mem_Demand_Status status, List<Mem_Demand_Status> statuses,
            Mem_Demand_Master master, decimal vocId)
        {
            foreach (var l in lines.Where(l => !l.Stop_Demand))
            {
                l.Voc_Id = vocId;
                l.Is_Recovery_Saved = true;
            }
            status.Is_Recovery_Saved = true;
            status.Voc_Id = vocId;
            master.IsRecoveryUpdated = statuses.All(s => s.Is_Recovery_Saved);
        }

        // =====================================================================
        // Delete saved recovery (§7)
        // =====================================================================

        public async Task<EcsRecoveryResultVM> DeleteSavedAsync(decimal demandId, decimal officeId, EcsUserContextVM ctx)
        {
            var brCode = ctx.BrCode;
            var guard = await CommonGuardsAsync(ctx);
            if (guard != null) return Fail(guard);

            _unitOfWork.BeginTransaction();
            try
            {
                var (master, status, _, error) = await LoadOfficeAsync(demandId, officeId, brCode);
                if (error != null) { _unitOfWork.RollBack(); return Fail(error); }
                if (!status!.Is_Recovery_Saved) { _unitOfWork.RollBack(); return Fail("The recovery of this office is not saved."); }

                var masters = await _unitOfWork.EcsDemand.GetLiveMastersAsync(brCode, 1000);
                var blockers = await DeleteSavedBlockersAsync(master!, status, masters, brCode);
                if (blockers.Count > 0)
                {
                    _unitOfWork.RollBack();
                    return Fail("The saved recovery cannot be deleted.", blockers);
                }

                decimal zero = decimal.TryParse(brCode + "0000000", out var z) ? z : 0;
                var vocId = status.Voc_Id;
                int rows = 0;
                if (vocId != 0 && vocId != zero)
                {
                    var r = await _unitOfWork.EcsRecovery.GetReceiptsForUpdateAsync(vocId, brCode);
                    foreach (var v in r.Vouchers) v.Voc_Delete = true;
                    foreach (var t in r.VoucherTrns) t.FinVocTr_Delete = true;
                    foreach (var b in r.BankRows) b.Fvb_Delete = true;
                    foreach (var t in r.LoanTrns) t.TrnTr_Delete = true;
                    foreach (var t in r.DepositTrns) t.Is_Active = false;
                    foreach (var t in r.MemTrns) t.MemTrn_Delete = true;
                    foreach (var t in r.RdTrns) t.TD_Delete = true;
                    rows = r.LoanTrns.Count + r.DepositTrns.Count + r.MemTrns.Count + r.RdTrns.Count;
                }

                foreach (var l in await _unitOfWork.EcsRecovery.GetOfficeLinesAsync(demandId, officeId, brCode, forUpdate: true))
                {
                    l.Voc_Id = zero;
                    l.Is_Recovery_Saved = false;
                }
                status.Is_Recovery_Saved = false;
                status.Voc_Id = zero;
                master!.IsRecoveryUpdated = false;

                await _unitOfWork.CompleteAsync();
                _unitOfWork.CommitTransaction();
                return new EcsRecoveryResultVM
                {
                    Success = true,
                    Message = $"Saved recovery deleted ({rows} receipt row(s) reversed). The appropriation remains."
                };
            }
            catch (Exception ex)
            {
                _unitOfWork.RollBack();
                throw Wrap(ex, "Nothing was deleted.");
            }
        }

        private async Task<List<string>> DeleteSavedBlockersAsync(Mem_Demand_Master master, Mem_Demand_Status status,
            List<Mem_Demand_Master> liveMasters, string brCode)
        {
            var reasons = new List<string>();
            var later = liveMasters.Where(m => m.DemandDate > master.DemandDate).OrderBy(m => m.DemandDate).FirstOrDefault();
            if (later != null)
                reasons.Add($"The demand for {later.DemandDate:dd-MM-yyyy} is already calculated. Delete it first.");
            decimal zero = decimal.TryParse(brCode + "0000000", out var z) ? z : 0;
            if (status.Voc_Id != 0 && status.Voc_Id != zero)
            {
                var trns = await _unitOfWork.EcsRecovery.GetLaterTransactionsAsync(status.Voc_Id, brCode);
                if (trns.Count > 0)
                {
                    reasons.Add($"{trns.Count} account(s) have transactions after this recovery:");
                    reasons.AddRange(trns.Take(20));
                    if (trns.Count > 20) reasons.Add($"…and {trns.Count - 20} more.");
                }
            }
            return reasons;
        }

        // =====================================================================
        // Revised demand inputs
        // =====================================================================

        private sealed class RevisionInputs
        {
            public Dictionary<decimal, EcsRecoveryLoanRow> Loans { get; set; } = new();
            public Dictionary<decimal, EcsAppropriationLoan> AppropriationLoans { get; set; } = new();
            public List<EcsRecoveryLoanRow> OpenLoans { get; set; } = new();
            public Dictionary<(decimal, decimal), double> DueTo { get; set; } = new();
        }

        private async Task<RevisionInputs> LoadRevisionInputsAsync(List<Mem_Demand> lines, IEnumerable<decimal> recoveringMembers, string brCode)
        {
            var inputs = new RevisionInputs
            {
                Loans = await _unitOfWork.EcsRecovery.GetLoansAsync(lines.Where(l => l.Demand_Type == "L").Select(l => l.Loan_Id), brCode),
                OpenLoans = await _unitOfWork.EcsRecovery.GetOpenLoansOfMembersAsync(recoveringMembers, brCode),
                DueTo = await _unitOfWork.EcsRecovery.GetDueToBalancesAsync(lines.Where(l => l.Demand_Type == "S").Select(l => l.Mem_Id), brCode)
            };
            inputs.AppropriationLoans = inputs.Loans.ToDictionary(kv => kv.Key, kv => ToAppropriationLoan(kv.Value));
            return inputs;
        }

        private static void ReviseLine(Mem_Demand l, RevisionInputs inputs)
        {
            EcsLoanBalance? bal = null;
            if (l.Demand_Type == "L" && inputs.Loans.TryGetValue(l.Loan_Id, out var ln))
                bal = new EcsLoanBalance(ln.Disb, ln.PrlColl, ln.IntCalc, ln.IntColl, ln.PICalc, ln.PIColl);
            double? dueTo = l.Demand_Type == "S" ? inputs.DueTo.GetValueOrDefault((l.Mem_Id, l.Debtor_Led_Id)) : null;
            EcsDemandAppropriator.Revise(l, bal, dueTo);
        }

        private static EcsAppropriationLoan ToAppropriationLoan(EcsRecoveryLoanRow l) =>
            new(l.LoanId, l.SchemeId, l.SanDate, Math.Max(0, R(l.Disb - l.PrlColl)));

        // =====================================================================
        // Guards / loading
        // =====================================================================

        private async Task<(Mem_Demand_Master? master, Mem_Demand_Status? status, List<Mem_Demand_Status>? statuses, string? error)>
            LoadOfficeAsync(decimal demandId, decimal officeId, string brCode)
        {
            var master = await _unitOfWork.EcsDemand.GetMasterForUpdateAsync(demandId, brCode);
            if (master?.DemandDate == null) return (null, null, null, "The demand no longer exists.");
            if (!master.IsDemandUpdated) return (null, null, null, "The demand is not posted yet. Post it on the Demand Calculation page first.");
            var statuses = await _unitOfWork.EcsDemand.GetStatusRowsForUpdateAsync(demandId, brCode);
            var status = statuses.FirstOrDefault(s => s.Office_Id == officeId);
            if (status == null) return (null, null, null, "This office has no demand in this demand date.");
            return (master, status, statuses, null);
        }

        private async Task<string?> CommonGuardsAsync(EcsUserContextVM ctx)
        {
            if (!await IsEcsAsync(ctx.BrCode)) return "Recovery Appropriation is available only to ECS societies.";
            return await CheckBusinessDayAsync(ctx);
        }

        private async Task<string?> CheckBusinessDayAsync(EcsUserContextVM ctx)
        {
            var open = await _unitOfWork.EcsDemand.GetOpenBusinessDayAsync(ctx.BrCode);
            if (open == null) return "No business day is open. Begin the day first.";
            if (open.Value.Date != ctx.CurrentDate.Date)
                return $"The business day has changed since you logged in (open business day: {open.Value:dd-MM-yyyy}). Log in again.";
            return null;
        }

        private async Task<bool> IsEcsAsync(string brCode)
        {
            if (string.IsNullOrEmpty(brCode)) return false;
            var society = await _unitOfWork.General.GetSocietyData(brCode);
            return (society?.Bank_Type ?? 0).ToInstitutionType() == InstitutionType.ECS;
        }

        // =====================================================================
        // Mapping
        // =====================================================================

        private static bool IsDemandLine(Mem_Demand l) => l.Demand_Type is "L" or "D" or "S" or "R";

        private static EcsRecoveryOfficeVM ToOfficeVM(Mem_Demand_Status s, EcsRecoveryOfficeStatRow? stat,
            Dictionary<decimal, string> names, Dictionary<decimal, string?> vocNos) => new()
        {
            OfficeId = s.Office_Id,
            OfficeName = names.GetValueOrDefault(s.Office_Id) ?? $"Office {s.Office_Id}",
            Members = stat?.Members ?? 0,
            Demand = R(stat?.Demand ?? 0),
            Recovery = R(stat?.Recovery ?? 0),
            IsAppropriated = s.Is_Appropriated,
            IsSaved = s.Is_Recovery_Saved,
            RecoveryId = s.Is_Appropriated ? stat?.RecoveryId ?? 0 : 0,
            RecoveryDate = s.Is_Appropriated ? stat?.RecoveryDate?.ToDateTime(TimeOnly.MinValue) : null,
            VocId = s.Voc_Id,
            VocNo = vocNos.GetValueOrDefault(s.Voc_Id)
        };

        private static EcsRecoveryLineVM ToLineVM(Mem_Demand l, Dictionary<string, string> labels, bool revised)
        {
            string key = l.Demand_Type switch
            {
                "L" or "A" => $"L:{l.Loan_Id}",
                "D" => $"D:{l.Deposit_Id}",
                "R" => $"R:{l.TD_Id}",
                "S" => $"S:{l.Debtor_Led_Id}",
                _ => ""
            };
            string account = l.Demand_Type == "B" ? "Excess recovery" : labels.GetValueOrDefault(key) ?? l.Demand_Type ?? "";
            if (l.Demand_Type == "A") account += " (advance)";
            return new EcsRecoveryLineVM
            {
                Type = l.Demand_Type,
                Account = account,
                Stopped = l.Stop_Demand,
                Demand = R(revised ? l.Total_Demand2 : l.Total_Demand),
                Principal = R(l.Loan_Prl_Arrear_Collection + l.Loan_Prl_Current_Collection),
                Interest = R(l.Loan_Int_Arrear_Collection + l.Loan_Int_Current_Collection),
                Penal = R(l.loan_PI_Arrear_Collection + l.Loan_PI_Current_Collection + l.Deposit_Arrear_PI_Collection + l.Deposit_Current_PI_Collection),
                Amount = R(l.Deposit_Arrear_Collection + l.Deposit_Current_Collection + l.Debtor_Total_Collection + l.RD_Total_Receipt + l.Creditor_Collection),
                Collected = R(l.Total_Collection)
            };
        }

        private static EcsRecoveryHeadTotalsVM HeadTotals(List<Mem_Demand> lines)
        {
            var live = lines.Where(l => !l.Stop_Demand).ToList();
            var loans = live.Where(l => l.Demand_Type == "L").ToList();
            return new EcsRecoveryHeadTotalsVM
            {
                Principal = R(loans.Sum(l => l.Loan_Prl_Arrear_Collection + l.Loan_Prl_Current_Collection)),
                Interest = R(loans.Sum(l => l.Loan_Int_Arrear_Collection + l.Loan_Int_Current_Collection)),
                Penal = R(loans.Sum(l => l.loan_PI_Arrear_Collection + l.Loan_PI_Current_Collection)
                          + live.Sum(l => l.Deposit_Arrear_PI_Collection + l.Deposit_Current_PI_Collection)),
                Deposit = R(live.Sum(l => l.Deposit_Arrear_Collection + l.Deposit_Current_Collection)),
                DueTo = R(live.Sum(l => l.Debtor_Total_Collection)),
                Rd = R(live.Sum(l => l.RD_Total_Receipt)),
                Advance = R(live.Where(l => l.Demand_Type == "A").Sum(l => l.Loan_Total_Collection)),
                Excess = R(live.Sum(l => l.Creditor_Collection)),
                Total = R(live.Sum(l => l.Total_Collection))
            };
        }

        private static string Trunc(string s, int max) => s.Length <= max ? s : s[..max];
        private static double R(double v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);

        private static EcsRecoveryResultVM Fail(string message, List<string>? reasons = null) =>
            new() { Success = false, Message = message, Reasons = reasons ?? new() };

        private static InvalidOperationException Wrap(Exception ex, string nothingDone)
        {
            var baseMsg = ex.GetBaseException().Message;
            if (baseMsg.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) || baseMsg.Contains("23505"))
                return new InvalidOperationException($"Another user saved at the same time. {nothingDone} Please try again.");
            return new InvalidOperationException($"{ex.Message} {nothingDone}");
        }
    }
}
