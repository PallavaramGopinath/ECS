namespace Infin8.Coapp.Dto
{
    public class LoanDemandLedgerVM
    {
        public string? DemandType { get; set; }
        public string? AccountName { get; set; }
        public double? PIArrear { get; set; }
        public double? PICurrent { get; set; }
        public double? PITotal { get; set; }
        public double? IntArrear { get; set; }
        public double? IntCurrent { get; set; }
        public double? IntTotal { get; set; }
        public double? PrlArrear { get; set; }
        public double? PrlCurrent { get; set; }
        public Double? PrlTotal { get; set; }
        public double? TotalDemand { get; set; }
    }
}
