namespace Infin8.Coapp.Dto
{
    public class DemandStatusDetailsVM
    {
        public int Demand_Id { get; set; }
        public int OfficeId { get; set; }
        public string? ReferName { get; set; }
        public bool IsAppropriated { get; set; }
        public bool IsRecoverySaved { get; set; }
    }
}
