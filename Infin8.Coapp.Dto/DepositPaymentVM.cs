namespace Infin8.Coapp.Dto
{
    public class DepositPaymentVM
    {
        public int Led_Id { get; set; }
        public int IntLed_Id { get; set; }
        public string? Led_Name { get; set; }
        public double Rpt { get; set; }
        public double Pmt { get; set; }
        public string? MemberNo { get; set; }
        public string? MemberName { get; set; }
        public int Mem_Id { get; set; }
        public int DM_Id { get; set; }
        public double IntBal { get; set; }
        public double IntCalc { get; set; }
        public DateTime? IntCalcDate { get; set; }
        public int Acc_Id { get; set; }
        public int Trn_Type { get; set; }
        public int Status { get; set; }
        public int PbleMaster_Id { get; set; }
        public string? PbleYear { get; set; }

    }
}
