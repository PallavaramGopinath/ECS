namespace Infin8.Coapp.Dto
{
    public class MemberTransactionOE
    {
        public int Mem_Trn_Id { get; set; }
        public int Trn_Type { get; set; }
        public int Mem_Id { get; set; }
        public int Led_Id { get; set; }
        public DateTime Trn_Date { get; set; }
        public string? Led_Name { get; set; }
        public double Rpt_Amt { get; set; }
        public double Pmt_Amt { get; set; }
        public DateTime? IntCalc_Date { get; set; }
        public int IntCalc_Amt { get; set; }
        public int IntPaid_Amt { get; set; }
        public int Voc_Id { get; set; }
        public int Acc_Id { get; set; }
        public string? Acc_No { get; set; }
    }
}
