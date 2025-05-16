namespace Infin8.Coapp.Dto
{
    public  class rptDividendPaid
    {
        public int Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? MemberName { get; set; }
        public int PbleMaster_Id { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public double DividendPaid { get; set; }
    }
}
