namespace Infin8.Coapp.Dto
{
    public class rptFinGeneralLedger
    {
        public decimal Led_Id { get; set; }
        public int Fnl_Id { get; set; }
        public string? Led_Name { get; set; }
        public string? GL_Month { get; set; }
        public string? GL_MonthName { get; set; }
        public DateTime GL_Date { get; set; }
        public double OpeningBalance { get; set; }
        public double Receipts { get; set; }
        public double Payments { get; set; }
        public double ClosingBalance { get; set; }
        public double PreviousRpt { get; set; }
        public double PreviousPmt { get; set; }
        public double TotalReceipts { get; set; }
        public double TotalPayments { get; set; }
    }
}
