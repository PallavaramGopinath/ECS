namespace Infin8.Coapp.Dto
{
    public class DividendOrIntOnTDPaymentVM
    {
        public int PbleMaster_Id { get; set; }
        public long Mem_Id { get; set; }
        public int Led_Id { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int PbleType { get; set; }
        public int IntCalc_Amt { get; set; }
        public int IntPaid_Amt  { get; set;}
        public int Int_Bal { get; set; }
    }
}
