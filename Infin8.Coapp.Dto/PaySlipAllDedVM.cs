namespace Infin8.Coapp.Dto
{
    public class PaySlipAllDedVM
    {
        public int All_Id { get; set; }
        public int All_Type { get; set; }
        public string? All_Name { get; set; }
        public double Allowance_Amt { get; set; }
        public int Ded_Id { get; set; }
        public int Ded_Type { get; set; }
        public int Led_Id { get; set; }
        public int Loan_Id { get; set; }
        public string? Ded_Name { get; set; }
        public double Deduction_Amt { get; set; }
        public double Amt_Coll { get; set; }
        public double Prl_Coll { get; set; }
        public double Int_Coll { get; set; }
        public double Int_Calc_Amt { get; set; }
        public DateTime? Int_Calc_Upto { get; set; }
        public string? Led_Name { get; set; }
        public string? Scheme_Name { get; set; }
        
    }
}
