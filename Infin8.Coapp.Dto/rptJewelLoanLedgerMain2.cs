namespace Infin8.Coapp.Dto
{
    public  class rptJewelLoanLedgerMain2
    {
        ///  Preamble
        public int Loan_Id { get; set; }
        public int Mem_Id { get; set; }
        public  string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public string? Token_PersonNo { get; set; }
        public string? Address { get; set; }
        public string? MobileNo { get; set; }
        public string? PANNo { get; set; }
        public string? AadharNo { get; set; }
        public string? SmartCardNo { get; set; }
        public byte[]? PhotoImage { get; set; }
        public byte[]? JewelsImage { get; set; }
        public string? Loan_No { get; set; }
        public double San_Amt { get; set; } = 0;
        public DateTime San_Date { get; set; }
        public DateTime JL_DueDate { get; set; }
        public double Roi { get; set; } = 0;
        public double Pi { get; set; } = 0;
        public double RatePerGram { get; set; } = 0;
        public double MarketRatePerGram { get; set; } = 0;
        public double GrossWeight { get; set; } = 0;
        public double Wastage { get; set; } = 0;
        public double NetWeight { get; set; } = 0;
        public double NetValue { get; set; } = 0;

        /// Running loan transaction

        public DateTime Trn_Date { get; set; }
        public double Disb_Amt { get; set; }
        public double PICalc_Amt { get; set; }
        public DateTime? PICalc_Date { get; set; }
        public double IntCalc_Amt { get; set; }
        public DateTime? IntCalc_Date { get; set; }
        public double PIColl_Amt { get; set; }
        public double IntColl_Amt { get; set; }
        public double PrlColl_Amt { get; set; }
        public double Prl_OS { get; set; }
        public double Int_Bal { get; set; }
        public double PI_Bal { get; set; }
    }
}
