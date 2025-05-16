namespace Infin8.Coapp.Dto
{
    public class FinBal
    {
        public decimal Led_Id { get; set; }
        public int Fnl_Id { get; set; }
        public string? Fnl_Name { get; set; }
        public int Grp_Id { get; set; }
        public string? Grp_Name { get; set; }
        public string? Led_Name { get; set; }
        public DateTime Voc_Date { get; set; }
        public double OB_Amt { get; set; }
        public double TotalReceipts { get; set; }
        public double TotalPayments { get; set; }
        public double CB_Amt { get; set; }
        public int GLMonth { get; set; }
        public int GLYear { get; set; }

    }
}
