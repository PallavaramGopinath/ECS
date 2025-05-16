namespace Infin8.Coapp.Dto
{
    public class PaySlipListVM
    {
        public int Pay_Id { get; set; }
        public int Emp_Id { get; set; }
        public string? MemberName { get; set; }
        public int Pay_Month { get; set; }
        public int Pay_Year { get; set; }

        public double Pay_Tot_Allowance { get; set; }
        public double Pay_Tot_Deductions { get; set; }
        public double Pay_Net { get; set; }

        public DateTime? from_date { get; set; }
        public DateTime? to_date { get; set; }

        public int DA_Id { get; set; }
    }
}
