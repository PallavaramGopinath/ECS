namespace Infin8.Coapp.Dto
{
    public class FDDetailsVM
    {
        public decimal FDId { get; set; }
        public string? TDScheme_Name { get; set; }
        public string? FDNo { get; set; }
        public int FDSchemeId { get; set; }
        public DateTime FDValueDate { get; set; }
        public double FDAmount { get; set; }
        public int FDPrdInMonths { get; set; }
        public int FDPrdInDays { get; set; }
        public int FDIntPayableFrequency { get; set; }
        public int FDCompoundFrequency { get; set; }
        public double FDROI { get; set; }
        public bool FDIsDiscountRate { get; set; }
        public double FDMaturityAmount { get; set; }
        public DateTime FDMaturityDate { get; set; }
        public double FDIntAlreadyCalculated { get; set; }
        public DateTime? FDIntAlreadyCalculatedDate { get; set; }
        public double FDIntAlreadyPaid { get; set; }
        public double FDIntCalculatedNow { get; set; }
        public DateTime? FDIntCalculatedDateNow { get; set; }
        public double FDIntPayable { get; set; }
        public double FDAmountRefund { get; set; }
        public double FDROIApplied { get; set; }
    }
}
