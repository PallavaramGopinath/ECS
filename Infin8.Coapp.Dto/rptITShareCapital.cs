namespace Infin8.Coapp.Dto
{
    public class rptITShareCapital
    {
        public int Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public string? TokenNo { get; set; }
        public string? MemberType { get; set; }
        public string? Address { get; set; }
        public string? PANNo { get; set; }
        public string? AadharNo { get; set; }
        public int Led_Id { get; set; }
        public string? Led_Name { get; set; }
        public double Amt_OB { get; set; }
        public double Rpt_Amt { get; set; }
        public double Pmt_Amt { get; set; }
        public double Amt_CB { get; set; }
        public DateTime Trn_Date { get; set; }
    }
}
