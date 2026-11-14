namespace Infin8.Coapp.Dto
{
    /// <summary>A member (or surety) as printed in the personal register header (report 84).</summary>
    public class PersonalRegisterMember
    {
        public decimal Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? MemberName { get; set; }
        public string? TokenNo { get; set; }
        public string? OfficeName { get; set; }
        public decimal SuretyMem_Id { get; set; }
    }

    /// <summary>
    /// One raw movement for the personal register, from any module, normalised to receipt/payment and
    /// interest/penal columns. The handler turns these into running-balance register lines.
    /// </summary>
    public class PersonalRegisterMovement
    {
        /// <summary>SC share capital, DUEBY, DUETO, LOAN, DEP (thrift-type deposits), RD.</summary>
        public string Module { get; set; } = "";
        public decimal Mem_Id { get; set; }
        /// <summary>Led_Id, Loan_Id, Deposit_Id or TD_Id, depending on the module.</summary>
        public decimal AccountKey { get; set; }
        public string? Acc_No { get; set; }
        public string? Acc_Name { get; set; }
        public DateTime Trn_Date { get; set; }
        public int Trn_SlNo { get; set; }
        public string? Voc_No { get; set; }
        public string? Voc_Rpt_No { get; set; }
        public int Voc_Type { get; set; }
        /// <summary>Money received from the member (share capital paid in, loan principal repaid, deposit made).</summary>
        public double Rpt { get; set; }
        /// <summary>Money paid to the member (refund, loan disbursed, deposit withdrawn).</summary>
        public double Pmt { get; set; }
        public double IntCalc { get; set; }
        public double IntPaid { get; set; }
        public double PICalc { get; set; }
        public double PIPaid { get; set; }
    }
}
