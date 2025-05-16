namespace Infin8.Coapp.Dto
{
    public  class rptFDIntCalc
    {
        public int TDScheme_Id { get; set; }
        public string? TDScheme_Name { get; set; }
        public int TD_Id { get; set; }
        public string? TD_No { get; set; }
        public string? TDH_Name { get; set; }
        public DateTime Trn_Date { get; set; }
        public double InterestCalculatedAmount { get; set; }
        public DateTime InterestAppliedDate { get; set; }
    }
}
