namespace Infin8.Coapp.Dto
{
    public class LoanVM
    {
        public string? LoanNo { get; set; }
        public int LoanId { get; set; }
        public DateTime TrnDate { get; set; }
        public double PrlDeand { get; set; }
        public double IntDemand { get; set; }
        public double PIBalance { get; set; }
        public double PrlColl { get; set; }
        public double IntColl { get; set; }
        public double PIColl { get; set; }
    }

    public static class LoanVMList
    {
        public static List<LoanVM>? LoanList { get; set; }
    }
}
