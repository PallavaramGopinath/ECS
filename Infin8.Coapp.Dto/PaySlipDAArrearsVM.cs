namespace Infin8.Coapp.Dto
{
    public class PaySlipDAArrearsVM
    {
        public int Mem_Id { get; set; }
        public string? MemberName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public double TotalBasicPay { get; set; }
        public double SLS { get; set; }
        public double LLP { get; set; }
        public double DAArrears { get; set; }
        public double PF { get; set; }
        public double NetDAArrears { get; set; }
    }
}
