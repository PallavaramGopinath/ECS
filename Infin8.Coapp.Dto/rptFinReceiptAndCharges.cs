namespace Infin8.Coapp.Dto
{
    public class rptFinReceiptAndCharges
    {
        public decimal Led_Id { get; set; }
        public string? Led_Name { get; set; }
        public int Grp_Id { get; set; }
        public string? Grp_Name { get; set; }
        public double Beg_Rpt { get; set; }
        public double Beg_Pmt { get; set; }
        public double Dur_Rpt { get; set; }
        public double Dur_Pmt { get; set; }
        public double Tot_Rpt { get; set; }
        public double Tot_Pmt { get; set; }
        public double Rpt_Dur_OB { get; set; }
        public double Rpt_Beg_OB { get; set; }
        public double Pmt_End_CB { get; set; }
        public int Fnl_Id { get; set; }
    }
}
