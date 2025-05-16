namespace Infin8.Coapp.Dto
{
    public class MemberTransactionList
    {
        public string? Voc_No { get; set; }
        public string? Voc_Rpt_No { get; set; }
        public int? Voc_Type { get; set; }
        public byte Trn_Type { get; set; }
        public int Led_Id { get; set; }
        public string? Led_Name { get; set; }
        public int Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? Per_No { get; set; }
        public string? MemberName { get; set; }
        public DateTime Trn_Date { get; set; }
        public double Pmt_Amt { get; set; }
        public double Rpt_Amt { get; set; }
        public double Amt_OB { get; set; }
        public double Amt_CB { get; set; }

        /// the below properties relates to SB Account
        public int Int_OB { get; set; }
        public int Int_CB { get; set; }
        public int IntCalc_Amt { get; set; }
        public DateTime? IntCalc_Date { get; set; }
        public int IntPaid_Amt { get; set; }
        public int IntNowCalcAmt { get; set; }
        public DateTime? IntNowCalcDate { get; set; }
        public int IntBalance { get; set; }
        public int? AccId { get; set; }
        public int Trn_SlNo { get; set; }
    }
}
