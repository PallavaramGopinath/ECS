namespace Infin8.Coapp.Dto
{
    public class rptFDPaymentList
    {
        public DateTime Voc_Date { get; set; }
        public string? Voc_Pmt_No { get; set; }
        public string? TD_No { get; set; }
        public string? TDH_Name { get; set; }
        public DateTime ValueDate { get; set; }
        public double DepositAmount { get; set; }
        public DateTime MaturityDate { get; set; }
        public double MaturityAmount { get; set; }
        public double RateOfInterest { get; set; }
        public DateTime? InterestAppliedDate { get; set; }
        public double InterestPaidAmount { get; set; }
        public double DepositPaidAmount { get; set; }
        public string? ChequeDetails { get; set; }
        public string? BrCode { get; set; }
    }
}
