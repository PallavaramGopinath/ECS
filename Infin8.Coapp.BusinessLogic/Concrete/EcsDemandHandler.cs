using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using Infin8.Coapp.Utility;
using Infin8.Coapp.Utility.Enums;

namespace Infin8.Coapp.BusinessLogic
{
    /// <summary>
    /// ECS Demand Calculation (spec 12). The rules live in the pure <see cref="EcsDemandCalculator"/>; this class
    /// loads the inputs, applies the guards (§7) and writes mem_demand_* (calculate) or the ledgers (post).
    /// </summary>
    public class EcsDemandHandler : IEcsDemandHandler
    {
        private const double Tolerance = 0.005;
        private static readonly DateTime NoDate = new(1900, 1, 1);

        readonly IUnitOfWork _unitOfWork;

        public EcsDemandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // =====================================================================
        // State / preview
        // =====================================================================

        public async Task<EcsDemandStateVM> GetStateAsync(EcsUserContextVM ctx)
        {
            var state = new EcsDemandStateVM { BusinessDay = ctx.CurrentDate.Date };
            var dayError = await CheckBusinessDayAsync(ctx);
            state.IsOpenBusinessDay = dayError == null;
            state.Message = dayError;

            var masters = await _unitOfWork.EcsDemand.GetLiveMastersAsync(ctx.BrCode, 12);
            var totals = await _unitOfWork.EcsDemand.GetTotalsAsync(masters.Select(m => m.Demand_Id), ctx.BrCode);
            state.History = masters.Select(m => ToMasterVM(m, totals)).ToList();

            var latest = masters.FirstOrDefault();
            state.NextDueDate = NextDueDate(latest, ctx.CurrentDate);

            if (latest != null)
            {
                state.Current = state.History[0];
                state.CurrentSummary = await GetSavedSummaryAsync(latest.Demand_Id, ctx.BrCode, withMembers: false);
                if (latest.IsDemandUpdated)
                    state.DeletePostedBlockedReasons = await DeletePostedBlockersAsync(latest, ctx.BrCode);
                state.CanDeletePosted = latest.IsDemandUpdated && state.DeletePostedBlockedReasons.Count == 0;
            }

            var blockers = await CalculateBlockersAsync(state.NextDueDate, ctx.BrCode, masters);
            state.CanCalculate = blockers.Count == 0 && state.IsOpenBusinessDay;
            state.CalculateBlockedReason = dayError ?? blockers.FirstOrDefault();
            return state;
        }

        public async Task<EcsDemandSummaryVM> PreviewAsync(DateTime dueDate, EcsUserContextVM ctx)
        {
            var due = dueDate.Date;
            var build = await CalculateAsync(due, ctx.CurrentDate, ctx.UsrId, ctx.YrId, ctx.BrCode);
            var officeNames = await _unitOfWork.EcsRecovery.GetOfficeNamesAsync(ctx.BrCode);
            var summary = Summarise(build.Lines, build.Members, build.Labels, officeNames, withMembers: true);
            summary.DueDate = due;
            summary.CalculationDate = ctx.CurrentDate.Date;

            var dayError = await CheckBusinessDayAsync(ctx);
            if (dayError != null) summary.Messages.Add(dayError);
            var masters = await _unitOfWork.EcsDemand.GetLiveMastersAsync(ctx.BrCode, 1000);
            summary.Messages.AddRange(await CalculateBlockersAsync(due, ctx.BrCode, masters));
            if (build.Lines.Count == 0) summary.Messages.Add("Nothing to demand for this due date.");
            summary.Warnings.AddRange(build.Warnings);
            if (due <= ctx.CurrentDate.Date)
                summary.Warnings.Add($"The due date is not after the business day ({ctx.CurrentDate:dd-MM-yyyy}).");
            summary.CanSave = summary.Messages.Count == 0;
            return summary;
        }

        public Task<EcsDemandSummaryVM?> GetSavedSummaryAsync(decimal demandId, string brCode) =>
            GetSavedSummaryAsync(demandId, brCode, withMembers: true);

        private async Task<EcsDemandSummaryVM?> GetSavedSummaryAsync(decimal demandId, string brCode, bool withMembers)
        {
            var master = (await _unitOfWork.EcsDemand.GetLiveMastersAsync(brCode, 1000)).FirstOrDefault(m => m.Demand_Id == demandId);
            if (master == null) return null;
            var lines = await _unitOfWork.EcsDemand.GetLinesAsync(demandId, brCode, forUpdate: false);
            var members = (await _unitOfWork.EcsDemand.GetMembersAsync(brCode)).ToDictionary(m => m.MemId);
            var labels = withMembers ? await _unitOfWork.EcsDemand.GetAccountLabelsAsync(lines, brCode) : new();

            var officeNames = await _unitOfWork.EcsRecovery.GetOfficeNamesAsync(brCode);
            var summary = Summarise(lines, members, labels, officeNames, withMembers);
            summary.DemandId = demandId;
            summary.DueDate = master.DemandDate?.Date ?? DateTime.MinValue;
            summary.CalculationDate = master.DemandCalculationDate?.Date ?? DateTime.MinValue;
            return summary;
        }

        // =====================================================================
        // Save calculation
        // =====================================================================

        public async Task<EcsDemandResultVM> SaveCalculationAsync(DateTime dueDate, EcsUserContextVM ctx)
        {
            var due = dueDate.Date;
            var guard = await CommonGuardsAsync(ctx);
            if (guard != null) return Fail(guard);

            _unitOfWork.BeginTransaction();
            try
            {
                var masters = await _unitOfWork.EcsDemand.GetLiveMastersAsync(ctx.BrCode, 1000);
                var blockers = await CalculateBlockersAsync(due, ctx.BrCode, masters);
                if (blockers.Count > 0)
                {
                    _unitOfWork.RollBack();
                    return Fail("The demand cannot be calculated.", blockers);
                }

                var demandId = await AddCalculationAsync(due, ctx.CurrentDate.Date, ctx.UsrId, ctx.YrId, ctx.BrCode);
                if (demandId == null)
                {
                    _unitOfWork.RollBack();
                    return Fail("Nothing to demand for this due date.");
                }

                await _unitOfWork.CompleteAsync();
                _unitOfWork.CommitTransaction();
                return new EcsDemandResultVM
                {
                    Success = true,
                    DemandId = demandId.Value,
                    Message = $"Demand for {due:dd-MM-yyyy} calculated and saved."
                };
            }
            catch (Exception ex)
            {
                _unitOfWork.RollBack();
                throw Wrap(ex, "Nothing was saved.");
            }
        }

        /// <summary>Calculates and adds master, lines and status rows to the change tracker. Null when no line.</summary>
        private async Task<decimal?> AddCalculationAsync(DateTime due, DateTime calcDate, decimal usrId, decimal yrId, string brCode)
        {
            var build = await CalculateAsync(due, calcDate, usrId, yrId, brCode);
            if (build.Lines.Count == 0) return null;

            var alloc = await _unitOfWork.EcsDemand.CreateIdAllocatorAsync(brCode);
            var zero = alloc.ZeroId;
            var master = new Mem_Demand_Master
            {
                Demand_Id = alloc.NextDemandId(),
                DemandDate = due,
                DemandCalculationDate = calcDate.Date,
                NextDemandDate = EcsDemandCalculator.NextDemandDate(due),
                IsDemandUpdated = false,
                IsRecoveryAppropriated = false,
                IsRecoveryUpdated = false,
                RecoveryUpdatedVoc_Id = zero,
                DemandMaster_Delete = false,
                Usr_Id = usrId,
                Yr_Id = yrId,
                BrCode = brCode,
                Voc_Status = "V"
            };

            foreach (var line in build.Lines)
            {
                line.Id = alloc.NextLineId();
                line.Demand_Id = master.Demand_Id;
            }

            var offices = build.Lines.Select(l => build.Members.TryGetValue(l.Mem_Id, out var m) ? m.OfficeId : zero)
                                     .Distinct().OrderBy(o => o).ToList();
            var statuses = offices.Select(o => new Mem_Demand_Status
            {
                Id = alloc.NextStatusId(),
                Demand_Id = master.Demand_Id,
                Office_Id = o,
                Is_Appropriated = false,
                Is_Recovery_Saved = false,
                is_Active = true,
                Voc_Id = zero,
                Usr_Id = usrId,
                Yr_Id = yrId,
                BrCode = brCode
            }).ToList();

            await _unitOfWork.EcsDemand.AddCalculationAsync(master, build.Lines, statuses);
            return master.Demand_Id;
        }

        // =====================================================================
        // Post
        // =====================================================================

        public async Task<EcsDemandResultVM> PostAsync(decimal demandId, EcsUserContextVM ctx)
        {
            var guard = await CommonGuardsAsync(ctx);
            if (guard != null) return Fail(guard);

            _unitOfWork.BeginTransaction();
            try
            {
                var master = await _unitOfWork.EcsDemand.GetMasterForUpdateAsync(demandId, ctx.BrCode);
                if (master == null || master.DemandDate == null)
                {
                    _unitOfWork.RollBack();
                    return Fail("The demand no longer exists.");
                }
                if (master.IsDemandUpdated)
                {
                    _unitOfWork.RollBack();
                    return Fail("The demand is already posted.");
                }

                var due = master.DemandDate.Value.Date;
                var saved = await _unitOfWork.EcsDemand.GetLinesAsync(demandId, ctx.BrCode, forUpdate: false);

                // Recalculate and compare the current figures: a transaction posted after the calculation (e.g. a
                // recovery that moved IntCalc_Date) would otherwise be demanded twice.
                var fresh = await CalculateAsync(due, ctx.CurrentDate, ctx.UsrId, ctx.YrId, ctx.BrCode);
                var changes = CompareCurrent(saved, fresh.Lines, fresh.Labels, fresh.Members);
                if (changes.Count > 0)
                {
                    _unitOfWork.RollBack();
                    return Fail("The ledgers have changed since this demand was calculated. Delete the calculation and calculate it again.",
                        changes.Take(20).Concat(changes.Count > 20 ? new[] { $"…and {changes.Count - 20} more." } : Array.Empty<string>()).ToList());
                }

                var alloc = await _unitOfWork.EcsDemand.CreateIdAllocatorAsync(ctx.BrCode);
                var zero = alloc.ZeroId;
                await alloc.PrimeSerialsAsync(
                    saved.Where(l => l.Demand_Type == "L").Select(l => l.Loan_Id),
                    saved.Where(l => l.Demand_Type == "D").Select(l => l.Mem_Id),
                    saved.Where(l => l.Demand_Type == "R").Select(l => l.TD_Id));

                var loanTrns = new List<Loan_Trn>();
                foreach (var l in saved.Where(l => l.Demand_Type == "L"
                             && l.Loan_Prl_Current + l.Loan_Int_Current + l.Loan_PI_Current > 0))
                {
                    bool penalApplies = fresh.Loans.TryGetValue(l.Loan_Id, out var loan)
                                        && EcsInterestCalculator.PenalApplies(loan.PIApplication);
                    loanTrns.Add(new Loan_Trn
                    {
                        Trn_Id = alloc.NextLoanTrnId(),
                        Loan_Id = l.Loan_Id,
                        Demand_Id = demandId,
                        Trn_Status = "D",
                        Trn_Date = due,
                        Due_Date = due,
                        Prl_Sched = l.Loan_Prl_Current,
                        Prl_Dem = l.Loan_Prl_Current,
                        IntCalc_Amt = l.Loan_Int_Current,
                        IntCalc_Date = due,                       // always, even when 0 (spec 12 §5)
                        PICalc_Amt = penalApplies ? l.Loan_PI_Current : 0,
                        PICalc_Date = penalApplies ? due : null,
                        Trn_SlNo = alloc.NextLoanTrnSlNo(l.Loan_Id),
                        TrnTr_Oe = false,
                        TrnTr_Delete = false,
                        Voc_Id = zero,
                        Usr_Id = ctx.UsrId,
                        Yr_Id = ctx.YrId,
                        BrCode = ctx.BrCode,
                        Voc_Status = "V"
                    });
                }

                var depositTrns = saved.Where(l => l.Demand_Type == "D" && l.Deposit_Current + l.Deposit_PI_Current > 0)
                    .Select(l => new Deposit_Trn
                    {
                        Trn_Id = alloc.NextDepositTrnId(),
                        Deposit_Id = l.Deposit_Id,
                        Mem_Id = l.Mem_Id,
                        Demand_Id = demandId,
                        Status = "D",
                        Trn_Date = due,
                        Due_Date = due,
                        Demand_Amount = l.Deposit_Current,
                        Penal_Interest_Calculated_Amount = l.Deposit_PI_Current,
                        Penal_Interest_Calculated_Date = l.Deposit_PI_Current > 0 ? due : null,
                        Is_Opening_Entry = false,
                        Is_Active = true,
                        Trn_SlNo = alloc.NextDepositTrnSlNo(l.Mem_Id, l.Deposit_Id),
                        Voc_Id = zero,
                        Usr_Id = ctx.UsrId,
                        Yr_Id = ctx.YrId,
                        BrCode = ctx.BrCode
                    }).ToList();

                var rdTrns = saved.Where(l => l.Demand_Type == "R" && l.RD_Demand + l.RD_PI_Calc > 0)
                    .Select(l => new TermDeposit_Trn
                    {
                        TDTrn_Id = alloc.NextRdTrnId(),
                        Trn_Date = due,
                        TD_Id = l.TD_Id,
                        DepositDemandAmount = l.RD_Demand,
                        PenalCalculatedAmount = l.RD_PI_Calc,
                        PenalAppliedDate = l.RD_PI_Calc > 0 ? due : null,
                        NoOfInstalments = l.RD_NoOf_Instalments,
                        InstalmentAmount = (int)Math.Round(l.RD_Instalment, MidpointRounding.AwayFromZero),
                        Demand_Id = demandId,
                        TDCalc_Id = 0,
                        TD_OE = false,
                        TD_Delete = false,
                        Trn_SlNo = alloc.NextRdTrnSlNo(l.TD_Id),
                        Voc_Id = zero,
                        Usr_Id = ctx.UsrId,
                        Yr_Id = ctx.YrId,
                        BrCode = ctx.BrCode,
                        Voc_Status = "V"
                    }).ToList();

                await _unitOfWork.EcsDemand.AddPostingAsync(loanTrns, depositTrns, rdTrns);
                master.IsDemandUpdated = true;

                await _unitOfWork.CompleteAsync();
                _unitOfWork.CommitTransaction();
                return new EcsDemandResultVM
                {
                    Success = true,
                    DemandId = demandId,
                    Message = $"Demand for {due:dd-MM-yyyy} posted: {loanTrns.Count} loan, {depositTrns.Count} deposit and {rdTrns.Count} RD rows."
                };
            }
            catch (Exception ex)
            {
                _unitOfWork.RollBack();
                throw Wrap(ex, "Nothing was posted.");
            }
        }

        // =====================================================================
        // Delete
        // =====================================================================

        public async Task<EcsDemandResultVM> DeleteCalculationAsync(decimal demandId, EcsUserContextVM ctx)
        {
            var guard = await CommonGuardsAsync(ctx);
            if (guard != null) return Fail(guard);

            _unitOfWork.BeginTransaction();
            try
            {
                var master = await _unitOfWork.EcsDemand.GetMasterForUpdateAsync(demandId, ctx.BrCode);
                if (master == null)
                {
                    _unitOfWork.RollBack();
                    return Fail("The demand no longer exists.");
                }
                if (master.IsDemandUpdated)
                {
                    _unitOfWork.RollBack();
                    return Fail("The demand is posted. Delete the posted demand first.");
                }

                master.DemandMaster_Delete = true;
                foreach (var l in await _unitOfWork.EcsDemand.GetLinesAsync(demandId, ctx.BrCode, forUpdate: true)) l.Is_Active = false;
                foreach (var s in await _unitOfWork.EcsDemand.GetStatusRowsForUpdateAsync(demandId, ctx.BrCode)) s.is_Active = false;

                await _unitOfWork.CompleteAsync();
                _unitOfWork.CommitTransaction();
                return new EcsDemandResultVM
                {
                    Success = true,
                    DemandId = demandId,
                    Message = $"Calculation for {master.DemandDate:dd-MM-yyyy} deleted."
                };
            }
            catch (Exception ex)
            {
                _unitOfWork.RollBack();
                throw Wrap(ex, "Nothing was deleted.");
            }
        }

        public async Task<EcsDemandResultVM> DeletePostedAsync(decimal demandId, EcsUserContextVM ctx)
        {
            var guard = await CommonGuardsAsync(ctx);
            if (guard != null) return Fail(guard);

            _unitOfWork.BeginTransaction();
            try
            {
                var master = await _unitOfWork.EcsDemand.GetMasterForUpdateAsync(demandId, ctx.BrCode);
                if (master == null)
                {
                    _unitOfWork.RollBack();
                    return Fail("The demand no longer exists.");
                }
                if (!master.IsDemandUpdated)
                {
                    _unitOfWork.RollBack();
                    return Fail("The demand is not posted.");
                }
                var blockers = await DeletePostedBlockersAsync(master, ctx.BrCode);
                if (blockers.Count > 0)
                {
                    _unitOfWork.RollBack();
                    return Fail("The posted demand cannot be deleted.", blockers);
                }

                var loans = await _unitOfWork.EcsDemand.GetPostedLoanTrnsAsync(demandId, ctx.BrCode);
                var deposits = await _unitOfWork.EcsDemand.GetPostedDepositTrnsAsync(demandId, ctx.BrCode);
                var rds = await _unitOfWork.EcsDemand.GetPostedRdTrnsAsync(demandId, ctx.BrCode);
                foreach (var t in loans) t.TrnTr_Delete = true;
                foreach (var t in deposits) t.Is_Active = false;
                foreach (var t in rds) t.TD_Delete = true;
                master.IsDemandUpdated = false;

                await _unitOfWork.CompleteAsync();
                _unitOfWork.CommitTransaction();
                return new EcsDemandResultVM
                {
                    Success = true,
                    DemandId = demandId,
                    Message = $"Posting of {master.DemandDate:dd-MM-yyyy} reversed ({loans.Count} loan, {deposits.Count} deposit, {rds.Count} RD rows). The calculation remains."
                };
            }
            catch (Exception ex)
            {
                _unitOfWork.RollBack();
                throw Wrap(ex, "Nothing was deleted.");
            }
        }

        // =====================================================================
        // Stop demand
        // =====================================================================

        public async Task<EcsStopDemandListVM> GetStopListAsync(DateTime dueDate, string brCode)
        {
            var due = dueDate.Date;
            var rows = await _unitOfWork.EcsDemand.GetStopRowsAsync(due, brCode);
            var officeNames = await _unitOfWork.EcsRecovery.GetOfficeNamesAsync(brCode);
            var locked = await StopListLockAsync(due, brCode);
            return new EcsStopDemandListVM
            {
                DueDate = due,
                LockedReason = locked,
                IsLocked = locked != null,
                Members = rows.Select(r => new EcsStopDemandMemberVM
                {
                    Id = r.Id,
                    MemId = r.MemId,
                    MemberNo = r.MemberNo,
                    MemberName = r.MemberName,
                    OfficeId = r.OfficeId,
                    OfficeName = officeNames.TryGetValue(r.OfficeId, out var n) ? n : r.OfficeId == 0 ? "—" : $"Office {r.OfficeId}",
                    IsInactive = r.MemberStatus == 2
                }).ToList()
            };
        }

        public async Task<EcsDemandResultVM> AddStopAsync(EcsStopDemandRequestVM request, EcsUserContextVM ctx)
        {
            var due = request.DueDate.Date;
            var guard = await CommonGuardsAsync(ctx) ?? await StopListLockAsync(due, ctx.BrCode);
            if (guard != null) return Fail(guard);
            if (string.IsNullOrWhiteSpace(request.MemberNo)) return Fail("Enter the member no.");

            var member = await _unitOfWork.EcsDemand.FindMemberAsync(request.MemberNo, ctx.BrCode);
            if (member == null) return Fail($"No open member {request.MemberNo.Trim()} in this branch.");
            if ((await _unitOfWork.EcsDemand.GetActiveStopRowsForUpdateAsync(member.MemId, due, ctx.BrCode)).Count > 0)
                return Fail($"Member {member.MemberNo} is already stopped for {due:dd-MM-yyyy}.");

            await _unitOfWork.EcsDemand.AddStopRowAsync(new Mem_Demand_Stop
            {
                Mem_Id = member.MemId,
                Demand_Date = DateOnly.FromDateTime(due),
                Is_Active = true,
                Voc_Id = decimal.TryParse(ctx.BrCode + "0000000", out var z) ? z : 0,
                Usr_Id = ctx.UsrId,
                Yr_Id = ctx.YrId,
                BrCode = ctx.BrCode
            });
            await _unitOfWork.CompleteAsync();

            var note = member.MemberStatus == 2 ? " (the member is inactive, so the demand was stopped anyway)" : "";
            return new EcsDemandResultVM
            {
                Success = true,
                Message = $"Demand stopped for member {member.MemberNo} {member.MemberName} for {due:dd-MM-yyyy}{note}."
            };
        }

        public async Task<EcsDemandResultVM> RemoveStopAsync(decimal stopId, EcsUserContextVM ctx)
        {
            var guard = await CommonGuardsAsync(ctx);
            if (guard != null) return Fail(guard);

            var row = await _unitOfWork.EcsDemand.GetStopRowForUpdateAsync(stopId, ctx.BrCode);
            if (row == null || !row.Is_Active) return Fail("The stop entry no longer exists.");
            var due = row.Demand_Date.ToDateTime(TimeOnly.MinValue);
            var locked = await StopListLockAsync(due, ctx.BrCode);
            if (locked != null) return Fail(locked);

            // Legacy data can hold several active rows for one member and date; clear them all.
            foreach (var r in await _unitOfWork.EcsDemand.GetActiveStopRowsForUpdateAsync(row.Mem_Id, due, ctx.BrCode))
                r.Is_Active = false;
            await _unitOfWork.CompleteAsync();
            return new EcsDemandResultVM { Success = true, Message = $"Stop removed for {due:dd-MM-yyyy}." };
        }

        /// <summary>A due date whose demand is already calculated cannot have its stop list changed.</summary>
        private async Task<string?> StopListLockAsync(DateTime due, string brCode)
        {
            var masters = await _unitOfWork.EcsDemand.GetLiveMastersAsync(brCode, 1000);
            var m = masters.FirstOrDefault(x => x.DemandDate?.Date == due.Date);
            if (m == null) return null;
            return m.IsDemandUpdated
                ? $"The demand for {due:dd-MM-yyyy} is already posted, so its stop list cannot change."
                : $"The demand for {due:dd-MM-yyyy} is already calculated. Delete the calculation to change its stop list.";
        }

        // =====================================================================
        // Delete demand for expired members (TSIS frmLoanDemandDeleteForExpiredMembers)
        // =====================================================================

        public async Task<EcsExpiredMemberVM?> GetExpiredMemberAsync(string memberNo, string brCode)
        {
            if (string.IsNullOrWhiteSpace(memberNo)) return null;
            var m = await _unitOfWork.EcsDemand.FindAnyMemberAsync(memberNo, brCode);
            if (m == null) return null;
            var vm = new EcsExpiredMemberVM
            {
                MemId = m.MemId,
                MemberNo = m.MemberNo,
                MemberName = m.MemberName,
                ExpiredDate = m.ExpiredDate?.Date,
                Message = ExpiredMemberProblem(m)
            };
            if (vm.Message == null)
                vm.Loans = (await _unitOfWork.EcsDemand.GetMemberLoansAsync(m.MemId, brCode)).Select(l => new EcsExpiredLoanVM
                {
                    LoanId = l.LoanId,
                    LoanNo = l.LoanNo,
                    SchemeName = l.SchemeName,
                    IsClosed = l.IsClosed
                }).ToList();
            return vm;
        }

        public async Task<EcsExpiredLoanRowsVM?> GetExpiredLoanRowsAsync(decimal loanId, string brCode)
        {
            var m = await _unitOfWork.EcsDemand.GetMemberOfLoanAsync(loanId, brCode);
            if (m == null || ExpiredMemberProblem(m) != null) return null;
            var expiry = m.ExpiredDate!.Value.Date;

            var rows = await _unitOfWork.EcsDemand.GetExpiredWindowRowsAsync(loanId, expiry, brCode);
            var deleted = await _unitOfWork.EcsDemand.GetExpiredDeletedRowsAsync(loanId, brCode);
            var vm = new EcsExpiredLoanRowsVM { LoanId = loanId, ExpiredDate = expiry };
            foreach (var t in rows)
            {
                var reason = DeleteProblem(t);
                vm.Rows.Add(ToExpiredVM(t, reason));
            }
            foreach (var t in deleted)
                vm.Deleted.Add(ToExpiredVM(t, await RevokeProblemAsync(t, brCode)));
            return vm;
        }

        public async Task<EcsDemandResultVM> DeleteExpiredDemandRowAsync(decimal trnId, EcsUserContextVM ctx)
        {
            var guard = await CommonGuardsAsync(ctx);
            if (guard != null) return Fail(guard);

            var t = await _unitOfWork.EcsDemand.GetLoanTrnForUpdateAsync(trnId, ctx.BrCode);
            if (t == null || t.TrnTr_Delete || t.Voc_Status != "V") return Fail("The loan transaction no longer exists.");
            var m = await _unitOfWork.EcsDemand.GetMemberOfLoanAsync(t.Loan_Id, ctx.BrCode);
            var memberProblem = m == null ? "The loan is not an ECS loan of this branch." : ExpiredMemberProblem(m);
            if (memberProblem != null) return Fail(memberProblem);
            var problem = DeleteProblem(t);
            if (problem != null) return Fail(problem);

            // Only rows inside the window the page shows (last row on or before the expiry date onwards).
            var window = await _unitOfWork.EcsDemand.GetExpiredWindowRowsAsync(t.Loan_Id, m!.ExpiredDate!.Value, ctx.BrCode);
            if (!window.Any(w => w.Trn_Id == t.Trn_Id))
                return Fail($"The row is dated before the member's expiry ({m.ExpiredDate:dd-MM-yyyy}).");

            t.TrnTr_Delete = true;
            t.ExpiredDeletion = true;
            await _unitOfWork.CompleteAsync();
            return new EcsDemandResultVM { Success = true, Message = $"Demand row of {t.Trn_Date:dd-MM-yyyy} deleted." };
        }

        public async Task<EcsDemandResultVM> RevokeExpiredDeletionAsync(decimal trnId, EcsUserContextVM ctx)
        {
            var guard = await CommonGuardsAsync(ctx);
            if (guard != null) return Fail(guard);

            var t = await _unitOfWork.EcsDemand.GetLoanTrnForUpdateAsync(trnId, ctx.BrCode);
            if (t == null || !t.TrnTr_Delete || !t.ExpiredDeletion) return Fail("The row is not an expired-member deletion.");
            if (await _unitOfWork.EcsDemand.GetMemberOfLoanAsync(t.Loan_Id, ctx.BrCode) == null)
                return Fail("The loan is not an ECS loan of this branch.");
            var problem = await RevokeProblemAsync(t, ctx.BrCode);
            if (problem != null) return Fail(problem);

            t.TrnTr_Delete = false;
            t.ExpiredDeletion = false;
            await _unitOfWork.CompleteAsync();
            return new EcsDemandResultVM { Success = true, Message = $"Demand row of {t.Trn_Date:dd-MM-yyyy} restored." };
        }

        private static string? ExpiredMemberProblem(EcsDemandMemberRow m) =>
            m.IsExpired && m.ExpiredDate.HasValue
                ? null
                : $"Member {m.MemberNo} is not marked as expired with an expiry date. Record the expiry on the member first.";

        /// <summary>Only demand rows with no money on them can be deleted.</summary>
        private static string? DeleteProblem(Loan_Trn t)
        {
            if (t.Trn_Status != "D") return "Only demand rows can be deleted.";
            if (t.Disb_Amt + t.PrlColl_Amt + t.IntColl_Amt + t.PIColl_Amt + t.IODColl_Amt > 0)
                return "Money was received or paid on this row.";
            return null;
        }

        /// <summary>A row can be restored only while its demand is still posted.</summary>
        private async Task<string?> RevokeProblemAsync(Loan_Trn t, string brCode)
        {
            var master = await _unitOfWork.EcsDemand.GetMasterForUpdateAsync(t.Demand_Id, brCode);
            if (master == null || !master.IsDemandUpdated)
                return "The posting of this row's demand has been reversed or deleted.";
            return null;
        }

        private static EcsExpiredLoanTrnVM ToExpiredVM(Loan_Trn t, string? reason) => new()
        {
            TrnId = t.Trn_Id,
            Status = t.Trn_Status,
            TrnDate = t.Trn_Date,
            Disb = t.Disb_Amt,
            PrlDem = t.Prl_Dem,
            IntCalc = t.IntCalc_Amt,
            IntCalcDate = t.IntCalc_Date,
            PICalc = t.PICalc_Amt,
            PrlColl = t.PrlColl_Amt,
            IntColl = t.IntColl_Amt,
            PIColl = t.PIColl_Amt,
            CanAct = reason == null,
            Reason = reason
        };

        // =====================================================================
        // Day End
        // =====================================================================

        public async Task<string?> CalculateForDayEndAsync(DateTime processDate, decimal usrId, decimal yrId, string brCode)
        {
            var day = processDate.Date;
            if (!await IsEcsAsync(brCode)) return null;
            if (!await _unitOfWork.EcsDemand.IsLastBusinessDayOfMonthAsync(day, brCode)) return null;

            var masters = await _unitOfWork.EcsDemand.GetLiveMastersAsync(brCode, 1000);
            var due = NextDueDate(masters.FirstOrDefault(), day);
            var nextMonth = day.AddMonths(1);
            // Only the demand falling due next month; anything else is left to the page.
            if (due <= day || due.Year != nextMonth.Year || due.Month != nextMonth.Month) return null;
            if ((await CalculateBlockersAsync(due, brCode, masters)).Count > 0) return null;

            var demandId = await AddCalculationAsync(due, day, usrId, yrId, brCode);
            if (demandId == null) return null;
            await _unitOfWork.CompleteAsync();
            return $"Demand for {due:dd-MM-yyyy} calculated (not posted).";
        }

        // =====================================================================
        // Calculation
        // =====================================================================

        private sealed class Build
        {
            public List<Mem_Demand> Lines { get; } = new();
            public Dictionary<decimal, EcsDemandMemberRow> Members { get; set; } = new();
            public Dictionary<decimal, EcsDemandLoanRow> Loans { get; } = new();
            public Dictionary<string, string> Labels { get; } = new();
            public List<string> Warnings { get; } = new();
        }

        private async Task<Build> CalculateAsync(DateTime due, DateTime calcDate, decimal usrId, decimal yrId, string brCode)
        {
            var repo = _unitOfWork.EcsDemand;
            var build = new Build();
            decimal zero = decimal.TryParse(brCode + "0000000", out var z) ? z : 0;
            var calculated = DateOnly.FromDateTime(calcDate.Date);

            build.Members = (await repo.GetMembersAsync(brCode)).ToDictionary(m => m.MemId);
            var stopRows = (await repo.GetStoppedMemberIdsAsync(due, brCode)).ToHashSet();
            bool Stopped(EcsDemandMemberRow m) => EcsDemandCalculator.IsStopped(m.MemberStatus, stopRows.Contains(m.MemId));
            bool Active(EcsDemandMemberRow m) => m.MemberStatus == 1;

            Mem_Demand NewLine(string type, EcsDemandMemberRow m) => new()
            {
                Demand_Type = type,
                Mem_Id = m.MemId,
                Calculated_Date = calculated,
                Debtor_Led_Id = zero,
                TD_Id = zero,
                DueBy_Led_Id = zero,
                Creditor_Led_Id = zero,
                RD_PI_Application_Date2 = NoDate,
                Voc_Id = zero,
                Usr_Id = usrId,
                Yr_Id = yrId,
                BrCode = brCode,
                Is_Active = true,
                Stop_Demand = Stopped(m)
            };

            // ----- loans (§4.1, §4.2) -----
            var loans = await repo.GetOpenLoansAsync(brCode);
            var sums = (await repo.GetLoanSumsAsync(brCode, due)).ToDictionary(s => s.LoanId);
            var candidates = loans.Where(l => build.Members.ContainsKey(l.MemId) && sums.TryGetValue(l.LoanId, out var s)
                                              && (s.Disb - s.PrlColl > Tolerance || s.IntCalc - s.IntColl > Tolerance
                                                  || s.PICalc - s.PIColl > Tolerance)).ToList();
            int orphanLoans = loans.Count(l => !build.Members.ContainsKey(l.MemId) && sums.TryGetValue(l.LoanId, out var s)
                                               && s.Disb - s.PrlColl > Tolerance);
            if (orphanLoans > 0)
                build.Warnings.Add($"{orphanLoans} open loan(s) with a balance belong to closed or deleted members and are not demanded.");

            var ids = candidates.Select(l => l.LoanId).ToList();
            var trns = (await repo.GetLoanTrnRowsAsync(ids, brCode, due)).ToLookup(t => t.Loan_Id);
            var rois = (await repo.GetRoiRowsAsync(ids, brCode, due)).ToLookup(r => r.Loan_Id);

            foreach (var l in candidates)
            {
                var m = build.Members[l.MemId];
                build.Loans[l.LoanId] = l;
                build.Labels[$"L:{l.LoanId}"] = $"{l.SchemeName} {l.LoanNo}".Trim();
                if (l.FirstPrlDueDate == null)
                    build.Warnings.Add($"Loan {l.LoanNo} has no first principal due date; only interest is demanded.");

                var input = new EcsDemandLoan(l.LoanId, l.InstType, l.DemFrequency, l.IntApplication, l.PIApplication,
                    l.AdvPrlApplication, l.InstAmt, l.PrlPrd, l.FirstPrlDueDate, l.Roi, l.Pi,
                    trns[l.LoanId].Select(t => new EcsDemandLoanTrn(t.Trn_Date, t.Trn_Status, t.Disb_Date, t.Disb_Amt, t.Prl_Dem,
                        t.PrlColl_Amt, t.IntCalc_Amt, t.IntColl_Amt, t.IntCalc_Date, t.PICalc_Amt, t.PIColl_Amt, t.PICalc_Date)).ToList(),
                    rois[l.LoanId].Select(r => new EcsRateEvent(r.Roi_Wef, r.Roi, r.Pi)).ToList());
                var f = EcsDemandCalculator.Loan(input, due, EcsDemandCalculator.InterestStopDate(m.IsExpired, m.ExpiredDate));
                if (f.Total <= 0) continue;

                var line = NewLine("L", m);
                line.Loan_Id = l.LoanId;
                line.Prl_Schedule = f.PrlCurrent;
                line.Loan_Oustanding = f.Outstanding;
                line.Loan_Prl_Arrear = f.PrlArrear;
                line.Loan_Int_Arrear = f.IntArrear;
                line.Loan_PI_Arrear = f.PiArrear;
                line.Loan_Prl_Current = f.PrlCurrent;
                line.Loan_Int_Current = f.IntCurrent;
                line.Loan_PI_Current = f.PiCurrent;
                line.Loan_Total = f.Total;
                line.Total_Demand = f.Total;
                build.Lines.Add(line);
            }

            var active = build.Members.Values.Where(Active).ToList();

            // ----- deposits (§4.3) -----
            var masters = await repo.GetDepositMastersAsync(brCode);
            if (masters.Count > 0 && active.Count > 0)
            {
                var options = (await repo.GetDepositOptionsAsync(brCode, due))
                    .GroupBy(o => (o.Mem_Id, o.Deposit_Id))
                    .ToDictionary(g => g.Key, g => g.Last().Option_Amount);   // ordered by Wef, Option_Id
                var dueOnly = DateOnly.FromDateTime(due);
                var rates = await repo.GetDepositRoiAsync(brCode);
                var depSums = (await repo.GetDepositSumsAsync(brCode, due)).ToDictionary(s => (s.MemId, s.DepositId));

                foreach (var dm in masters)
                {
                    build.Labels[$"D:{dm.Deposit_Id}"] = dm.Deposit_Name ?? $"Deposit {dm.Deposit_Id}";
                    double penalRate = rates.Where(r => r.Deposit_Id == dm.Deposit_Id && r.Wef.HasValue && r.Wef.Value <= dueOnly)
                                            .OrderBy(r => r.Wef).LastOrDefault()?.Penal_Interest_Rate ?? 0;
                    foreach (var m in active)
                    {
                        depSums.TryGetValue((m.MemId, dm.Deposit_Id), out var s);
                        double? option = options.TryGetValue((m.MemId, dm.Deposit_Id), out var o) ? o : null;
                        var input = new EcsDemandDeposit(dm.Deposit_Id, dm.Percentage, dm.Fixed_Amount, dm.Maximum_Amount,
                            dm.Frequency, dm.Demand_Month, dm.Is_Arrear_Demand_Applicable, option, penalRate,
                            s?.Demand ?? 0, s?.Receipt ?? 0, s?.PiCalc ?? 0, s?.PiReceived ?? 0);
                        var f = EcsDemandCalculator.Deposit(input, m.BasicPay, due);
                        if (f.Total <= 0) continue;

                        var line = NewLine("D", m);
                        line.Deposit_Id = dm.Deposit_Id;
                        line.Deposit_Arrear = f.Arrear;
                        line.Deposit_Current = f.Current;
                        line.Deposit_PI_Arrear = f.PiArrear;
                        line.Deposit_PI_Current = f.PiCurrent;
                        line.Deposit_Total = f.Total;
                        line.Total_Demand = f.Total;
                        build.Lines.Add(line);
                    }
                }
            }

            // ----- recurring deposits (§4.4) -----
            var activeIds = active.Select(m => m.MemId).ToHashSet();
            foreach (var rd in (await repo.GetOpenRdsAsync(brCode, due)).Where(r => activeIds.Contains(r.MemId)))
            {
                var m = build.Members[rd.MemId];
                build.Labels[$"R:{rd.TdId}"] = $"RD {rd.TdNo}";
                var f = EcsDemandCalculator.Rd(new EcsDemandRd(rd.TdId, rd.ValueDate, rd.PeriodInMonths, rd.Instalment,
                    rd.Received, rd.PenalCalc, rd.PenalReceived, rd.PenalRate), due);
                if (f.Total <= 0) continue;

                var line = NewLine("R", m);
                line.TD_Id = rd.TdId;
                line.RD_Instalment = f.Instalment;
                line.RD_NoOf_Instalments = f.MissedInstalments;
                line.RD_Demand = f.Demand;
                line.RD_PI_Arrear = f.PiArrear;
                line.RD_PI_Calc = f.PiCurrent;
                line.RD_PI_Application_Date = f.PiCurrent > 0 ? due : null;
                line.RD_Total = f.Total;
                line.Total_Demand = f.Total;
                build.Lines.Add(line);
            }

            // ----- due-to (§4.5) -----
            foreach (var d in (await repo.GetDueToSumsAsync(brCode, due)).Where(d => activeIds.Contains(d.MemId)))
            {
                double amount = EcsDemandCalculator.DueTo(d.Payments, d.Receipts);
                if (amount <= 0) continue;
                build.Labels[$"S:{d.LedId}"] = d.LedName ?? $"Ledger {d.LedId}";

                var line = NewLine("S", build.Members[d.MemId]);
                line.Debtor_Led_Id = d.LedId;
                line.Debtor_Current = amount;
                line.Debtor_Total = amount;
                line.Total_Demand = amount;
                build.Lines.Add(line);
            }

            return build;
        }

        /// <summary>Lines whose current (posted) figures differ between the saved and a fresh calculation.</summary>
        private static List<string> CompareCurrent(List<Mem_Demand> saved, List<Mem_Demand> fresh,
            Dictionary<string, string> labels, Dictionary<decimal, EcsDemandMemberRow> members)
        {
            static string Key(Mem_Demand l) => $"{l.Demand_Type}:{l.Mem_Id}:{AccountId(l)}";
            static double[] Current(Mem_Demand? l) => l == null
                ? new double[6]
                : new[] { l.Loan_Prl_Current, l.Loan_Int_Current, l.Loan_PI_Current, l.Deposit_Current + l.Deposit_PI_Current,
                          l.RD_Demand + l.RD_PI_Calc, l.Debtor_Current };

            var a = saved.GroupBy(Key).ToDictionary(g => g.Key, g => g.First());
            var b = fresh.GroupBy(Key).ToDictionary(g => g.Key, g => g.First());
            var changes = new List<string>();
            foreach (var key in a.Keys.Union(b.Keys))
            {
                a.TryGetValue(key, out var s);
                b.TryGetValue(key, out var f);
                var cs = Current(s);
                var cf = Current(f);
                if (cs.Zip(cf).All(p => Math.Abs(p.First - p.Second) < Tolerance)) continue;

                var line = (s ?? f)!;
                var who = members.TryGetValue(line.Mem_Id, out var m) ? m.MemberNo : line.Mem_Id.ToString();
                var what = labels.TryGetValue($"{line.Demand_Type}:{AccountId(line)}", out var lbl) ? lbl : line.Demand_Type;
                changes.Add($"Member {who}, {what}: saved {cs.Sum():N2}, now {cf.Sum():N2}");
            }
            return changes;
        }

        // =====================================================================
        // Guards
        // =====================================================================

        private async Task<string?> CommonGuardsAsync(EcsUserContextVM ctx)
        {
            if (!await IsEcsAsync(ctx.BrCode)) return "ECS Demand Calculation is available only to ECS societies.";
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

        /// <summary>§7: previous demand posted, one live demand per due date, due dates move forward.</summary>
        private static Task<List<string>> CalculateBlockersAsync(DateTime due, string brCode, List<Mem_Demand_Master> liveMasters)
        {
            var reasons = new List<string>();
            var latest = liveMasters.OrderByDescending(m => m.DemandDate).ThenByDescending(m => m.Demand_Id).FirstOrDefault();
            if (liveMasters.Any(m => m.DemandDate?.Date == due))
                reasons.Add($"A demand for {due:dd-MM-yyyy} already exists.");
            else if (latest?.DemandDate != null && due < latest.DemandDate.Value.Date)
                reasons.Add($"The due date must be after the last demand's due date ({latest.DemandDate:dd-MM-yyyy}).");
            if (latest != null && !latest.IsDemandUpdated && latest.DemandDate?.Date != due)
                reasons.Add($"The demand for {latest.DemandDate:dd-MM-yyyy} is calculated but not posted. Post or delete it first.");
            // Spec 13 §8: the next demand's arrears assume every office's recovery of this one is saved.
            else if (latest != null && latest.IsDemandUpdated && !latest.IsRecoveryUpdated && latest.DemandDate?.Date != due)
                reasons.Add($"The salary recovery against the demand for {latest.DemandDate:dd-MM-yyyy} is not saved for every office. Save it on the Recovery Appropriation page first.");
            return Task.FromResult(reasons);
        }

        private async Task<List<string>> DeletePostedBlockersAsync(Mem_Demand_Master master, string brCode)
        {
            var reasons = new List<string>();
            if (master.IsRecoveryUpdated) reasons.Add("The salary recovery against this demand has been saved.");
            else if ((await _unitOfWork.EcsDemand.GetStatusRowsForUpdateAsync(master.Demand_Id, brCode)).Any(s => s.Is_Appropriated))
                reasons.Add("The salary recovery of at least one office is appropriated. Delete it on the Recovery Appropriation page first.");
            var later = await _unitOfWork.EcsDemand.GetLaterTransactionsAsync(master.Demand_Id, brCode);
            if (later.Count > 0)
            {
                reasons.Add($"{later.Count} account(s) have transactions after the demand was posted:");
                reasons.AddRange(later.Take(20));
                if (later.Count > 20) reasons.Add($"…and {later.Count - 20} more.");
            }
            return reasons;
        }

        // =====================================================================
        // Mapping
        // =====================================================================

        private static DateTime NextDueDate(Mem_Demand_Master? latest, DateTime businessDay)
        {
            if (latest?.DemandDate != null)
                return (latest.NextDemandDate ?? EcsDemandCalculator.NextDemandDate(latest.DemandDate.Value)).Date;
            var d = businessDay.Date;
            return new DateTime(d.Year, d.Month, 1).AddMonths(1);
        }

        private static decimal AccountId(Mem_Demand l) => l.Demand_Type switch
        {
            "L" => l.Loan_Id,
            "D" => l.Deposit_Id,
            "R" => l.TD_Id,
            "S" => l.Debtor_Led_Id,
            _ => 0
        };

        private static EcsDemandMasterVM ToMasterVM(Mem_Demand_Master m, List<EcsDemandTotalRow> totals)
        {
            var mine = totals.Where(t => t.DemandId == m.Demand_Id).ToList();
            return new EcsDemandMasterVM
            {
                DemandId = m.Demand_Id,
                DueDate = m.DemandDate?.Date ?? DateTime.MinValue,
                CalculationDate = m.DemandCalculationDate,
                NextDemandDate = m.NextDemandDate,
                IsPosted = m.IsDemandUpdated,
                IsRecoveryAppropriated = m.IsRecoveryAppropriated,
                IsRecoveryUpdated = m.IsRecoveryUpdated,
                Lines = mine.Sum(t => t.Lines),
                Total = EcsDemandCalculator.R2(mine.Sum(t => t.Total))
            };
        }

        private static EcsDemandLineVM ToLineVM(Mem_Demand l, Dictionary<string, string> labels)
        {
            var vm = new EcsDemandLineVM { Type = l.Demand_Type ?? "", AccountId = AccountId(l), Total = l.Total_Demand };
            vm.Account = labels.TryGetValue($"{vm.Type}:{vm.AccountId}", out var lbl) ? lbl : null;
            switch (l.Demand_Type)
            {
                case "L":
                    vm.PrincipalArrear = l.Loan_Prl_Arrear; vm.PrincipalCurrent = l.Loan_Prl_Current;
                    vm.InterestArrear = l.Loan_Int_Arrear; vm.InterestCurrent = l.Loan_Int_Current;
                    vm.PenalArrear = l.Loan_PI_Arrear; vm.PenalCurrent = l.Loan_PI_Current;
                    vm.LoanOutstanding = l.Loan_Oustanding;
                    break;
                case "D":
                    vm.PrincipalArrear = l.Deposit_Arrear; vm.PrincipalCurrent = l.Deposit_Current;
                    vm.PenalArrear = l.Deposit_PI_Arrear; vm.PenalCurrent = l.Deposit_PI_Current;
                    break;
                case "R":
                    vm.PrincipalCurrent = l.RD_Demand;
                    vm.PenalArrear = l.RD_PI_Arrear; vm.PenalCurrent = l.RD_PI_Calc;
                    break;
                case "S":
                    vm.PrincipalArrear = l.Debtor_Arrear; vm.PrincipalCurrent = l.Debtor_Current;
                    break;
            }
            return vm;
        }

        private static readonly (string Type, string Name)[] Components =
        {
            ("L", "Loans"), ("D", "Deposits"), ("R", "Recurring deposits"), ("S", "Due-to")
        };

        private static EcsDemandSummaryVM Summarise(List<Mem_Demand> lines, Dictionary<decimal, EcsDemandMemberRow> members,
            Dictionary<string, string> labels, Dictionary<decimal, string> officeNames, bool withMembers)
        {
            var vms = lines.Select(l => (Line: l, Vm: ToLineVM(l, labels))).ToList();
            var summary = new EcsDemandSummaryVM
            {
                LineCount = lines.Count,
                MemberCount = lines.Select(l => l.Mem_Id).Distinct().Count(),
                StoppedMemberCount = lines.Where(l => l.Stop_Demand).Select(l => l.Mem_Id).Distinct().Count(),
                GrandTotal = EcsDemandCalculator.R2(lines.Sum(l => l.Total_Demand))
            };

            foreach (var (type, name) in Components)
            {
                var of = vms.Where(x => x.Vm.Type == type).ToList();
                if (of.Count == 0) continue;
                summary.Components.Add(new EcsDemandComponentTotalVM
                {
                    Type = type,
                    Name = name,
                    Lines = of.Count,
                    StoppedLines = of.Count(x => x.Line.Stop_Demand),
                    PrincipalArrear = EcsDemandCalculator.R2(of.Sum(x => x.Vm.PrincipalArrear)),
                    PrincipalCurrent = EcsDemandCalculator.R2(of.Sum(x => x.Vm.PrincipalCurrent)),
                    InterestArrear = EcsDemandCalculator.R2(of.Sum(x => x.Vm.InterestArrear)),
                    InterestCurrent = EcsDemandCalculator.R2(of.Sum(x => x.Vm.InterestCurrent)),
                    PenalArrear = EcsDemandCalculator.R2(of.Sum(x => x.Vm.PenalArrear)),
                    PenalCurrent = EcsDemandCalculator.R2(of.Sum(x => x.Vm.PenalCurrent)),
                    Total = EcsDemandCalculator.R2(of.Sum(x => x.Vm.Total))
                });
            }

            decimal OfficeOf(decimal memId) => members.TryGetValue(memId, out var m) ? m.OfficeId : 0;
            string OfficeName(decimal officeId) =>
                officeNames.TryGetValue(officeId, out var n) ? n : officeId == 0 ? "—" : $"Office {officeId}";
            summary.Offices = lines.GroupBy(l => OfficeOf(l.Mem_Id)).Select(g => new EcsDemandOfficeTotalVM
            {
                OfficeId = g.Key,
                OfficeName = OfficeName(g.Key),
                Members = g.Select(l => l.Mem_Id).Distinct().Count(),
                StoppedMembers = g.Where(l => l.Stop_Demand).Select(l => l.Mem_Id).Distinct().Count(),
                Loans = EcsDemandCalculator.R2(g.Where(l => l.Demand_Type == "L").Sum(l => l.Total_Demand)),
                Deposits = EcsDemandCalculator.R2(g.Where(l => l.Demand_Type == "D").Sum(l => l.Total_Demand)),
                Others = EcsDemandCalculator.R2(g.Where(l => l.Demand_Type is "R" or "S").Sum(l => l.Total_Demand)),
                Total = EcsDemandCalculator.R2(g.Sum(l => l.Total_Demand))
            }).OrderBy(o => o.OfficeName, StringComparer.OrdinalIgnoreCase).ThenBy(o => o.OfficeId).ToList();

            if (withMembers)
            {
                summary.Members = vms.GroupBy(x => x.Line.Mem_Id).Select(g =>
                {
                    members.TryGetValue(g.Key, out var m);
                    return new EcsDemandMemberVM
                    {
                        MemId = g.Key,
                        MemberNo = m?.MemberNo ?? g.Key.ToString(),
                        MemberName = m?.MemberName,
                        OfficeId = m?.OfficeId ?? 0,
                        OfficeName = OfficeName(m?.OfficeId ?? 0),
                        Stopped = g.Any(x => x.Line.Stop_Demand),
                        Total = EcsDemandCalculator.R2(g.Sum(x => x.Vm.Total)),
                        Lines = g.Select(x => x.Vm).OrderBy(v => Array.FindIndex(Components, c => c.Type == v.Type)).ToList()
                    };
                }).OrderBy(m => m.MemberNo, StringComparer.OrdinalIgnoreCase).ToList();
            }
            return summary;
        }

        // =====================================================================
        // Results
        // =====================================================================

        private static EcsDemandResultVM Fail(string message, List<string>? reasons = null) =>
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
