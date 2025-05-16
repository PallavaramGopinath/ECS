namespace Infin8.Coapp.Dto
{
    public class FDPaymentItemsVM
    {
        public int FDId { get; set; }
        public string? FDNo { get; set; }
        public double FDIntAlreadyCalculated { get; set; }
        public double FDIntAlreadyPaid { get; set; }
        public double FDIntCalculatedNow { get; set; }
        public DateTime? FDIntCalculatedDateNow { get; set; }
        public double FDIntPayable { get; set; }
        public double FDAmountRefund { get; set; }
    }
}
