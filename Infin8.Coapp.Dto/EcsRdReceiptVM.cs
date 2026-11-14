using System;
using System.Collections.Generic;

namespace Infin8.Coapp.Dto
{
    /// <summary>
    /// ECS RD Receipt, acc 2 (spec 14 §9). Serialised into staging_details.related_account_data at Initiate and
    /// rechecked line by line at approval.
    /// </summary>
    public class DtoEcsRdReceipt
    {
        public decimal MemId { get; set; }
        public string? MemberNo { get; set; }
        public string? MemberName { get; set; }
        public DateTime ApplicationDate { get; set; }   // business day
        public double ReceiptAmount { get; set; }       // total received = staging line amount
        public int CashOrAdjustment { get; set; }       // 1 cash, >1 adjustment (as voc_Trn_Type)
        public List<DtoEcsRdReceiptLine> Lines { get; set; } = new();
    }

    /// <summary>One RD of the member (spec 14 §4–§7). Figures are as on the business day.</summary>
    public class DtoEcsRdReceiptLine
    {
        public decimal TD_Id { get; set; }
        public string? TD_No { get; set; }
        public decimal TDScheme_Id { get; set; }
        public double Instalment { get; set; }          // D = termdeposit_master.DepositAmount
        public DateTime ValueDate { get; set; }
        public int PeriodInMonths { get; set; }
        public double RateOfInterest { get; set; }
        public DateTime? MaturityDate { get; set; }
        public double MaturityAmount { get; set; }
        public double PenalRate { get; set; }           // PenalRateForRD, a monthly rate (spec 14 §16)

        // Ledger sums over valid termdeposit_trn rows (§4)
        public double Received { get; set; }
        public double PenalCalculated { get; set; }
        public double PenalReceived { get; set; }
        public DateTime? LastPenalAppliedDate { get; set; }
        public DateTime? LastInstalmentDate { get; set; }
        public DateTime? LastReceiptDate { get; set; }

        // Computed (§5, §6)
        public int InstalmentsDue { get; set; }          // N
        public int InstalmentsReceived { get; set; }     // n
        public double RdPending { get; set; }
        public double Advance { get; set; }
        public double PiArrear { get; set; }
        public double PiCurrent { get; set; }
        public double PiPending { get; set; }

        // Appropriated (§7.2)
        public double RdReceipt { get; set; }
        public double PiReceipt { get; set; }

        public bool Locked { get; set; }                 // display only: in another pending acc 2 staging line
    }

    /// <summary>Response of GET api/rdreceipt/Rds/{memId}.</summary>
    public class EcsRdReceiptLoadVM
    {
        public DateTime ApplicationDate { get; set; }
        public List<DtoEcsRdReceiptLine> Lines { get; set; } = new();
        public string? Message { get; set; }
    }
}
