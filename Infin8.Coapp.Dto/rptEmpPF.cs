namespace Infin8.Coapp.Dto
{
    public class rptEmpPF
    {
        public string? Emp_No { get; set; }
        public string? MemberName { get; set; }
        public DateTime Pf_Date { get; set; }
        public double Pf_Subscription { get; set; }
        public double Pf_Withdrawn { get; set; }
        public double Pf_Balance { get; set; }
        public int No_Of_Days { get; set; }
        public double Pf_Product { get; set; }
        public double Epf_Interest { get; set; }
        public double Bpf_Contribution { get; set; }
        public double Bpf_Withdrawn { get; set; }
        public double Bpf_Balance { get; set; }
        public double Bpf_Product { get; set; }
        public double Bpf_Interest { get; set; }
        public double Total_Balance { get; set; }
        public double Vpf_Contribution { get; set; }
        public double Vpf_Withdrawn { get; set; }
        public double Vpf_Balance { get; set; }
        public decimal Pf_Id { get; set; }
    }
}
