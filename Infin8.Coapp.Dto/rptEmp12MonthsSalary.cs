namespace Infin8.Coapp.Dto
{
    public  class rptEmp12MonthsSalary
    {
        public decimal Emp_Id { get; set; }
        public string? MemberName { get; set; }
        public string? Emp_Desgn { get; set; }
        public string? SalaryMonth { get; set; }
        public int Pay_Month { get; set; }
        public int Pay_Year { get; set; }
        public string? Pay_Des { get; set; }
        public double Pay_Basic_Earned { get; set; }
        public double Pay_GradePay_Earned { get; set; }
        public double Pay_PP_Earned { get; set; }
        public double Pay_DA_Earned { get; set; }
        public double OtherAllowances { get; set; }
        public double Pay_Tot_Allowance { get; set; }
        public double Pay_PF { get; set; }
        public double Pay_VPF { get; set; }
        public double IT { get; set; }
        public double OtherDeductions { get; set; }
        public double Pay_Tot_Deductions { get; set; }
        public double Pay_Net { get; set; }
        public double Pay_SLS { get; set; }
        public double Pay_Exgratia { get; set; }
        public double Pay_Bonus { get; set; }
    }
}
