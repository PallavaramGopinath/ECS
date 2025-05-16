namespace Infin8.Coapp.Dto
{
    public class rptDemandStop
    {
        public int Mem_Id { get; set; }
        public string? Voc_Rpt_No { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo  { get; set; }
        public string? MemberName { get; set; }
        public string? TicketTokenGangNo { get; set; }
        public DateTime Voc_Date { get; set; }
        public double PrlColl_Amt { get; set; }
        public double IntColl_Amt { get; set; }
        public double PIColl_Amt { get; set; }
        public double Total_Amt { get; set; }
    }
}
