namespace Infin8.Coapp.Dto
{
    public class rptMemberBulkTrn
    {
        public int FromMem_Id { get; set; }
        public string? FromMem_No { get; set; }
        public string? FromMem_Name { get; set; }
        public int FromLed_Id { get; set; }
        public string? FromLed_Name { get; set; }
        public double FromPmt_Amt { get; set; }
        public int ToMem_Id { get; set; }
        public string? ToMem_No { get; set; }
        public string? ToMem_Name { get; set; }
        public int ToLed_Id { get; set; }
        public string? ToLed_Name { get; set; }
        public double ToRpt_Amt { get; set; }
        public int Voc_Id { get; set; }
        public string? Voc_Rpt_No { get; set; }
        public string? Voc_Pmt_No { get; set; }
        public DateTime Transfer_Date { get; set; }
    }
}
