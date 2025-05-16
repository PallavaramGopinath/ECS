namespace Infin8.Coapp.Dto
{
    public class RecoveryListVM
    {
        public int Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public int Demand_Id { get; set; }
        public int OfficeId { get; set; }
        public double TotalRecovery { get; set; }

        public double TotalLoanRecovery { get; set; }
        public double TotalDepositRecovery { get; set; }
        public double TotalDueToRecovery { get; set; }
        public double TotalDueByRecovery { get; set; }
        public int TotalDueByCollectionOnDemand { get; set; }
        public int TotalRDRecovery { get; set; }
    }
}
