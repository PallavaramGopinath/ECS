namespace Infin8.Coapp.Dto
{
    public class ViewTDNew
    {
        public int Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public string? FatherName { get; set; }
        public int TD_Id { get; set; }
        public string? TD_No { get; set; }
        public int TDScheme_Id { get; set; }
        public string? TDScheme_Name { get; set; }
        public string? TDH_Name { get; set; }
        public DateTime ValueDate { get; set; }
        public double DepositAmount { get; set; }
        public int InterestPayableFrequency { get; set; }
        public int PeriodInMonths { get; set; }
        public int PeriodInDays { get; set; }
        public double MaturityAmount { get; set; }
        public DateTime MaturityDate { get; set; }
        public double RateOfInterest { get; set; }
        public double PenalRate { get; set; }
        public bool IsDiscountRate { get; set; }
        public bool IsCompoundInterest { get; set; }
        public int CompoundFrequency { get; set; }
        public string? Nominee1Name { get; set; }
        public int Nominee1Age { get; set; }
        public string? Nominee1Relationship { get; set; }
        public string? Nominee2Name { get; set; }
        public int Nominee2Age { get; set; }
        public string? Nominee2Relationship { get; set; }
        public int Voc_Id { get; set; }
        public int Age { get; set; }
    }
}
