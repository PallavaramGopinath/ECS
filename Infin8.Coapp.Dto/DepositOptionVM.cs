namespace Infin8.Coapp.Dto
{
    public class DepositOptionVM
    {
        public int DM_Id { get; set; }
        public int Mem_Id { get; set; }
        public DateTime DO_Wef { get; set; }
        public double OptionAmount { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public string? DMName { get; set; }
    }
}
