using System;
using System.Collections.Generic;

namespace Infin8.Coapp.Dto
{
    /// <summary>
    /// ECS Loan on Recurring Deposit, acc 14 (spec 17 §10). Serialised into staging_details.related_account_data at
    /// Initiate and rechecked figure by figure at approval.
    /// </summary>
    public class DtoEcsRdLoan
    {
        public decimal MemId { get; set; }
        public string? MemberNo { get; set; }
        public string? MemberName { get; set; }
        public DateTime ApplicationDate { get; set; }      // business day = sanction / disbursement date
        public int CashOrAdjustment { get; set; }          // 1 cash, >1 adjustment

        public long Scheme_Id { get; set; }
        public string? SchemeName { get; set; }
        public string? ProvisionalLoanNo { get; set; }     // display only; allocated at approval
        public string? LoanNo { get; set; }                // filled by the View after approval, if read back

        public List<DtoEcsRdLoanLine> Lines { get; set; } = new();   // selected RDs only
        public double EligPer { get; set; }
        public double TotalReceived { get; set; }
        public double BorrowingPower { get; set; }
        public double ExistingOutstanding { get; set; }
        public double EligibleAmount { get; set; }
        public double RdRoiMax { get; set; }
        public double Margin { get; set; }
        public double LoanRoi { get; set; }
        public DateTime DueDate { get; set; }
        public double LoanAmount { get; set; }             // = staging Payment_Amount
    }

    /// <summary>One open RD of the member that can be pledged (spec 17 §4.1, §4.2).</summary>
    public class DtoEcsRdLoanLine
    {
        public decimal TD_Id { get; set; }
        public string? TD_No { get; set; }
        public long TDScheme_Id { get; set; }
        public double Instalment { get; set; }
        public DateTime ValueDate { get; set; }
        public int PeriodInMonths { get; set; }
        public double RateOfInterest { get; set; }
        public DateTime MaturityDate { get; set; }
        public double MaturityAmount { get; set; }
        public double Received { get; set; }
        public double DrawingPower { get; set; }
        public string? BlockReason { get; set; }           // null = selectable
        public bool Selected { get; set; }
    }

    /// <summary>Response of GET api/rdloan/Setup/{memId} (spec 17 §4.1).</summary>
    public class EcsRdLoanSetupVM
    {
        public DateTime ApplicationDate { get; set; }
        public long Scheme_Id { get; set; }
        public string? SchemeName { get; set; }
        public string? ProvisionalLoanNo { get; set; }
        public double EligPer { get; set; }
        public double Margin { get; set; }
        public List<DtoEcsRdLoanLine> Lines { get; set; } = new();

        /// <summary>Informational, e.g. "The member has no open recurring deposits".</summary>
        public string? Message { get; set; }

        /// <summary>
        /// A setup problem that blocks Initiate for every RD (business day, scheme, eligibility %, ROI margin, member
        /// status). Null when the loan can be made.
        /// </summary>
        public string? SetupError { get; set; }
    }
}
