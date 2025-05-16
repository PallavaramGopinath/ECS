namespace Infin8.Coapp.Dto
{
    public class rptDayBook2
    {
        public DateTime Voc_Date { get; set; }
        public string? Voc_Rpt_No { get; set; }
        public string? Voc_Pmt_No { get; set; }
        public string? Voc_No { get; set; }
        public decimal Led_Id { get; set; }
        public string? Led_Name { get; set; }
        public double Voc_Cash_Rpt { get; set; }
        public double Voc_Adj_Rpt { get; set; }
        public double Voc_Rpt { get; set; }
        public double Voc_Cash_Pmt { get; set; }
        public double Voc_Adj_Pmt { get; set; }
        public double Voc_Pmt { get; set; }
        public int Voc_Trn_Type { get; set; }
        public string? Voc_Narr { get; set; }
        public double Fin_Db_Ob { get; set; }
        public double Fin_Db_Cb { get; set; }
        public string? RsInWords { get; set; }
        public int Led_SlNo { get; set; }
        public int Voc_Rpt_SlNo { get; set; }
        public int Voc_Pmt_SlNo { get; set; }
    }
}
