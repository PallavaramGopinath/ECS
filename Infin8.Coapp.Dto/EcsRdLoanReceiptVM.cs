using System;
using System.Collections.Generic;

namespace Infin8.Coapp.Dto
{
    /// <summary>
    /// ECS Recurring Deposit Loan Receipt, acc 25 (spec 18 §10). Serialised into staging_details.related_account_data at
    /// Initiate and rechecked figure by figure at approval.
    /// </summary>
    public class DtoEcsRdLoanReceipt
    {
        public decimal MemId { get; set; }
        public string? MemberNo { get; set; }
        public string? MemberName { get; set; }
        public DateTime ApplicationDate { get; set; }
        public int CashOrAdjustment { get; set; }          // 1 cash, >1 adjustment
        public List<DtoEcsRdLoanReceiptLine> Lines { get; set; } = new();   // loans with something received
        public double TotalDue { get; set; }
        public double TotalReceived { get; set; }                           // = staging Receipt_Amount
        public double TotalInterest { get; set; }
        public double TotalPrincipal { get; set; }
    }

    /// <summary>One open loan on RD of the member (spec 18 §4.2).</summary>
    public class DtoEcsRdLoanReceiptLine
    {
        public decimal Loan_Id { get; set; }
        public string? Loan_No { get; set; }
        public long Scheme_Id { get; set; }
        public DateTime DisbursedOn { get; set; }
        public double Disbursed { get; set; }
        public double Roi { get; set; }
        public string? PledgedRdNos { get; set; }
        public DateTime LastDueDate { get; set; }
        public double PrincipalOutstanding { get; set; }
        public DateTime InterestFrom { get; set; }
        public DateTime InterestTo { get; set; }
        public double InterestArrear { get; set; }
        public double InterestCurrent { get; set; }          // stamped as IntCalc_Amt
        public double InterestDue { get; set; }
        public double InterestReceived { get; set; }
        public double PrincipalReceived { get; set; }
        public bool ClosesLoan { get; set; }

        /// <summary>Screen only: null = the loan can be received (e.g. not locked by another pending acc 25 line).</summary>
        public string? BlockReason { get; set; }
    }

    /// <summary>Response of GET api/rdloanreceipt/Loans/{memId} (spec 18 §4.1).</summary>
    public class EcsRdLoanReceiptSetupVM
    {
        public DateTime ApplicationDate { get; set; }
        public List<DtoEcsRdLoanReceiptLine> Lines { get; set; } = new();

        /// <summary>Informational, e.g. "The member has no open loan on RD".</summary>
        public string? Message { get; set; }

        /// <summary>A problem that blocks Initiate for every loan (business day, scheme). Null when the receipt can be made.</summary>
        public string? SetupError { get; set; }
    }
}
