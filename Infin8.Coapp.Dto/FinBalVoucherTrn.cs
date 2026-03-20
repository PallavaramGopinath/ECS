namespace Infin8.Coapp.Dto
{
    public class FinBalVoucherTrn
    {
        public decimal Voc_Id { get; set; }
        public string? Voc_No { get; set; }
        //public string? Voc_Rpt_No { get; set; }
        //public string? Voc_Pmt_No { get; set; }
        public DateTime Voc_Date { get; set; }
        public int Voc_Type { get; set; }
        //public int Voc_Trn_Type { get; set; }
        public decimal Led_Id { get; set; }
        public string? Led_Name { get; set; }
        public double Voc_Rpt { get; set; }
        public double Voc_Pmt { get; set; }
        //public string? Voc_Narr { get; set; }
        //public string? Voc_Narration { get; set; }

    }
}
