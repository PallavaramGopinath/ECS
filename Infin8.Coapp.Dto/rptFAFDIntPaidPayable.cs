namespace Infin8.Coapp.Dto
{
    public  class rptFAFDIntPaidPayable
    {
        public int Td_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? MemberName { get; set; }
        public string? TDH_Name { get; set; }
        public string? TD_No { get; set; }
        public double DepositAmount { get; set; }
        public DateTime ValueDate { get; set; }
        public DateTime MaturityDate { get; set; }
        public double RateOfInterest { get; set; }
        public int PeriodInMonths { get; set; }
        public int PeriodInDays { get; set; }
        public DateTime? InterestAppliedDate { get; set; }
        public double IntPaidBeforePeriod { get; set; }
        public double IntPaidDuringPeriod { get; set; }
        public double IntPayable { get; set; }
        public double DepositPaidAmount { get; set; }

    }
}
