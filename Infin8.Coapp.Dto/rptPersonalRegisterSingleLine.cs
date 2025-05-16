namespace Infin8.Coapp.Dto
{
    public class rptPersonalRegisterSingleLine
    {
        public int Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? Per_No { get; set; }
        public string? TokenNo { get; set; }
        public string? MemberName { get; set; }
        public string? SectionName { get; set; }
        public string? OfficeName { get; set; }
        public string? Surety_memberNo { get; set; }
        public string? Surety_Per_No { get; set; }
        public string? Surety_memberName { get; set; }
        public string? Surety_TokenNo { get; set; }
        public DateTime Trn_Date { get; set; }
        public double Mem_SC { get; set; }
        public double Surety_SC { get; set; }
        public double SLDisbAmt { get; set; }
        public double SLPrlRec { get; set; }
        public double SLIntRec { get; set; }
        public double SLPIRec { get; set; }
        public double SLTotIntRec { get; set; }
        public double ELDisbAmt { get; set; }
        public double ELPrlRec { get; set; }
        public double ELIntRec { get; set; }
        public double ELPIRec { get; set; }
        public double ELTotIntRec { get; set; }
        public double TDPmt { get; set; }
        public double TDRec { get; set; }
        public double FWDPmt { get; set; }
        public double FWDRec { get; set; }
        public double FWDIntRec { get; set; }
        public double SRFRec { get; set; }
        public double DueByRec { get; set; }
        public double SLPrlOS { get; set; }
        public double ELPrlOS { get; set; }
        public double TDBal { get; set; }
        public double FWDBal { get; set; }
        public double SCBal { get; set; }
        public int Voc_Type { get; set; }
        public string? Voc_Rpt_No { get; set; }
    }
}
