namespace Infin8.Coapp.Dto
{
    public class rptTermDepositRegister
    {
        /// Term Deposit Master
        public decimal TD_Id { get; set; }
        public string? TD_No { get; set; }
        public int TDScheme_Id { get; set; }
        public decimal Mem_Id { get; set; }
        public string? TDH_Name { get; set; }
        public int TDH_Age { get; set; }
        public int ModeOfOperation { get; set; }
        public DateTime ValueDate { get; set; }
        public double DepositAmount { get; set; }
        public int PeriodInMonths { get; set; }
        public int PeriodInDays { get; set; }
        public double RateOfInterest { get; set; }
        public DateTime MaturityDate { get; set; }
        public double MaturityAmount { get; set; }
        public int InterestPayableFrequency { get; set; }
        public double PenalRate { get; set; }
        public bool IsDiscountRate { get; set; }
        public string? Nominee1Name { get; set; }
        public int Nominee1Age { get; set; }
        public string? Nominee1Relationship { get; set; }
        public string? Nominee2Name { get; set; }
        public int Nominee2Age { get; set; }
        public string? Nominee2Relationship { get; set; }
        public string? Status { get; set; }
        public string? RenewalTD_No { get; set; }

        /// Term Deposit Trn
        public DateTime Trn_Date { get; set; }
        public double DepositReceiptAmount { get; set; }
        public double InterestCalculatedAmount { get; set; }
        public DateTime? InterestAppliedDate { get; set; }
        public double InterestPaidAmount { get; set; }
        public double DepositPaidAmount { get; set; }
        public double PenalCalculatedAmount { get; set; }
        public DateTime? PenalAppliedDate { get; set; }
        public double PenalReceivedAmount { get; set; }

        /// Member data
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public string? FatherName { get; set; }
        public string? PerAdd1 { get; set; }
        public string? PerAdd2 { get; set; }
        public string? PerAdd3 { get; set; }
        public string? MobileNo { get; set; }
        public string? PANNo { get; set; }
        public string? AadharNo { get; set; }
        public string? SmartCardNo { get; set; }
        public string? memberphoto { get; set; }

        /// TermDeposit scheme
        public string? TDScheme_Name { get; set; }
        public string? TDSchemeType { get; set; }



    }
}
