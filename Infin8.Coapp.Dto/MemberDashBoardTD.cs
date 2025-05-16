namespace Infin8.Coapp.Dto
{
    public  class MemberDashBoardTD
    {
        public int Mem_Id { get; set; }
        public int TD_Id { get; set; }
        public int TDScheme_Id { get; set; }
        public string? TDScheme_Name { get; set; }
        public string? TD_No { get; set; }
        public string? TDH_Name { get; set; }
        public int PeriodInMonths { get; set; }
        public int PeriodInDays { get; set; }
        public double RateOfInterest { get; set; }
        public string? ValueDate { get; set; }
        public double TDAmt { get; set; }
        public string? MaturityDate { get; set; }
        public double MaturityAmount { get; set; }
        public double IntCalc { get; set; }
        public double IntPaid { get; set; }
        public string? IntCalcDate { get; set; }
        public bool AccountClosed { get; set; }

    }
}
