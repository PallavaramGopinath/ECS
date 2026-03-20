namespace Infin8.Coapp.Dto
{
    public class FinBalGL
    {
        public string? GL_Month { get; set; }
        public decimal Led_Id { get; set; }
        public string? Led_Name { get; set; }
        public DateTime GL_Date { get; set; }
        public double GL_OB { get; set; }
        public double GL_Receipts { get; set; }
        public double GL_Payments { get; set; }
        public double GL_CB { get; set; }
        public double GL_PreMonthRpt { get; set; }
        public double GL_PreMonthPmt { get; set; }
    }
}
