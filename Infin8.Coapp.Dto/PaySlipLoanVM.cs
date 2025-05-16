namespace Infin8.Coapp.Dto
{
    public class PaySlipLoanVM
    {
        public int Id { get; set; }
        public string? Type { get; set; }
        public string? Ded_Name { get; set; }
        public double Amount { get; set; }
        public double Prl_Coll { get; set; }
        public double Int_Coll { get; set; }
        public double Int_Calc { get; set; }
        public DateTime? Int_CalcDate { get; set; }
        public double Prl_Dem { get; set; }
    }
}
