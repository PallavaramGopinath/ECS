namespace Infin8.Coapp.Dto
{
    public class rptMemberRegister
    {
        public decimal Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public string? Token_PersonNo { get; set; }
        public string? FatherName { get; set; }
        public DateTime? Dob { get; set; }
        public int Age { get; set; }
        public string? PreAdd1 { get; set; }
        public string? PreAdd2 { get; set; }
        public string? PreAdd3 { get; set; }
        public string? Area_Name { get; set; }
        public DateTime? Trn_Date { get; set; }
        public double Rpt_Amt { get; set; }
        public double Pmt_Amt { get; set; }
        public double Bal_Amt { get; set; }
        public double IntCalc_Amt { get; set; }
        public DateTime? IntCalc_Date { get; set; }
        public double IntPaid_Amt { get; set; }
        public double Int_Bal { get; set; }
        public string? MobileNo { get; set; }
        public string? PANNo { get; set; }
        public string? AadharNo { get; set; }
        public string? SmartCardNo { get; set; }
        public string? MemberPhoto { get; set; }
    }
}
