namespace Infin8.Coapp.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class Map_General
    {
        [Key]
        public decimal ID { get; set; }
        public decimal Society_Id { get; set; }
        //public decimal Mem_Share_Capital_Led_Id { get; set; }
        public bool IsMemNo { get; set; }

        [Required(ErrorMessage = "Share Capital Ledger is required")]
        [Range(1, double.MaxValue, ErrorMessage = "Ledger Id must be greater than 0 (not selected)")]
        public decimal ShareCapital_Led_Id { get; set; }

        [Required(ErrorMessage = "Dividend Ledger is required")]
        [Range(1, double.MaxValue, ErrorMessage = "Dividend Id must be greater than 0 (not selected)")]
        public decimal Dividend_Led_Id { get; set; }

        [Required(ErrorMessage = "Cash Ledger Ledger is required")]
        [Range(1, double.MaxValue, ErrorMessage = "Cash Ledger Id must be greater than 0 (not selected)")]
        public decimal Cash_Led_Id { get; set; }
        public int SF_Code { get; set; }
        public int SC_Code { get; set; }
        public int ST_Code { get; set; }

        [Required(ErrorMessage = "Fixed Deposit Ledger is required")]
        [Range(1, double.MaxValue, ErrorMessage = "Fixed Deposit Id must be greater than 0 (not selected)")]
        public decimal FD_Led_Id { get; set; }

        [Required(ErrorMessage = "Intererest Paid On Fixed Deposit Ledger is required")]
        [Range(1, double.MaxValue, ErrorMessage = "Interest Paid On Fixed Deposit Id must be greater than 0 (not selected)")]
        public decimal FD_Int_Led_Id { get; set; }

        [Required(ErrorMessage = "Interest Received on Pre-Maturied Fixed Deposit Ledger is required")]
        [Range(1, double.MaxValue, ErrorMessage = "Interest Received on Pre-Maturied Fixed Deposit Id must be greater than 0 (not selected)")]
        public decimal FD_Excess_IntPaid_Id { get; set; }
        public decimal RD_Led_Id { get; set; }
        public decimal RD_Int_Led_Id { get; set; }
        public decimal RD_PI_Led_Id { get; set; }
        public decimal CTC_DueBy_Led_Id { get; set; }
        public double MaxLoanLimit { get; set; }
        public int ThriftDeposit_DMId { get; set; }
        public decimal FamilyWelfareSchemeLed_Id { get; set; }
        public decimal FamilyWelfareDepsitDM_Id { get; set; }
        public int MinInstalmentsForLoan { get; set; }
        public int JL_minimum_Days { get; set; }
        public bool FDAutoRenewal { get; set; }
        public int FDRenewalPeriod { get; set; }
        //public int SBACLed_Id { get; set; }
        //public int SBACIntLed_Id { get; set; }
        public int TSISRetirementAgeInMonths { get; set; }
        public int DemandRestrictedAmount { get; set; }
        public bool IsNewModeForChequeRpt { get; set; }
        public bool IsFDIntCalcOnMonthBasis { get; set; }
        public double ShareCapitalPercentageOnLoanOS { get; set; }
        public string? BrCode { get; set; }

        [Required(ErrorMessage = "Appraisal Fee Ledger is required")]
        [Range(1, double.MaxValue, ErrorMessage = "Appraisal Fee Id must be greater than 0 (not selected)")]
        public decimal Appraisal_Fee_Led_Id { get; set; }

        [Required(ErrorMessage = "Bank Charges Ledger is required")]
        [Range(1, double.MaxValue, ErrorMessage = "Bank Charges Id must be greater than 0 (not selected)")]
        public decimal Bank_Charges_Led_Id { get; set; }

        [Required(ErrorMessage = "Service Tax Ledger is required")]
        [Range(1, double.MaxValue, ErrorMessage = "Service Tax Id must be greater than 0 (not selected)")]
        public decimal Service_Tax_Led_Id { get; set; }
    }
}
