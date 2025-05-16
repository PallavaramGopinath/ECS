namespace Infin8.Coapp.Dto
{
    public class DemandStopVM
    {
        public int StopDemand_Id { get; set; }
        public int Voc_Id { get; set; }
        public string? Voc_Rpt_No { get; set; }
        public int Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public DateTime DemandDate { get; set; }
        public string? Voc_Date { get; set; }
        public double PrlColl_Amt { get; set; }


    }
}
