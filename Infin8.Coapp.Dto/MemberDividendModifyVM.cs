namespace Infin8.Coapp.Dto
{
    public class MemberDividendModifyVM
    {
        public int ID { get; set; }
        public DateTime Trn_Date { get; set; }
        public double IntCalc_Amt { get; set; }
        public DateTime? intCalc_Date { get; set; }
        public double IntPaid_Amt { get; set; }
        public double IntBal_Amt { get; set; }
    }
}
