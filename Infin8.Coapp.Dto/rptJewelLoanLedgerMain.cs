namespace Infin8.Coapp.Dto
{
    public  class rptJewelLoanLedgerMain
    {
        public decimal Loan_Id { get; set; }
        public decimal Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public string? Token_PersonNo { get; set; }
        public string? Address { get; set; }
        public string? MobileNo { get; set; }
        public string? PANNo { get; set; }
        public string? AadharNo { get; set; }
        public string? SmartCardNo { get; set; }
        public string? PhotoImage { get; set; }
        public string? JewelsImage { get; set; }
        public string? Loan_No { get; set; }
        public double San_Amt { get; set; }
        public DateTime San_Date { get; set; }
        public DateTime JL_DueDate { get; set; }
        public double Roi { get; set; }
        public double Pi { get; set; }
        public double RatePerGram { get; set; }
        public double MarketRatePerGram { get; set; }
        public double GrossWeight { get; set; }
        public double Wastage { get; set; }
        public double NetWeight { get; set; }
        public double NetValue { get; set; }
    }
}
