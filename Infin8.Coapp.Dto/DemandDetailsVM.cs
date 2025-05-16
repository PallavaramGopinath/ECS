namespace Infin8.Coapp.Dto
{
    public class DemandDetailsVM
    {
        public int Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public double LoanDemand { get; set; }
        public double DepositDemand { get; set; }
        public double DueToDemand { get; set; }
        public int RDDemand { get; set; }
        public double TotalDemand { get; set; }

        public int Trn_Id { get; set; }

    }
}
