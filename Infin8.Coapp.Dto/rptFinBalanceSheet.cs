namespace Infin8.Coapp.Dto
{
    public class rptFinBalanceSheet
    {
        public decimal Led_Id { get; set; }
        public int Fnl_Id { get; set; }
        public string? Fnl_Name { get; set; }
        public int Grp_Id { get; set; }
        public string? Grp_Name { get; set; }
        public string? Led_Name { get; set; }
        public double OB_Amt { get; set; }
        public double CB_Amt { get; set; }
        public double Profit { get; set; }
        public double Loss { get; set; }
    }
}
