namespace Infin8.Coapp.Dto
{
    public class TDRateOfInterstDto
    {
        public decimal Roi_Id { get; set; }
        public int TDScheme_Id { get; set; }
        public string? TDScheme_Name { get; set; }
        public DateTime Wef { get; set; }
        public string? PeriodType { get; set; }
        public int   PeriodBegin { get; set; }
        public int PeriodEnd { get; set; }
        public double Roi { get; set; }
        public double PenalRateForRD { get; set; }
    }
}
