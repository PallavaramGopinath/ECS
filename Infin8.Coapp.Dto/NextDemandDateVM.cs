namespace Infin8.Coapp.Dto
{
    public class NextDemandDateVM
    {
        public DateTime NextDemandDate { get; set; }
        public bool IsDemandUpdated { get; set; }
        public bool IsAppropriated { get; set; }
        public bool IsRecoverySaved { get; set; }
        public string? ReferName { get; set; }
    }
}
