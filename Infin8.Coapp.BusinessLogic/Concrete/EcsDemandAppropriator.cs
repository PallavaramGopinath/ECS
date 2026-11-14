using Infin8.Coapp.Models;

namespace Infin8.Coapp.BusinessLogic
{
    /// <summary>A loan's valid ledger totals (all dates) — what the revised demand is measured against.</summary>
    public sealed record EcsLoanBalance(double Disb, double PrlColl, double IntCalc, double IntColl, double PICalc, double PIColl)
    {
        public double PrincipalOutstanding => Disb - PrlColl;
    }

    /// <summary>
    /// A loan line's inputs as on the recovery date (spec 13a §4.2): ledger totals, what this demand's own <c>D</c> row
    /// booked, and the fresh accrual from the loan's last calculation date to the receipt date.
    /// </summary>
    public sealed record EcsLoanRevision(EcsLoanBalance Balance, double DemandIntCalc, double DemandPICalc, double FreshInt, double FreshPI);

    /// <summary>A deposit line's deposit_trn sums (spec 13a §4.3): before the due date and received since it.</summary>
    public sealed record EcsDepositRevision(double BeforeDemand, double BeforeReceipt, double BeforePiCalc, double BeforePiReceived,
        double SinceReceipt, double SincePiReceived, bool Closed);

    /// <summary>An RD line recounted as on the recovery date (spec 13a §4.4); <see cref="Figures"/> null = keep the demand.</summary>
    public sealed record EcsRdRevision(EcsRdDemandFigures? Figures, bool Closed);

    /// <summary>What <see cref="EcsDemandAppropriator.Revise"/> needs for one line; only the part for its type is read.</summary>
    public sealed record EcsLineRevision(EcsLoanRevision? Loan = null, EcsDepositRevision? Deposit = null,
        EcsRdRevision? Rd = null, double? DueTo = null);

    /// <summary>A loan the split may touch: scheme, sanction date (oldest first) and principal outstanding.</summary>
    public sealed record EcsAppropriationLoan(decimal LoanId, int SchemeId, DateTime? SanDate, double Outstanding);

    /// <summary>One <c>mem_collection_priority</c> row.</summary>
    public sealed record EcsPriorityHead(int No, string Type, int SchemeId, int DepositId, decimal LedId);

    /// <summary>
    /// Recovery appropriation rules (spec 13 §4), ported from TSIS <c>DemandService.UpdateSubsequentRecoveryInDemand</c>
    /// and <c>AppropriationOfRecovery</c>. Pure — no database — so the split can be checked by hand. Works on
    /// <see cref="Mem_Demand"/> rows: reads the demand columns, writes the <c>*2</c> (revised) and collection columns.
    /// </summary>
    public static class EcsDemandAppropriator
    {
        // =====================================================================
        // §4.1 Revised demand
        // =====================================================================

        /// <summary>
        /// Writes the demand as it stands on the recovery date into the line's <c>*2</c> columns (spec 13a §4).
        /// </summary>
        public static void Revise(Mem_Demand l, EcsLineRevision input)
        {
            switch (l.Demand_Type)
            {
                case "L":
                    var lr = input.Loan ?? new EcsLoanRevision(new EcsLoanBalance(0, 0, 0, 0, 0, 0), 0, 0, 0, 0);
                    var b = lr.Balance;
                    // §4.1 Principal: as demanded, capped (arrear first) at what is still outstanding. No new principal.
                    double prlOs = Math.Max(0, b.PrincipalOutstanding);
                    double pa = R(l.Loan_Prl_Arrear), pc = R(l.Loan_Prl_Current);
                    if (pa + pc > prlOs)
                    {
                        if (pa >= prlOs) { pa = prlOs; prlOs = 0; } else prlOs -= pa;
                        if (pc >= prlOs) pc = prlOs;
                    }
                    l.Loan_Prl_Arrear2 = R(pa);
                    l.Loan_Prl_Current2 = R(pc);

                    // §4.2 Interest / penal from the ledger now plus the fresh accrual to the receipt date.
                    (l.Loan_Int_Arrear2, l.Loan_Int_Current2) = InterestHeads(b.IntCalc, b.IntColl, lr.DemandIntCalc, lr.FreshInt);
                    (l.Loan_PI_Arrear2, l.Loan_PI_Current2) = InterestHeads(b.PICalc, b.PIColl, lr.DemandPICalc, lr.FreshPI);
                    l.Loan_Total2 = R(l.Loan_Prl_Arrear2 + l.Loan_Prl_Current2 + l.Loan_Int_Arrear2 + l.Loan_Int_Current2
                                      + l.Loan_PI_Arrear2 + l.Loan_PI_Current2);
                    break;

                case "D":
                    var d = input.Deposit;
                    if (d == null)
                    {
                        (l.Deposit_Arrear2, l.Deposit_Current2) = (l.Deposit_Arrear, l.Deposit_Current);
                        (l.Deposit_PI_Arrear2, l.Deposit_PI_Current2) = (l.Deposit_PI_Arrear, l.Deposit_PI_Current);
                    }
                    else if (d.Closed)
                    {
                        l.Deposit_Arrear2 = l.Deposit_Current2 = l.Deposit_PI_Arrear2 = l.Deposit_PI_Current2 = 0;
                    }
                    else
                    {
                        (l.Deposit_Arrear2, l.Deposit_Current2) =
                            DepositHeads(l.Deposit_Arrear, l.Deposit_Current, d.BeforeDemand - d.BeforeReceipt, d.SinceReceipt);
                        (l.Deposit_PI_Arrear2, l.Deposit_PI_Current2) =
                            DepositHeads(l.Deposit_PI_Arrear, l.Deposit_PI_Current, d.BeforePiCalc - d.BeforePiReceived, d.SincePiReceived);
                    }
                    l.Deposit_Total2 = R(l.Deposit_Arrear2 + l.Deposit_Current2 + l.Deposit_PI_Arrear2 + l.Deposit_PI_Current2);
                    break;

                case "S":
                    l.Debtor_Arrear2 = 0;
                    l.Debtor_Current2 = R(Math.Max(0, input.DueTo ?? 0));
                    l.Debtor_Total2 = l.Debtor_Current2;
                    break;

                case "R":
                    var rd = input.Rd;
                    int demandInst = (int)Math.Round(l.RD_Instalment, MidpointRounding.AwayFromZero);
                    if (rd is { Closed: true })
                    {
                        l.RD_Instalment2 = demandInst;
                        l.RD_NoOf_Instalments2 = 0;
                        l.RD_Demand2 = l.RD_PI_Arrear2 = l.RD_PI_Calc2 = 0;
                    }
                    else if (rd?.Figures is { } f)
                    {
                        // Recounted as on the recovery date, never above what was demanded (no new demand).
                        int inst = f.Instalment > 0 ? f.Instalment : demandInst;
                        l.RD_Instalment2 = inst;
                        l.RD_Demand2 = R(Math.Min(l.RD_Demand, f.Demand));
                        l.RD_NoOf_Instalments2 = inst > 0 ? (int)Math.Round(l.RD_Demand2 / inst) : 0;
                        l.RD_PI_Arrear2 = R(Math.Min(l.RD_PI_Arrear, f.PiArrear));
                        l.RD_PI_Calc2 = R(Math.Min(l.RD_PI_Calc, f.PiCurrent));
                    }
                    else
                    {
                        l.RD_Instalment2 = demandInst;
                        l.RD_NoOf_Instalments2 = l.RD_NoOf_Instalments;
                        l.RD_Demand2 = l.RD_Demand;
                        l.RD_PI_Arrear2 = l.RD_PI_Arrear;
                        l.RD_PI_Calc2 = l.RD_PI_Calc;
                    }
                    l.RD_Total2 = R(l.RD_Demand2 + l.RD_PI_Arrear2 + l.RD_PI_Calc2);
                    break;
            }
            l.Total_Demand2 = R(l.Loan_Total2 + l.Deposit_Total2 + l.Debtor_Total2 + l.RD_Total2);
        }

        /// <summary>
        /// Spec 13a §4.2. Still due = Σ calc − Σ coll + fresh (floored at 0). Arrear = what was booked before this demand
        /// (Σ calc less this demand's own D row) less everything collected, capped at what is still due; the rest is
        /// current — this demand's D row plus the fresh accrual, less any collection beyond the arrear.
        /// </summary>
        public static (double arrear, double current) InterestHeads(double calc, double coll, double demandCalc, double fresh)
        {
            double due = R(Math.Max(0, calc - coll + fresh));
            double arrear = R(Math.Min(due, Math.Max(0, calc - demandCalc - coll)));
            return (arrear, R(due - arrear));
        }

        /// <summary>
        /// Spec 13a §4.3. Arrear base = the demanded arrear, capped at the ledger's arrear before the due date; money
        /// received since the due date comes off the arrear, then the current.
        /// </summary>
        public static (double arrear, double current) DepositHeads(double demandArrear, double demandCurrent, double ledgerArrear, double since)
        {
            double a = R(Math.Min(Math.Max(0, demandArrear), Math.Max(0, ledgerArrear)));
            double s = Math.Max(0, since);
            double arrear = R(Math.Max(0, a - s));
            double over = Math.Max(0, s - a);
            return (arrear, R(Math.Max(0, demandCurrent - over)));
        }

        /// <summary>The revised figures, for comparing a stored appropriation with a fresh one (spec 13 §6.1).</summary>
        public static double[] RevisedFigures(Mem_Demand l) => new[]
        {
            l.Loan_Prl_Arrear2, l.Loan_Prl_Current2, l.Loan_Int_Arrear2, l.Loan_Int_Current2, l.Loan_PI_Arrear2,
            l.Loan_PI_Current2, l.Deposit_Arrear2, l.Deposit_Current2, l.Deposit_PI_Arrear2, l.Deposit_PI_Current2,
            l.Debtor_Arrear2, l.Debtor_Current2, l.RD_Demand2, l.RD_PI_Arrear2, l.RD_PI_Calc2
        };

        // =====================================================================
        // §4.2 Split
        // =====================================================================

        public sealed class Result
        {
            /// <summary>New <c>A</c> (advance on a loan not in the demand) and <c>B</c> (excess) lines.</summary>
            public List<Mem_Demand> NewLines { get; } = new();
            /// <summary>What no head took (only when the branch has no DBY priority).</summary>
            public double Unappropriated { get; set; }
        }

        /// <summary>
        /// Splits one member's <paramref name="recovery"/> across <paramref name="lines"/> (that member's live lines of
        /// the demand, revised) in <paramref name="priorities"/> order. Collection columns of every line are reset
        /// first. <paramref name="newLine"/> creates an empty A/B line for the member.
        /// </summary>
        public static Result Appropriate(
            double recovery,
            IReadOnlyList<Mem_Demand> lines,
            IReadOnlyDictionary<decimal, EcsAppropriationLoan> loans,
            IReadOnlyList<EcsAppropriationLoan> loansNotInDemand,
            IReadOnlyList<EcsPriorityHead> priorities,
            decimal zeroId,
            Func<string, Mem_Demand> newLine)
        {
            var result = new Result();
            foreach (var l in lines) ClearCollections(l);
            double rem = R(recovery);

            var live = lines.Where(l => !l.Stop_Demand).ToList();
            List<Mem_Demand> LoanLines(int schemeId) => live
                .Where(l => l.Demand_Type == "L" && loans.TryGetValue(l.Loan_Id, out var ln) && ln.SchemeId == schemeId)
                .OrderBy(l => loans[l.Loan_Id].SanDate ?? DateTime.MaxValue).ThenBy(l => l.Loan_Id).ToList();
            Mem_Demand? DepositLine(int depositId) =>
                live.FirstOrDefault(l => l.Demand_Type == "D" && l.Deposit_Id == depositId);

            foreach (var p in priorities.OrderBy(p => p.No))
            {
                rem = R(rem);
                if (rem <= 0) break;   // every head after this would take nothing (LPA writes nothing at 0 either)
                switch (p.Type)
                {
                    case "DPI":
                        if (DepositLine(p.DepositId) is { } dpi)
                        {
                            dpi.Deposit_Arrear_PI_Collection = Take(ref rem, dpi.Deposit_PI_Arrear2);
                            dpi.Deposit_Current_PI_Collection = Take(ref rem, dpi.Deposit_PI_Current2);
                        }
                        break;

                    case "D":
                        if (DepositLine(p.DepositId) is { } d)
                        {
                            d.Deposit_Arrear_Collection = Take(ref rem, d.Deposit_Arrear2);
                            d.Deposit_Current_Collection = Take(ref rem, d.Deposit_Current2);
                        }
                        break;

                    case "LPI":
                        foreach (var l in LoanLines(p.SchemeId))
                        {
                            l.loan_PI_Arrear_Collection = Take(ref rem, l.Loan_PI_Arrear2);
                            l.Loan_PI_Current_Collection = Take(ref rem, l.Loan_PI_Current2);
                        }
                        break;

                    case "LI":
                        foreach (var l in LoanLines(p.SchemeId))
                        {
                            l.Loan_Int_Arrear_Collection = Take(ref rem, l.Loan_Int_Arrear2);
                            l.Loan_Int_Current_Collection = Take(ref rem, l.Loan_Int_Current2);
                        }
                        break;

                    case "LP":
                        foreach (var l in LoanLines(p.SchemeId))
                        {
                            l.Loan_Prl_Arrear_Collection = Take(ref rem, l.Loan_Prl_Arrear2);
                            l.Loan_Prl_Current_Collection = Take(ref rem, l.Loan_Prl_Current2);
                        }
                        break;

                    case "DTO":
                        bool anyLedger = p.LedId == 0 || p.LedId == zeroId;
                        foreach (var l in live.Where(l => l.Demand_Type == "S" && (anyLedger || l.Debtor_Led_Id == p.LedId)))
                        {
                            l.Debtor_Arrear_Collection = Take(ref rem, l.Debtor_Arrear2);
                            l.Debtor_Current_Collection = Take(ref rem, l.Debtor_Current2);
                        }
                        break;

                    case "RD":
                        foreach (var l in live.Where(l => l.Demand_Type == "R").OrderBy(l => l.TD_Id))
                        {
                            l.RD_PI_Receipt = Take(ref rem, l.RD_PI_Arrear2 + l.RD_PI_Calc2);
                            double inst = l.RD_Instalment2 > 0 ? l.RD_Instalment2 : l.RD_Instalment;
                            if (rem >= l.RD_Demand2) l.RD_Receipt = Take(ref rem, l.RD_Demand2);
                            else if (inst > 0) l.RD_Receipt = Take(ref rem, Math.Floor(rem / inst) * inst);
                        }
                        break;

                    case "LPA":
                        foreach (var ln in loansNotInDemand.OrderBy(x => x.LoanId))
                        {
                            double take = Take(ref rem, ln.Outstanding);
                            if (take <= 0) continue;
                            var a = newLine("A");
                            a.Loan_Id = ln.LoanId;
                            a.Loan_Prl_Current_Collection = take;
                            result.NewLines.Add(a);
                        }
                        break;

                    case "LP1":
                    case "LP2":
                    case "LP3":
                        foreach (var l in LoanLines(p.SchemeId))
                        {
                            double os = loans[l.Loan_Id].Outstanding - l.Loan_Prl_Arrear_Collection - l.Loan_Prl_Current_Collection;
                            l.Loan_Prl_Current_Collection = R(l.Loan_Prl_Current_Collection + Take(ref rem, os));
                        }
                        break;

                    case "DBY":
                        var b = newLine("B");
                        b.Creditor_Led_Id = p.LedId;
                        b.Creditor_Collection = rem;
                        rem = 0;
                        result.NewLines.Add(b);
                        break;
                }
            }

            foreach (var l in lines.Concat(result.NewLines)) Total(l);
            result.Unappropriated = R(rem);
            return result;
        }

        /// <summary>Recomputes the line's collection totals (TSIS <c>UpdateTotalAppropriation</c>).</summary>
        public static void Total(Mem_Demand l)
        {
            l.Loan_Total_Collection = R(l.loan_PI_Arrear_Collection + l.Loan_Int_Arrear_Collection + l.Loan_Prl_Arrear_Collection
                                        + l.Loan_PI_Current_Collection + l.Loan_Int_Current_Collection + l.Loan_Prl_Current_Collection);
            l.Deposit_Total_Collection = R(l.Deposit_Arrear_Collection + l.Deposit_Current_Collection
                                           + l.Deposit_Arrear_PI_Collection + l.Deposit_Current_PI_Collection);
            l.Debtor_Total_Collection = R(l.Debtor_Arrear_Collection + l.Debtor_Current_Collection);
            l.RD_Total_Receipt = R(l.RD_Receipt + l.RD_PI_Receipt);
            l.DueBy_Total_Collection = R(l.DueBy_Arrear_Collection + l.DueBy_Current_Collection);
            l.Total_Collection = R(l.Loan_Total_Collection + l.Deposit_Total_Collection + l.Debtor_Total_Collection
                                   + l.RD_Total_Receipt + l.DueBy_Total_Collection + l.Creditor_Collection);
        }

        public static void ClearCollections(Mem_Demand l)
        {
            l.loan_PI_Arrear_Collection = l.Loan_Int_Arrear_Collection = l.Loan_Prl_Arrear_Collection = 0;
            l.Loan_PI_Current_Collection = l.Loan_Int_Current_Collection = l.Loan_Prl_Current_Collection = 0;
            l.Deposit_Arrear_Collection = l.Deposit_Current_Collection = 0;
            l.Deposit_Arrear_PI_Collection = l.Deposit_Current_PI_Collection = 0;
            l.Debtor_Arrear_Collection = l.Debtor_Current_Collection = 0;
            l.RD_Receipt = l.RD_PI_Receipt = 0;
            l.DueBy_Arrear_Collection = l.DueBy_Current_Collection = 0;
            l.Creditor_Collection = 0;
            Total(l);
        }

        /// <summary>Takes at most <paramref name="due"/> (never a negative amount) from <paramref name="rem"/>.</summary>
        private static double Take(ref double rem, double due)
        {
            double take = R(Math.Min(rem, Math.Max(0, due)));
            if (take <= 0) return 0;
            rem = R(rem - take);
            return take;
        }

        private static double R(double v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);
    }
}
