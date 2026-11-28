using System;
using System.Collections.Generic;

namespace Infin8.Coapp.Dto
{
    /// <summary>
    /// ECS RD Refund, acc 10 (spec 16 §10). Serialised into staging_details.related_account_data at Initiate and
    /// rechecked RD by RD at approval.
    /// </summary>
    public class DtoEcsRdRefund
    {
        public decimal MemId { get; set; }
        public string? MemberNo { get; set; }
        public string? MemberName { get; set; }
        public DateTime ApplicationDate { get; set; }      // business day = refund date
        public int CashOrAdjustment { get; set; }          // 1 cash, >1 adjustment
        public List<DtoEcsRdRefundLine> Lines { get; set; } = new();   // selected RDs only
        public double TotalReceived { get; set; }
        public double TotalInterest { get; set; }
        public double TotalPenal { get; set; }
        public double NetPayable { get; set; }             // = staging Payment_Amount
    }

    /// <summary>One open RD of the member (spec 16 §4–§6). Figures are as on the refund date.</summary>
    public class DtoEcsRdRefundLine
    {
        public decimal TD_Id { get; set; }
        public string? TD_No { get; set; }
        public long TDScheme_Id { get; set; }
        public double Instalment { get; set; }
        public DateTime ValueDate { get; set; }
        public int PeriodInMonths { get; set; }
        public double RateOfInterest { get; set; }         // contract
        public DateTime? MaturityDate { get; set; }
        public double MaturityAmount { get; set; }
        public double PenalRate { get; set; }              // PenalRateForRD, per month (spec 14)

        // Ledger sums over valid termdeposit_trn rows (§4.1)
        public double Received { get; set; }               // Σ DepositReceiptAmount
        public double PenalCalculated { get; set; }
        public double PenalReceived { get; set; }
        public DateTime? LastPenalAppliedDate { get; set; }

        // Computed (§5, §6)
        public int InstalmentsReceived { get; set; }       // i
        public int MonthsRun { get; set; }                 // n (§5.2)
        public bool IsMatured { get; set; }
        public double BandRoi { get; set; }
        public double ForeclosureRoi { get; set; }
        public double AppliedRoi { get; set; }
        public double Interest { get; set; }
        public DateTime InterestAppliedDate { get; set; }
        public double PenalArrear { get; set; }
        public double PenalCurrent { get; set; }
        public double PenalPending { get; set; }           // §6 = arrear + current
        public double TotalPayable { get; set; }           // Received + Interest − PenalPending
        public bool Selected { get; set; }

        /// <summary>Why this RD cannot be refunded now (lien, pending transaction, demand, missing ROI); null when it can.</summary>
        public string? Block { get; set; }
    }

    /// <summary>Response of GET api/rdrefund/Rds/{memId}.</summary>
    public class EcsRdRefundLoadVM
    {
        public DateTime ApplicationDate { get; set; }
        public List<DtoEcsRdRefundLine> Lines { get; set; } = new();
        public string? Message { get; set; }
    }
}
