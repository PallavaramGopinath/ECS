namespace Infin8.Coapp.Dto
{
    public  class rptMemberSBAcTrn
    {
        public decimal Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public decimal Acc_Id { get; set; }
        public string? Acc_No { get; set; }
        public double Rpt_Amt { get; set; }
        public double Pmt_Amt { get; set; }
        public double Total_Amt { get; set; }
        public double Amt_CB { get; set; }
        public double Amt_OB { get; set; }
        public double Int_OB { get; set; }
        public double IntCalc_Amt { get; set; }
        public double IntPaid_Amt { get; set; }
        public double Int_CB { get; set; }
    }
}
