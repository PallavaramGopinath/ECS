namespace Infin8.Coapp.Dto
{
    public class ViewPFWithdrawalVM
    {
        public int Emp_Id { get; set; }
        public string? MemberName { get; set; }
        public double Pf_Withdrawn { get; set; }
        public double vpb_withdrawn { get; set; }
        public double Bpf_Withdrawn { get; set; }
        public double Epf_Int_Withdrawn { get; set; }
        public double Bpf_Int_Withdrawn { get; set; }
        public double Epf_Interest { get; set; }
        public double Bpf_Interest { get; set; }
        public DateTime? Last_Int_ApplicationDate { get; set; }
    }
}
