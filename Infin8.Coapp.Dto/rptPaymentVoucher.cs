namespace Infin8.Coapp.Dto
{
    public class rptPaymentVoucher
    {
        public string? Voc_Pmt_No { get; set; }
        public DateTime Voc_Date { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public string? Led_Name { get; set; }
        public string? Description { get; set; }
        public string? Voc_Narr { get; set; }
        public double PaymentAmt { get; set; }
        public int Grp_SlNo { get; set; }
        public int Led_SlNo { get; set; }
        public string? Led_Desc { get; set; }
    }

}
