namespace Infin8.Coapp.Dto
{
    public class rptEmpPayBill
    {
        public decimal Mem_Id { get; set; }
        public string? MemberName { get; set; }
        public string? Emp_Desgn { get; set; }
        public DateTime? Doj { get; set; }
        public string? Emp_Scale { get; set; }
        public int  Pay_Month { get; set; }
        public int Pay_Year { get; set; }
        public DateTime? Dob { get; set; }
        public DateTime? Dor { get; set; }
        public double Pay_Basic_Earned { get; set; }
        public double Pay_DA_Earned { get; set; }
        public double Pay_GradePay_Earned { get; set; }
        public double Pay_PF { get; set; }
        public double Pay_VPF { get; set; }
        public string? All_Name { get; set; }
        public double Allowance_Amt { get; set; }
        public string? Led_Name { get; set; }
        public string? Scheme_Name { get; set; }
        public double Deduction_Amt { get; set; }
        public double Pay_Tot_Allowance { get; set; }
        public double Pay_Tot_Deductions { get; set; }
        public double Pay_Net { get; set; }
        public decimal Ded_Id { get; set; }
        public decimal Loan_Id { get; set; }
        public decimal Led_Id { get; set; }
        public string? RsInWords { get; set; }
    }
}
