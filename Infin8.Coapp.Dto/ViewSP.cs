namespace Infin8.Coapp.Dto
{
    public class ViewSP
    {
        public int Emp_Id { get; set; }
        public int Pay_Id { get; set; }
        public string? EmpNo { get; set; }
        public string? MemberName { get; set; }
        public string? Pay_Info_Name { get; set; }
        public double Pay_Tot_Allowance { get; set; }
        public double Pay_Tot_Deductions { get; set; }
        public double Pay_Net { get; set; }
        public int Pay_Month { get; set; }
        public int Pay_Year { get; set; }
        public double Pay_PF { get; set; }
    }
}
