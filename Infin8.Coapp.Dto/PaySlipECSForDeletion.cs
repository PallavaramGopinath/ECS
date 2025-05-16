namespace Infin8.Coapp.Dto
{
    public class PaySlipECSForDeletion
    {
        public int Emp_Id { get; set; }
        public int Pay_Id { get; set; }
        public string? pay_des { get; set; }
        public string? MemberNo { get; set; }
        public string? MemberName { get; set; }
        public double Pay_Tot_Allowance { get; set; }
        public double Pay_Tot_Deductions { get; set; }
        public double Pay_Net { get; set; }
        public double Pay_PF { get; set; }
    }
}
