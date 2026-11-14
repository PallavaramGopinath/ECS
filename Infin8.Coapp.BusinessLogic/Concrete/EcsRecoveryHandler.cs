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
                var users = await UserNamesAsync(statuses, ctx.BrCode);
                state.Demands.Add(new EcsRecoveryDemandVM
                {
                    DemandId = m.Demand_Id,
                    DueDate = m.DemandDate!.Value.Date,
                    IsRecoveryAppropriated = m.IsRecoveryAppropriated,
                    IsRecoveryUpdated = m.IsRecoveryUpdated,
                    HasLaterDemand = masters.Any(x => x.DemandDate > m.DemandDate),
                    Offices = statuses.OrderBy(s => s.Office_Id)
                                      .Select(s => ToOfficeVM(s, stats.GetValueOrDefault(s.Office_Id), names, vocNos, users)).ToList()
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
                RecoveryDate = master.DemandDate.Value.Date,
                ApplicationDate = ctx.CurrentDate.Date,
                IsOwnAppropriation = IsOwn(status, ctx),
                Office = ToOfficeVM(status, stats, names, vocNos, await UserNamesAsync(new[] { status }, brCode))
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
                var matchedRows = await MatchItemsAsync(request.Items, request.OfficeId, names, brCode);
                var reasons = matchedRows.Where(r => r.Status == "Error").Select(r => $"Row {r.RowNo}: {r.Message}").ToList();
                var amounts = matchedRows.Where(r => r.Status != "Error").ToDictionary(r => r.MemId, r => r.Amount);
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
                var inputs = await LoadRevisionInputsAsync(lines, amounts.Keys, request.DemandId, master!.DemandDate!.Value, brCode);
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
                status.Appropriated_By = ctx.UsrId;
                status.Appropriated_On = DateTime.Now;
                status.Rejected_By = null;
                status.Rejected_On = null;
                status.Reject_Reason = null;
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

        /// <summary>
        /// Matches the recovery list (sheet rows or typed amounts) to members (spec 13a §3): Employee No → <c>perno</c>,
        /// Member No → <c>memberno</c>; both given and naming different members, no match, a member of another office
        /// (by current <c>office_id</c>) or a member listed twice is an error. A name differing from the sheet is a warning.
        /// </summary>
        private async Task<List<EcsSheetPreviewRowVM>> MatchItemsAsync(IEnumerable<EcsRecoveryItemVM> items, decimal officeId,
            Dictionary<decimal, string> officeNames, string brCode)
        {
            static string Norm(string? x) => (x ?? "").Trim().ToUpperInvariant();
            static string NameKey(string? x) => new string(Norm(x).Where(char.IsLetter).ToArray());

            var members = await _unitOfWork.EcsRecovery.GetBranchMembersAsync(brCode);
            // A CSV opened and re-saved in Excel loses leading zeros (001747 → 1747): when there is no exact match, the
            // ids are compared again with leading zeros stripped on both sides (still unique or an error).
            static string NoZeros(string x) => x.TrimStart('0') is { Length: > 0 } t ? t : "0";
            var byPerNo = members.Where(m => !string.IsNullOrWhiteSpace(m.PerNo)).ToLookup(m => Norm(m.PerNo));
            var byMemberNo = members.Where(m => !string.IsNullOrWhiteSpace(m.MemberNo)).ToLookup(m => Norm(m.MemberNo));
            var byPerNoLoose = members.Where(m => !string.IsNullOrWhiteSpace(m.PerNo)).ToLookup(m => NoZeros(Norm(m.PerNo)));
            var byMemberNoLoose = members.Where(m => !string.IsNullOrWhiteSpace(m.MemberNo)).ToLookup(m => NoZeros(Norm(m.MemberNo)));
            static List<EcsRecoveryMemberRow> Find(string key, ILookup<string, EcsRecoveryMemberRow> exact, ILookup<string, EcsRecoveryMemberRow> loose)
            {
                if (key.Length == 0) return new();
                var hit = exact[key].ToList();
                return hit.Count > 0 ? hit : loose[NoZeros(key)].ToList();
            }
            var seen = new Dictionary<decimal, int>();
            var result = new List<EcsSheetPreviewRowVM>();
            int index = 0;

            foreach (var item in items)
            {
                index++;
                var row = new EcsSheetPreviewRowVM
                {
                    RowNo = item.RowNo > 0 ? item.RowNo : index,
                    EmployeeNo = item.EmployeeNo?.Trim(),
                    MemberNo = item.MemberNo?.Trim(),
                    SheetName = item.SheetName?.Trim(),
                    Amount = R(item.Amount)
                };
                result.Add(row);
                string? Error(string message) { row.Status = "Error"; row.Message = message; return null; }

                string emp = Norm(item.EmployeeNo), mem = Norm(item.MemberNo);
                if (emp.Length == 0 && mem.Length == 0) { Error("Neither an employee no nor a member no."); continue; }
                if (double.IsNaN(item.Amount) || row.Amount <= 0) { Error("The amount must be more than zero."); continue; }

                var byEmp = Find(emp, byPerNo, byPerNoLoose);
                var byMem = Find(mem, byMemberNo, byMemberNoLoose);
                if (emp.Length > 0 && byEmp.Count > 1) { Error($"Employee no {item.EmployeeNo} matches {byEmp.Count} members."); continue; }
                if (mem.Length > 0 && byMem.Count > 1) { Error($"Member no {item.MemberNo} matches {byMem.Count} members."); continue; }
                EcsRecoveryMemberRow? e = byEmp.FirstOrDefault(), m = byMem.FirstOrDefault();
                if (e != null && m != null && e.MemId != m.MemId)
                {
                    Error($"Employee no {item.EmployeeNo} is member {e.MemberNo} {e.MemberName}, but member no {item.MemberNo} is {m.MemberName}.");
                    continue;
                }
                var found = e ?? m;
                if (found == null)
                {
                    Error(emp.Length > 0 && mem.Length > 0
                        ? $"Neither employee no {item.EmployeeNo} nor member no {item.MemberNo} is an open member of this branch."
                        : emp.Length > 0 ? $"Employee no {item.EmployeeNo} is not an open member of this branch."
                                         : $"Member no {item.MemberNo} is not an open member of this branch.");
                    continue;
                }
                row.MemId = found.MemId;
                row.MatchedMemberNo = found.MemberNo;
                row.MatchedName = found.MemberName;
                if (found.OfficeId != officeId)
                {
                    Error($"Member {found.MemberNo} belongs to {officeNames.GetValueOrDefault(found.OfficeId) ?? "another office"}.");
                    continue;
                }
                if (seen.TryGetValue(found.MemId, out var firstRow))
                {
                    Error($"Member {found.MemberNo} is already listed on row {firstRow}.");
                    continue;
                }
                seen[found.MemId] = row.RowNo;
                if (!string.IsNullOrWhiteSpace(row.SheetName) && NameKey(row.SheetName) != NameKey(found.MemberName))
                {
                    row.Status = "Warning";
                    row.Message = "The name on the sheet differs from the member's name.";
                }
            }
            return result;
        }

        // =====================================================================
        // Specimen sheet (spec 13a §3) — the office's members to send to the pay office
        // =====================================================================

        public async Task<EcsSpecimenFileVM?> GetSpecimenAsync(decimal demandId, decimal officeId, string format, EcsUserContextVM ctx)
        {
            var brCode = ctx.BrCode;
            if (!await IsEcsAsync(brCode)) return null;
            var (master, _, _, error) = await LoadOfficeAsync(demandId, officeId, brCode);
            if (error != null) return null;

            var members = await _unitOfWork.EcsRecovery.GetOfficeMembersAsync(officeId, brCode);
            var lines = await _unitOfWork.EcsRecovery.GetOfficeLinesAsync(demandId, officeId, brCode, forUpdate: false);
            var demand = lines.Where(l => IsDemandLine(l) && !l.Stop_Demand)
                              .GroupBy(l => l.Mem_Id).ToDictionary(g => g.Key, g => R(g.Sum(l => l.Total_Demand)));
            var rows = members.OrderBy(m => m.MemberNo, StringComparer.OrdinalIgnoreCase)
                .Select(m => new EcsSpecimenRow(m.PerNo?.Trim(), m.MemberNo?.Trim(), m.MemberName?.Trim(), demand.GetValueOrDefault(m.MemId)))
                .ToList();

            var due = master!.DemandDate!.Value.Date;
            var office = (await _unitOfWork.EcsRecovery.GetOfficeNamesAsync(brCode)).GetValueOrDefault(officeId) ?? $"Office {officeId}";
            var society = (await _unitOfWork.General.GetSocietyData(brCode))?.Bank_Name?.Trim();
            var safeOffice = new string(office.Select(c => char.IsLetterOrDigit(c) ? c : '_').ToArray()).Trim('_');
            bool csv = string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase);
            return new EcsSpecimenFileVM
            {
                FileName = $"Recovery_{safeOffice}_{due:yyyy-MM-dd}.{(csv ? "csv" : "xlsx")}",
                ContentType = csv ? EcsRecoverySheetWriter.CsvContentType : EcsRecoverySheetWriter.XlsxContentType,
                Content = csv
                    ? EcsRecoverySheetWriter.Csv(rows)
                    : EcsRecoverySheetWriter.Xlsx(
                        $"{(string.IsNullOrEmpty(society) ? "" : society + " — ")}{office} — salary recovery for the demand due {due:dd-MM-yyyy}", rows)
            };
        }

        // =====================================================================
        // Upload (spec 13a §3) — nothing is written
        // =====================================================================

        public async Task<EcsSheetPreviewVM> UploadAsync(decimal demandId, decimal officeId, Stream file, string fileName, EcsUserContextVM ctx)
        {
            var brCode = ctx.BrCode;
            var preview = new EcsSheetPreviewVM { FileName = fileName };
            var guard = await CommonGuardsAsync(ctx);
            if (guard != null) { preview.Errors.Add(guard); return preview; }

            var (master, status, _, error) = await LoadOfficeAsync(demandId, officeId, brCode);
            if (error != null) { preview.Errors.Add(error); return preview; }
            if (status!.Is_Recovery_Saved) { preview.Errors.Add("The recovery of this office is already saved."); return preview; }
            if (status.Is_Appropriated) { preview.Errors.Add("The recovery of this office is already appropriated. Delete the appropriation first."); return preview; }

            var (rows, errors) = EcsRecoverySheetReader.Read(file, fileName);
            preview.Errors.AddRange(errors);
            var names = await _unitOfWork.EcsRecovery.GetOfficeNamesAsync(brCode);
            preview.Rows = await MatchItemsAsync(rows.Select(r => new EcsRecoveryItemVM
            {
                RowNo = r.RowNo, EmployeeNo = r.EmployeeNo, MemberNo = r.MemberNo, SheetName = r.SheetName, Amount = r.Amount
            }), officeId, names, brCode);

            preview.SheetTotal = R(rows.Sum(r => r.Amount));
            var matched = preview.Rows.Where(r => r.Status != "Error").ToList();
            preview.MatchedCount = matched.Count;
            preview.MatchedTotal = R(matched.Sum(r => r.Amount));
            var lines = await _unitOfWork.EcsRecovery.GetOfficeLinesAsync(demandId, officeId, brCode, forUpdate: false);
            preview.OfficeDemand = R(lines.Where(l => IsDemandLine(l) && !l.Stop_Demand).Sum(l => l.Total_Demand));
            return preview;
        }

        // =====================================================================
        // Delete appropriation (§5)
        // =====================================================================

        public Task<EcsRecoveryResultVM> DeleteAppropriationAsync(decimal demandId, decimal officeId, EcsUserContextVM ctx) =>
            UnappropriateAsync(demandId, officeId, null, ctx);

        /// <summary>
        /// Checker's reject (spec 13a §6): the appropriation is deleted as by the Maker, and the reason is kept on the
        /// office's status row for the Maker to see. The Maker cannot reject their own appropriation unless Admin.
        /// </summary>
        public Task<EcsRecoveryResultVM> RejectAsync(EcsRejectRequestVM request, EcsUserContextVM ctx)
        {
            var reason = (request.Reason ?? "").Trim();
            if (reason.Length == 0) return Task.FromResult(Fail("Enter the reason for rejecting the appropriation."));
            return UnappropriateAsync(request.DemandId, request.OfficeId, reason.Length > 200 ? reason[..200] : reason, ctx);
        }

        private async Task<EcsRecoveryResultVM> UnappropriateAsync(decimal demandId, decimal officeId, string? rejectReason, EcsUserContextVM ctx)
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
                if (rejectReason != null && IsOwn(status, ctx))
                {
                    _unitOfWork.RollBack();
                    return Fail("You appropriated this recovery, so another user must check it. Use Delete appropriation to withdraw it.");
                }

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
                status.Appropriated_By = null;
                status.Appropriated_On = null;
                if (rejectReason != null)
                {
                    status.Rejected_By = ctx.UsrId;
                    status.Rejected_On = DateTime.Now;
                    status.Reject_Reason = rejectReason;
                }
                master!.IsRecoveryAppropriated = false;

                await _unitOfWork.CompleteAsync();
                _unitOfWork.CommitTransaction();
                return new EcsRecoveryResultVM
                {
                    Success = true,
                    Message = (rejectReason != null ? "Appropriation rejected; it is back with the Maker" : "Appropriation deleted")
                              + (removed > 0 ? $" ({removed} advance/excess line(s) removed)." : ".")
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
                if (IsOwn(status, ctx))
                {
                    _unitOfWork.RollBack();
                    return Fail("You appropriated this recovery, so another user (a Checker) must save it.");
                }

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

                // §6.1 recheck the revised demand (spec 13a §4: as on the recovery date, every head)
                RevisionInputs? inputs = null;
                if (status.Is_Appropriated && lines.Any(l => l.Total_Collection != 0))
                {
                    // Only members who recovered something: a later transaction on anyone else changes nothing.
                    var recovering = lines.Where(l => l.Total_Collection > 0).Select(l => l.Mem_Id).ToHashSet();
                    var demandLines = lines.Where(l => IsDemandLine(l) && !l.Stop_Demand && recovering.Contains(l.Mem_Id)).ToList();
                    inputs = await LoadRevisionInputsAsync(demandLines, Array.Empty<decimal>(), request.DemandId, due, brCode);
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
                    MarkSaved(lines, status, statuses!, master, zero, ctx.CurrentDate);
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
                            // Spec 13a §5.1: an L line's receipt carries the fresh accrual of the revised demand and its
                            // dates; an A (advance) line is principal only and leaves the loan's calculation dates alone.
                            var fresh = l.Demand_Type == "L" ? inputs?.Fresh.GetValueOrDefault(l.Loan_Id) : null;
                            var date = fresh?.ReceiptDate
                                       ?? ReceiptDate(due, ln.SanDate, lastLoanDates.TryGetValue(l.Loan_Id, out var last) ? last : null);
                            p.LoanTrns.Add(new Loan_Trn
                            {
                                IntCalc_Amt = fresh?.Interest ?? 0,
                                IntCalc_Date = fresh?.IntCalcDate,
                                PICalc_Amt = fresh?.Penal ?? 0,
                                PICalc_Date = fresh?.PICalcDate,
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
                MarkSaved(lines, status, statuses!, master, vocId, ctx.CurrentDate);

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
            Mem_Demand_Master master, decimal vocId, DateTime applicationDate)
        {
            var day = DateOnly.FromDateTime(applicationDate.Date);
            foreach (var l in lines.Where(l => !l.Stop_Demand))
            {
                l.Voc_Id = vocId;
                l.Is_Recovery_Saved = true;
                if (l.Is_Appropriated) l.Recovery_Date = day;   // spec 13a §5: the application date
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
        // Recovery refund (§16)
        // =====================================================================

        private sealed class RefundContext
        {
            public Mem_Demand_Master Master { get; set; } = null!;
            public Mem_Demand_Status? Status { get; set; }
            public EcsRecoveryMemberRow Member { get; set; } = null!;
            public EcsRecoveryReceipts Receipts { get; set; } = new();
            public decimal VocId { get; set; }
            public decimal ExcessLedId { get; set; }
            public List<string> Blockers { get; } = new();
        }

        /// <summary>Loads a member's saved recovery for a demand and everything that stops a refund.</summary>
        private async Task<(RefundContext? rc, string? error)> LoadRefundAsync(decimal demandId, Func<EcsRecoveryMemberRow, bool> match, string brCode)
        {
            var masters = await _unitOfWork.EcsDemand.GetLiveMastersAsync(brCode, 1000);
            var master = await _unitOfWork.EcsDemand.GetMasterForUpdateAsync(demandId, brCode);
            if (master?.DemandDate == null || !master.IsDemandUpdated) return (null, "The demand no longer exists or is not posted.");
            var member = (await _unitOfWork.EcsRecovery.GetBranchMembersAsync(brCode)).FirstOrDefault(match);
            if (member == null) return (null, "No open member with that number in this branch.");

            var rc = new RefundContext { Master = master, Member = member };
            rc.Status = (await _unitOfWork.EcsDemand.GetStatusRowsForUpdateAsync(demandId, brCode)).FirstOrDefault(s => s.Office_Id == member.OfficeId);
            decimal zero = decimal.TryParse(brCode + "0000000", out var z) ? z : 0;
            if (rc.Status == null) { rc.Blockers.Add("The member's pay office has no demand on this date."); return (rc, null); }
            if (!rc.Status.Is_Recovery_Saved) { rc.Blockers.Add("The recovery of the member's pay office is not saved yet."); return (rc, null); }
            rc.VocId = rc.Status.Voc_Id;
            if (rc.VocId == 0 || rc.VocId == zero) { rc.Blockers.Add("The office's recovery was saved as a nil recovery; there is nothing to refund."); return (rc, null); }

            rc.Receipts = await _unitOfWork.EcsRecovery.GetMemberReceiptsForUpdateAsync(rc.VocId, member.MemId, brCode);
            var later = masters.Where(m => m.DemandDate > master.DemandDate).OrderBy(m => m.DemandDate).FirstOrDefault();
            if (later != null) rc.Blockers.Add($"The demand for {later.DemandDate:dd-MM-yyyy} is already calculated; its arrears assume these receipts.");
            var trns = await _unitOfWork.EcsRecovery.GetLaterMemberTransactionsAsync(rc.VocId, member.MemId, brCode);
            if (trns.Count > 0) rc.Blockers.Add("These accounts have transactions after the recovery: " + string.Join(", ", trns));
            rc.ExcessLedId = (await _unitOfWork.EcsRecovery.GetPrioritiesAsync(brCode))
                .FirstOrDefault(p => (p.PriorityType ?? "").Trim().ToUpperInvariant() == "DBY")?.PriorityLed_Id ?? 0;
            if (rc.ExcessLedId == 0 || rc.ExcessLedId == zero) rc.Blockers.Add("No DBY (excess recovery) ledger is set in the collection priority.");
            return (rc, null);
        }

        private static Func<EcsRecoveryMemberRow, bool> MatchMember(string key)
        {
            var k = (key ?? "").Trim();
            return m => string.Equals((m.MemberNo ?? "").Trim(), k, StringComparison.OrdinalIgnoreCase)
                        || string.Equals((m.PerNo ?? "").Trim(), k, StringComparison.OrdinalIgnoreCase);
        }

        public async Task<EcsRefundVM?> GetRefundAsync(decimal demandId, string memberNo, EcsUserContextVM ctx)
        {
            var brCode = ctx.BrCode;
            var (rc, error) = await LoadRefundAsync(demandId, MatchMember(memberNo), brCode);
            if (rc == null) return error == null ? null : new EcsRefundVM { DemandId = demandId, BlockedReasons = { error } };

            var names = await _unitOfWork.EcsRecovery.GetOfficeNamesAsync(brCode);
            var vm = new EcsRefundVM
            {
                DemandId = demandId,
                DueDate = rc.Master.DemandDate!.Value.Date,
                MemId = rc.Member.MemId,
                MemberNo = rc.Member.MemberNo,
                PerNo = rc.Member.PerNo,
                MemberName = rc.Member.MemberName,
                OfficeName = names.GetValueOrDefault(rc.Member.OfficeId),
                VocNo = rc.Receipts.Vouchers.FirstOrDefault()?.Voc_No,
                BlockedReasons = rc.Blockers.ToList()
            };
            if (rc.ExcessLedId != 0)
                vm.ExcessLedgerName = (await _unitOfWork.EcsRecovery.GetLedgersAsync(brCode)).FirstOrDefault(l => l.LedId == rc.ExcessLedId)?.LedName;

            var loanLabels = await _unitOfWork.EcsRecovery.GetLoanLabelsAsync(rc.Receipts.LoanTrns.Select(t => t.Loan_Id), brCode);
            var depNames = await _unitOfWork.EcsRecovery.GetDepositNamesAsync(brCode);
            var rdNos = await _unitOfWork.EcsRecovery.GetRdNosAsync(rc.Receipts.RdTrns.Select(t => t.TD_Id), brCode);
            foreach (var t in rc.Receipts.LoanTrns.Where(t => t.PrlColl_Amt + t.IntColl_Amt + t.PIColl_Amt > 0))
                vm.Lines.Add(new EcsRefundLineVM { Kind = "L", TrnId = t.Trn_Id, Account = loanLabels.GetValueOrDefault(t.Loan_Id) ?? $"Loan id {t.Loan_Id}",
                                                   Principal = t.PrlColl_Amt, Interest = t.IntColl_Amt, Penal = t.PIColl_Amt });
            foreach (var t in rc.Receipts.DepositTrns.Where(t => t.Receipt_Amount + t.Penal_Interest_Received_Amount > 0))
                vm.Lines.Add(new EcsRefundLineVM { Kind = "D", TrnId = t.Trn_Id, Account = depNames.GetValueOrDefault(t.Deposit_Id) ?? $"Deposit {t.Deposit_Id}",
                                                   Principal = t.Receipt_Amount, Penal = t.Penal_Interest_Received_Amount });
            foreach (var t in rc.Receipts.RdTrns.Where(t => t.DepositReceiptAmount + t.PenalReceivedAmount > 0))
                vm.Lines.Add(new EcsRefundLineVM { Kind = "R", TrnId = t.TDTrn_Id, Account = rdNos.GetValueOrDefault(t.TD_Id) ?? $"RD id {t.TD_Id}",
                                                   Principal = t.DepositReceiptAmount, Penal = t.PenalReceivedAmount });

            var lines = await _unitOfWork.EcsRecovery.GetMemberLinesAsync(demandId, rc.Member.MemId, brCode, forUpdate: false);
            vm.Excess = R(lines.Where(l => l.Demand_Type == "B").Sum(l => l.Creditor_Collection));
            if (vm.Lines.Count == 0 && vm.BlockedReasons.Count == 0) vm.BlockedReasons.Add("The member has no loan, deposit or RD receipt in this recovery.");
            vm.CanRefund = vm.BlockedReasons.Count == 0;
            return vm;
        }

        public async Task<EcsRecoveryResultVM> RefundAsync(EcsRefundRequestVM request, EcsUserContextVM ctx)
        {
            var brCode = ctx.BrCode;
            var guard = await CommonGuardsAsync(ctx);
            if (guard != null) return Fail(guard);

            _unitOfWork.BeginTransaction();
            try
            {
                var (rc, error) = await LoadRefundAsync(request.DemandId, m => m.MemId == request.MemId, brCode);
                if (rc == null) { _unitOfWork.RollBack(); return Fail(error ?? "The member was not found."); }
                if (rc.Blockers.Count > 0) { _unitOfWork.RollBack(); return Fail("The recovery cannot be refunded.", rc.Blockers); }

                var reasons = new List<string>();
                var wanted = request.Lines.Where(l => l.Principal != 0 || l.Interest != 0 || l.Penal != 0).ToList();
                if (wanted.Count == 0) { _unitOfWork.RollBack(); return Fail("Enter the amount to refund."); }
                bool Bad(double asked, double have) => asked < 0 || R(asked) - R(have) > Tolerance;

                // ----- check every amount against its receipt -----
                foreach (var w in wanted)
                {
                    switch (w.Kind)
                    {
                        case "L":
                            var lt = rc.Receipts.LoanTrns.FirstOrDefault(t => t.Trn_Id == w.TrnId);
                            if (lt == null) reasons.Add("A loan receipt is not part of this recovery.");
                            else if (Bad(w.Principal, lt.PrlColl_Amt) || Bad(w.Interest, lt.IntColl_Amt) || Bad(w.Penal, lt.PIColl_Amt))
                                reasons.Add($"{w.Account ?? "Loan"}: the refund cannot exceed what was recovered.");
                            break;
                        case "D":
                            var dt = rc.Receipts.DepositTrns.FirstOrDefault(t => t.Trn_Id == w.TrnId);
                            if (dt == null) reasons.Add("A deposit receipt is not part of this recovery.");
                            else if (w.Interest != 0 || Bad(w.Principal, dt.Receipt_Amount) || Bad(w.Penal, dt.Penal_Interest_Received_Amount))
                                reasons.Add($"{w.Account ?? "Deposit"}: the refund cannot exceed what was recovered.");
                            break;
                        case "R":
                            var rt = rc.Receipts.RdTrns.FirstOrDefault(t => t.TDTrn_Id == w.TrnId);
                            if (rt == null) reasons.Add("An RD receipt is not part of this recovery.");
                            else if (w.Interest != 0 || Bad(w.Principal, rt.DepositReceiptAmount) || Bad(w.Penal, rt.PenalReceivedAmount))
                                reasons.Add($"{w.Account ?? "RD"}: the refund cannot exceed what was recovered.");
                            break;
                        default:
                            reasons.Add("Unknown receipt kind.");
                            break;
                    }
                }
                if (reasons.Count > 0) { _unitOfWork.RollBack(); return Fail("The refund cannot be saved.", reasons.Distinct().ToList()); }

                // ----- ledgers to take the refund off -----
                var loanIds = rc.Receipts.LoanTrns.Select(t => t.Loan_Id).Distinct().ToList();
                var loans = await _unitOfWork.EcsRecovery.GetLoansAsync(loanIds, brCode);
                var schemes = await _unitOfWork.EcsRecovery.GetSchemesAsync(loans.Values.Select(l => l.SchemeId), brCode);
                var depositLedgers = await _unitOfWork.EcsRecovery.GetDepositLedgersAsync(brCode);
                var rdLedgers = await _unitOfWork.EcsRecovery.GetRdLedgersAsync(rc.Receipts.RdTrns.Select(t => t.TD_Id), brCode);
                var debitOff = new Dictionary<decimal, double>();   // ledger -> receipt credit to remove
                void Off(decimal ledId, double amount)
                {
                    if (amount <= 0) return;
                    debitOff[ledId] = R(debitOff.GetValueOrDefault(ledId) + amount);
                }

                var lines = await _unitOfWork.EcsRecovery.GetMemberLinesAsync(request.DemandId, rc.Member.MemId, brCode, forUpdate: true);
                double total = 0;
                foreach (var w in wanted)
                {
                    double p = R(w.Principal), i = R(w.Interest), pi = R(w.Penal);
                    switch (w.Kind)
                    {
                        case "L":
                            var lt = rc.Receipts.LoanTrns.First(t => t.Trn_Id == w.TrnId);
                            var scheme = loans.TryGetValue(lt.Loan_Id, out var ln) ? schemes.GetValueOrDefault(ln.SchemeId) : null;
                            if (scheme == null) { reasons.Add($"Loan id {lt.Loan_Id}: scheme not found."); break; }
                            lt.PrlColl_Amt = R(lt.PrlColl_Amt - p);
                            lt.IntColl_Amt = R(lt.IntColl_Amt - i);
                            lt.PIColl_Amt = R(lt.PIColl_Amt - pi);
                            Off(scheme.PrlLed_Id, p); Off(scheme.IntLed_Id, i); Off(scheme.PILed_Id, pi);
                            var ll = lines.FirstOrDefault(l => l.Demand_Type is "L" or "A" && l.Loan_Id == lt.Loan_Id);
                            if (ll != null)
                            {
                                (ll.Loan_Prl_Current_Collection, ll.Loan_Prl_Arrear_Collection) = Unwind(ll.Loan_Prl_Current_Collection, ll.Loan_Prl_Arrear_Collection, p);
                                (ll.Loan_Int_Current_Collection, ll.Loan_Int_Arrear_Collection) = Unwind(ll.Loan_Int_Current_Collection, ll.Loan_Int_Arrear_Collection, i);
                                (ll.Loan_PI_Current_Collection, ll.loan_PI_Arrear_Collection) = Unwind(ll.Loan_PI_Current_Collection, ll.loan_PI_Arrear_Collection, pi);
                                EcsDemandAppropriator.Total(ll);
                            }
                            break;
                        case "D":
                            var dt = rc.Receipts.DepositTrns.First(t => t.Trn_Id == w.TrnId);
                            var (depLed, depPiLed) = depositLedgers.GetValueOrDefault(dt.Deposit_Id);
                            dt.Receipt_Amount = R(dt.Receipt_Amount - p);
                            dt.Penal_Interest_Received_Amount = R(dt.Penal_Interest_Received_Amount - pi);
                            Off(depLed, p); Off(depPiLed, pi);
                            var dl = lines.FirstOrDefault(l => l.Demand_Type == "D" && l.Deposit_Id == dt.Deposit_Id);
                            if (dl != null)
                            {
                                (dl.Deposit_Current_Collection, dl.Deposit_Arrear_Collection) = Unwind(dl.Deposit_Current_Collection, dl.Deposit_Arrear_Collection, p);
                                (dl.Deposit_Current_PI_Collection, dl.Deposit_Arrear_PI_Collection) = Unwind(dl.Deposit_Current_PI_Collection, dl.Deposit_Arrear_PI_Collection, pi);
                                EcsDemandAppropriator.Total(dl);
                            }
                            break;
                        case "R":
                            var rt = rc.Receipts.RdTrns.First(t => t.TDTrn_Id == w.TrnId);
                            rt.DepositReceiptAmount = R(rt.DepositReceiptAmount - p);
                            rt.PenalReceivedAmount = R(rt.PenalReceivedAmount - pi);
                            if (rt.InstalmentAmount > 0) rt.NoOfInstalments = (int)Math.Round(rt.DepositReceiptAmount / rt.InstalmentAmount);
                            Off(rdLedgers.GetValueOrDefault(rt.TD_Id), p + pi);
                            var rl = lines.FirstOrDefault(l => l.Demand_Type == "R" && l.TD_Id == rt.TD_Id);
                            if (rl != null)
                            {
                                rl.RD_Receipt = R(Math.Max(0, rl.RD_Receipt - p));
                                rl.RD_PI_Receipt = R(Math.Max(0, rl.RD_PI_Receipt - pi));
                                EcsDemandAppropriator.Total(rl);
                            }
                            break;
                    }
                    total = R(total + p + i + pi);
                }

                // ----- the voucher: receipts move to the excess ledger, the debit side is unchanged -----
                var vocLines = rc.Receipts.VoucherTrns;
                foreach (var (ledId, amount) in debitOff)
                {
                    double left = amount;
                    foreach (var v in vocLines.Where(v => v.Led_Id == ledId && v.Voc_Rpt > 0))
                    {
                        double take = Math.Min(left, v.Voc_Rpt);
                        v.Voc_Rpt = R(v.Voc_Rpt - take);
                        left = R(left - take);
                        if (left <= 0) break;
                    }
                    if (left > Tolerance) reasons.Add($"The voucher has no receipt of {amount:N2} on ledger {ledId}.");
                }
                if (reasons.Count > 0) { _unitOfWork.RollBack(); return Fail("The refund cannot be saved.", reasons.Distinct().ToList()); }

                var alloc = await _unitOfWork.EcsRecovery.CreateIdAllocatorAsync(brCode);
                var zero = alloc.ZeroId;
                await alloc.PrimeSerialsAsync(Array.Empty<decimal>(), new[] { rc.Member.MemId }, Array.Empty<decimal>());
                var posting = new EcsRecoverySavePosting();
                var excessLine = vocLines.FirstOrDefault(v => v.Led_Id == rc.ExcessLedId && v.Voc_Pmt == 0);
                if (excessLine != null) excessLine.Voc_Rpt = R(excessLine.Voc_Rpt + total);
                else
                {
                    var tmpl = vocLines.First();
                    posting.VoucherTrns.Add(new Fin_Voucher_Trn
                    {
                        Voc_Trn_Id = alloc.NextVocTrnId(),
                        Voc_Id = rc.VocId,
                        Led_Id = rc.ExcessLedId,
                        Voc_Rpt = total,
                        Voc_Pmt = 0,
                        Voc_Trn_Type = VocTrnTypeAdjustment,
                        Voc_Narr = tmpl.Voc_Narr,
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
                    });
                }

                var due = rc.Master.DemandDate!.Value.Date;
                posting.MemTrns.Add(MemTrn(2, rc.Member.MemId, rc.ExcessLedId, due, total, rc.VocId, zero, alloc, ctx));

                int recoveryId = lines.Where(l => l.Recovery_Id > 0).Select(l => l.Recovery_Id).DefaultIfEmpty(0).Max();
                var b = NewLine("B", rc.Member.MemId, request.DemandId, recoveryId, DateOnly.FromDateTime(ctx.CurrentDate.Date), zero, alloc, ctx);
                b.Creditor_Led_Id = rc.ExcessLedId;
                b.Creditor_Collection = total;
                b.Voc_Id = rc.VocId;
                b.Is_Recovery_Saved = true;
                EcsDemandAppropriator.Total(b);

                await _unitOfWork.EcsRecovery.AddLinesAsync(new[] { b });
                await _unitOfWork.EcsRecovery.AddSaveAsync(posting);
                await _unitOfWork.CompleteAsync();
                _unitOfWork.CommitTransaction();
                return new EcsRecoveryResultVM
                {
                    Success = true,
                    VocNo = rc.Receipts.Vouchers.FirstOrDefault()?.Voc_No,
                    Message = $"{total:N2} of member {rc.Member.MemberNo}'s recovery moved to the excess recovery ledger (voucher {rc.Receipts.Vouchers.FirstOrDefault()?.Voc_No})."
                };
            }
            catch (Exception ex)
            {
                _unitOfWork.RollBack();
                throw Wrap(ex, "Nothing was refunded.");
            }
        }

        /// <summary>Takes a refund off the current collection first, then the arrear (the reverse of the split).</summary>
        private static (double current, double arrear) Unwind(double current, double arrear, double refund)
        {
            double fromCurrent = Math.Min(Math.Max(0, current), refund);
            double fromArrear = Math.Min(Math.Max(0, arrear), refund - fromCurrent);
            return (R(current - fromCurrent), R(arrear - fromArrear));
        }

        // =====================================================================
        // Revised demand inputs
        // =====================================================================

        /// <summary>A loan line's fresh accrual to its receipt date (spec 13a §4.2), stamped on the R row at save (§5.1).</summary>
        private sealed record FreshAccrual(DateTime ReceiptDate, double Interest, DateTime IntCalcDate, double Penal, DateTime? PICalcDate);

        private sealed class RevisionInputs
        {
            public DateTime DueDate { get; set; }
            public Dictionary<decimal, EcsRecoveryLoanRow> Loans { get; set; } = new();
            public Dictionary<decimal, EcsAppropriationLoan> AppropriationLoans { get; set; } = new();
            public List<EcsRecoveryLoanRow> OpenLoans { get; set; } = new();
            public Dictionary<(decimal, decimal), double> DueTo { get; set; } = new();
            /// <summary>Per L loan: interest / penal booked by this demand's own D row(s).</summary>
            public Dictionary<decimal, (double Int, double PI)> DemandCalc { get; set; } = new();
            public Dictionary<decimal, FreshAccrual> Fresh { get; set; } = new();
            public Dictionary<(decimal MemId, int DepositId), EcsRecoveryDepositRow> Deposits { get; set; } = new();
            public Dictionary<decimal, EcsRecoveryRdRow> Rds { get; set; } = new();
            public Dictionary<decimal, EcsRecoveryMemberRow> Members { get; set; } = new();
        }

        /// <summary>
        /// Everything the revised demand as on the recovery date needs (spec 13a §4): loan ledgers with the fresh accrual
        /// to each loan's receipt date, deposit and RD sums, due-to balances. Ledger sums cover every valid row, all dates.
        /// </summary>
        private async Task<RevisionInputs> LoadRevisionInputsAsync(List<Mem_Demand> lines, IEnumerable<decimal> recoveringMembers,
            decimal demandId, DateTime dueDate, string brCode)
        {
            var due = dueDate.Date;
            var loanIds = lines.Where(l => l.Demand_Type == "L").Select(l => l.Loan_Id).Distinct().ToList();
            var inputs = new RevisionInputs
            {
                DueDate = due,
                Loans = await _unitOfWork.EcsRecovery.GetLoansAsync(loanIds, brCode),
                OpenLoans = await _unitOfWork.EcsRecovery.GetOpenLoansOfMembersAsync(recoveringMembers, brCode),
                DueTo = await _unitOfWork.EcsRecovery.GetDueToBalancesAsync(lines.Where(l => l.Demand_Type == "S").Select(l => l.Mem_Id), brCode),
                Deposits = await _unitOfWork.EcsRecovery.GetDepositRevisionAsync(lines.Where(l => l.Demand_Type == "D").Select(l => l.Mem_Id), due, brCode),
                Rds = await _unitOfWork.EcsRecovery.GetRdRevisionAsync(lines.Where(l => l.Demand_Type == "R").Select(l => l.TD_Id), due, brCode),
                Members = (await _unitOfWork.EcsRecovery.GetMembersByIdAsync(lines.Select(l => l.Mem_Id), brCode))
                    .GroupBy(m => m.MemId).ToDictionary(g => g.Key, g => g.First())
            };
            inputs.AppropriationLoans = inputs.Loans.ToDictionary(kv => kv.Key, kv => ToAppropriationLoan(kv.Value));

            var accrual = await _unitOfWork.EcsRecovery.GetAccrualInputsAsync(loanIds, brCode);
            foreach (var loanId in loanIds)
            {
                if (!accrual.TryGetValue(loanId, out var acc)) continue;
                var rows = acc.Trns;
                var mine = rows.Where(t => t.Demand_Id == demandId && t.Trn_Status == "D").ToList();
                inputs.DemandCalc[loanId] = (R(mine.Sum(t => t.IntCalc_Amt)), R(mine.Sum(t => t.PICalc_Amt)));

                // Receipt date (§5, decision Q3): the due date, or later when the loan was sanctioned or has a row after it.
                var sanDate = inputs.Loans.TryGetValue(loanId, out var ln) ? ln.SanDate?.Date : null;
                var receiptDate = ReceiptDate(due, sanDate, rows.Count > 0 ? rows.Max(t => t.Trn_Date) : null);

                DateTime? disbDate = rows.Where(t => t.Trn_Status == "I" && t.Disb_Date.HasValue).Min(t => t.Disb_Date)
                                     ?? rows.Where(t => t.Disb_Amt > 0).Select(t => (DateTime?)t.Trn_Date).Min();
                DateTime? lastInt = rows.Max(t => t.IntCalc_Date) ?? disbDate;
                DateTime? lastPi = rows.Max(t => t.PICalc_Date) ?? disbDate;
                var deltas = rows.Select(t => new EcsTrnDelta(t.Trn_Date, t.Disb_Amt, t.Prl_Dem, t.PrlColl_Amt, t.IntCalc_Amt, t.IntColl_Amt)).ToList();
                var rates = acc.Rates.Select(r => new EcsRateEvent(r.Roi_Wef, r.Roi, r.Pi)).ToList();

                // An expired member's interest stops at the expiry date, as in the demand (spec 12).
                var member = ln != null ? inputs.Members.GetValueOrDefault(ln.MemId) : null;
                var stop = EcsDemandCalculator.InterestStopDate(member?.IsExpired ?? false, member?.ExpiredDate);
                var appDate = stop.HasValue && stop.Value < receiptDate ? stop.Value : receiptDate;

                double freshInt = lastInt.HasValue
                    ? EcsInterestCalculator.Calculate(EcsAccrualHead.Interest, acc.IntApplication, deltas, rates, acc.Roi, acc.Pi, lastInt.Value, appDate).Amount
                    : 0;
                bool penal = EcsInterestCalculator.PenalApplies(acc.PIApplication);
                double freshPi = penal && lastPi.HasValue
                    ? EcsInterestCalculator.Calculate(EcsAccrualHead.Penal, acc.PIApplication, deltas, rates, acc.Roi, acc.Pi, lastPi.Value, appDate).Amount
                    : 0;
                inputs.Fresh[loanId] = new FreshAccrual(
                    receiptDate,
                    freshInt,
                    Max(receiptDate, lastInt),
                    freshPi,
                    penal ? Max(receiptDate, lastPi) : null);
            }
            return inputs;
        }

        /// <summary>A loan receipt's date: the due date, never before the sanction or the loan's last transaction.</summary>
        private static DateTime ReceiptDate(DateTime due, DateTime? sanDate, DateTime? lastTrn) =>
            new[] { due.Date, sanDate?.Date ?? due.Date, lastTrn?.Date ?? due.Date }.Max();

        private static DateTime Max(DateTime a, DateTime? b) => b.HasValue && b.Value.Date > a.Date ? b.Value.Date : a.Date;

        private static void ReviseLine(Mem_Demand l, RevisionInputs inputs)
        {
            var input = new EcsLineRevision();
            switch (l.Demand_Type)
            {
                case "L":
                    if (inputs.Loans.TryGetValue(l.Loan_Id, out var ln))
                    {
                        var calc = inputs.DemandCalc.GetValueOrDefault(l.Loan_Id);
                        var fresh = inputs.Fresh.GetValueOrDefault(l.Loan_Id);
                        input = input with
                        {
                            Loan = new EcsLoanRevision(new EcsLoanBalance(ln.Disb, ln.PrlColl, ln.IntCalc, ln.IntColl, ln.PICalc, ln.PIColl),
                                calc.Int, calc.PI, fresh?.Interest ?? 0, fresh?.Penal ?? 0)
                        };
                    }
                    break;
                case "D":
                    bool memberClosed = !(inputs.Members.GetValueOrDefault(l.Mem_Id)?.IsOpen ?? false);
                    var d = inputs.Deposits.GetValueOrDefault((l.Mem_Id, l.Deposit_Id));
                    // Closed: the member has left, or a payment since the due date took the deposit to nil.
                    bool closed = memberClosed || (d != null && d.SincePaid > 0 && d.AllReceipt - d.AllPaid <= Tolerance);
                    input = input with
                    {
                        Deposit = d == null
                            ? new EcsDepositRevision(0, 0, 0, 0, 0, 0, closed)
                            : new EcsDepositRevision(d.BeforeDemand, d.BeforeReceipt, d.BeforePiCalc, d.BeforePiReceived,
                                d.SinceReceipt, d.SincePiReceived, closed)
                    };
                    // No ledger row at all: keep the demanded figures (e.g. the first demand of a new member).
                    if (d == null && !closed) input = input with { Deposit = null };
                    break;
                case "R":
                    if (inputs.Rds.TryGetValue(l.TD_Id, out var rd))
                    {
                        bool rdClosed = rd.Closed || rd.Paid > 0;
                        var figures = rdClosed ? null : EcsDemandCalculator.Rd(
                            new EcsDemandRd(rd.TdId, rd.ValueDate, rd.PeriodInMonths, rd.Instalment, rd.Received,
                                rd.PenalCalcBeforeDue, rd.PenalReceived, rd.PenalRate), inputs.DueDate);
                        input = input with { Rd = new EcsRdRevision(figures, rdClosed) };
                    }
                    break;
                case "S":
                    input = input with { DueTo = inputs.DueTo.GetValueOrDefault((l.Mem_Id, l.Debtor_Led_Id)) };
                    break;
            }
            EcsDemandAppropriator.Revise(l, input);
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

        /// <summary>Spec 13a §6: the caller appropriated this office and is not Admin, so may not check it.</summary>
        private static bool IsOwn(Mem_Demand_Status s, EcsUserContextVM ctx) =>
            s.Is_Appropriated && s.Appropriated_By == ctx.UsrId && !ctx.IsAdmin;

        private Task<Dictionary<decimal, string>> UserNamesAsync(IEnumerable<Mem_Demand_Status> statuses, string brCode) =>
            _unitOfWork.EcsRecovery.GetUserNamesAsync(
                statuses.SelectMany(s => new[] { s.Appropriated_By, s.Rejected_By }).Where(x => x.HasValue).Select(x => x!.Value), brCode);

        private static EcsRecoveryOfficeVM ToOfficeVM(Mem_Demand_Status s, EcsRecoveryOfficeStatRow? stat,
            Dictionary<decimal, string> names, Dictionary<decimal, string?> vocNos, Dictionary<decimal, string> users) => new()
        {
            AppropriatedBy = s.Is_Appropriated ? s.Appropriated_By : null,
            AppropriatedByName = s.Is_Appropriated && s.Appropriated_By is { } by ? users.GetValueOrDefault(by) ?? $"User {by}" : null,
            AppropriatedOn = s.Is_Appropriated ? s.Appropriated_On : null,
            RejectReason = s.Is_Appropriated ? null : s.Reject_Reason,
            RejectedByName = !s.Is_Appropriated && s.Rejected_By is { } rb ? users.GetValueOrDefault(rb) ?? $"User {rb}" : null,
            RejectedOn = s.Is_Appropriated ? null : s.Rejected_On,
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
