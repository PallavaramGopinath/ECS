namespace Infin8.Coapp.Dto
{
    /// <summary>What the ECS Loan Disbursement page posts (spec 10 §6). Context comes from claims, never the body.</summary>
    public class EcsDisbursementRequestVM
    {
        public List<decimal> SanctionIds { get; set; } = new();
        /// <summary>"C" cheque, "N" NEFT.</summary>
        public string Mode { get; set; } = "C";
        public decimal BankLedId { get; set; }
        public string InstrumentNo { get; set; } = "";
        public DateTime InstrumentDate { get; set; }
        /// <summary>Beneficiary bank name typed for NEFT.</summary>
        public string? BankName { get; set; }
        public string? Narration { get; set; }
    }

    public class EcsBankLedgerVM
    {
        public decimal LedId { get; set; }
        public string? LedName { get; set; }
    }

    /// <summary>One fin_voucher_trn line as previewed.</summary>
    public class EcsVoucherLineVM
    {
        public decimal SanctionId { get; set; }
        public string? Status { get; set; }
        public decimal LedId { get; set; }
        public string? LedgerName { get; set; }
        public string? MemberNo { get; set; }
        public string? MemberName { get; set; }
        public string? Narration { get; set; }
        public double Debit { get; set; }
        public double Credit { get; set; }
    }

    /// <summary>One fin_voucher_bank row as previewed (one per sanction / member).</summary>
    public class EcsBankRowVM
    {
        public decimal SanctionId { get; set; }
        public string? MemberNo { get; set; }
        public string? MemberName { get; set; }
        public double Amount { get; set; }
        public string? Instrument { get; set; }
    }

    public class EcsDisbursementPreviewVM
    {
        public bool IsValid { get; set; }
        /// <summary>Blocking problems — nothing can be disbursed while any exist.</summary>
        public List<string> Messages { get; set; } = new();
        /// <summary>Non-blocking notices (e.g. cheque no. already used).</summary>
        public List<string> Warnings { get; set; } = new();
        public List<EcsVoucherLineVM> Lines { get; set; } = new();
        public List<EcsBankRowVM> BankRows { get; set; } = new();
        public double TotalDebit { get; set; }
        public double TotalCredit { get; set; }
        public double TotalLoan { get; set; }
        public double TotalRecoverable { get; set; }
        public double TotalNet { get; set; }
        public DateTime VoucherDate { get; set; }
        public string? BankLedgerName { get; set; }
    }

    public class EcsDisbursedLoanVM
    {
        public decimal SanctionId { get; set; }
        public decimal LoanId { get; set; }
        public string? LoanNo { get; set; }
        public string? MemberNo { get; set; }
        public string? MemberName { get; set; }
        public double LoanAmount { get; set; }
        public double Net { get; set; }
    }

    public class EcsDisbursementResultVM
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<string> Reasons { get; set; } = new();
        public decimal VocId { get; set; }
        public string? VocNo { get; set; }
        public List<EcsDisbursedLoanVM> Loans { get; set; } = new();
    }
}
