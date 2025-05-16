namespace Infin8.Coapp.Dto
{
    public class rptDayBook
    {
        public DateTime Voc_Date { get; set; }
        public string? Voc_Rpt_No { get; set; }
        public string? Voc_Pmt_No { get; set; }
        public string? Voc_No { get; set; }
        public int Led_Id { get; set; }
        public string? Led_Name { get; set; }
        public double Voc_Rpt { get; set; }
        public double Voc_Pmt { get; set; }
        public int Voc_Trn_Type { get; set; }
        public string? Voc_Narr { get; set; }
        public double Fin_db_ob { get; set; }
        public double Fin_db_cb { get; set; }
        public string? RsInWords { get; set; }
    }
}
