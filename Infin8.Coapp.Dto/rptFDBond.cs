namespace Infin8.Coapp.Dto
{
    public  class rptFDBond
    {
        public DateTime AccountOpenDate { get; set; }
        public DateTime ValueDate { get; set; }
        public string? TDScheme_Name { get; set; }
        public string? TD_No { get; set; }
        public double DepositAmount { get; set; }
        public DateTime MaturityDate { get; set; }
        public double MaturityAmount { get; set; }
        public int InterestPayableFrequency { get; set; }
        public string? TDFrequency { get; set; }
        public int PeriodInMonths { get; set; }
        public int PeriodInDays { get; set; }
        public double RateOfInterest { get; set; }
        public string? MemberNo { get; set; }
        public string? MemberName { get; set; }
        public string? TDH_Name { get; set; }
        public string? MobileNo { get; set; }
        public string? Nominee1Name { get; set; }
        public int Nominee1Age { get; set; }
        public string? Nominee1Relationship { get; set; }
        public string? Nominee2Name { get; set; }
        public int Nominee2Age { get; set; }
        public string? Nominee2Relationship { get; set; }
        public string? RsInWords { get; set; }
        public string? Voc_Rpt_No { get; set; }
        public string? Voc_No  { get; set; }
        public string? Address { get; set; }
        public string? RenewalTD_No { get; set; }

        public int ModeOfOperation { get; set; }
        public string? ModeOfOperationString { get; set; }
    }
}
