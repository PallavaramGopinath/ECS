namespace Infin8.Coapp.Dto
{
    public  class rptDemandPSIS
    {
        public int Scheme_Id { get; set; }
        public string? Scheme_Name { get; set; }
        public int Loan_Id { get; set; }
        public string? Loan_No { get; set; }
        public DateTime Due_Date { get; set; }
        public double Prl_Sched { get; set; }
        public double Prl_Dem { get; set; }
        public double IntCalc_Amt { get; set; }
        public DateTime ? IntCalc_Date { get; set; }
        public double PICalc_Amt { get; set; }
        public DateTime ? PICalc_Date { get; set; }
        public double IODCalc_Amt { get; set; }
        public int Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? MemberName { get; set; }
    }
}
