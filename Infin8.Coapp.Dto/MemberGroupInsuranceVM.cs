namespace Infin8.Coapp.Dto
{
    public class MemberGroupInsuranceVM
    {
        public int SlNo { get; set; }
        public int Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public int LoanOS { get; set; }
        public int FWDBal { get; set; }
        public int PolicyAmt { get; set; }
        public double PremiumAmt { get; set; }
        public double SurchargeAmt { get; set; }
        public double TotalAmt { get; set; }
        public double TotalRoundedAmt { get; set; }
        public double Inst_Amt { get; set; }
        public DateTime? Dob { get; set; }
        public int GrossPay { get; set; }
        public int GiTrn_Id { get; set; }
    }
}
