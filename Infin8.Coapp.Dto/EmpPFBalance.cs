namespace Infin8.Coapp.Dto
{
    public class EmpPFBalance
    {
        public int Emp_Id { get; set; }
        public double Pf_Subscription { get; set; }
        public double Pf_Withdrawn { get; set; }
        public double Vpf_Contribution { get; set; }
        public double Vpf_Withdrawn { get; set; }
        public double Bpf_Contribution { get; set; }
        public double Bpf_Withdrawn { get; set; }
        public double Pf_Interest { get; set; }
        public double Vpf_Interest { get; set; }
        public double Bpf_Interest { get; set; }
    }
}
