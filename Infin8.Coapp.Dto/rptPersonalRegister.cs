namespace Infin8.Coapp.Dto
{
    public class rptPersonalRegister
    {
        public int Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? Per_No { get; set; }
        public string? MemberName { get; set; }
        public string? TokenNo { get; set; }
        public string? SectionName { get; set; }
        public string? ReferName { get; set; }
        public string? Surety_memberNo { get; set; }
        public string? Surety_Per_No { get; set; }
        public string? Surety_memberName { get; set; }
        public double Mem_SC { get; set; }
        public double Surety_SC { get; set; }
        public int Loan_Id { get; set; }
        public int DM_Id { get; set; }
        public int TD_Id { get; set; }
        public int Led_Id { get; set; }
        public DateTime Trn_Date { get; set; }
        public string? PB_Status { get; set; }
        public string? Status { get; set; }  /// F=factory recovery
        //public string? PB_No { get; set; }
        public string? Acc_No { get; set; } /// Acc_No for loan No, rd No, fd No
        //public string? PB_AccountName { get; set; }
        public string? Acc_Name { get; set; }    /// Loan scheme Name, deposit scheme name, rd name, gl name
        //public string? PBTrnNo { get; set; }
        public string? Voc_No { get; set; }
        public double Prl_OB { get; set; }
        public double Prl_Rpt { get; set; }
        public double Prl_Pmt { get; set; }
        public double Prl_CB { get; set; }
        public double Int_OB { get; set; }
        public double IntCalc_Amt { get; set; }
        public double IntColl_Amt { get; set; }
        public double Int_Rpt { get; set; }
        public double Int_Pmt { get; set; }
        public double Int_CB { get; set; }
        public double PI_OB { get; set; }
        public double PICalc_Amt { get; set; }
        public double PI_Rpt { get; set; }
        public double PI_CB { get; set; }
        public double Prl_Dem { get; set; }
        public double Prl_OD { get; set; }
        public string? MonthYear { get; set; }
        public string? MonthYearString { get; set; }
        public double FR { get; set; }
        public int Voc_Type { get; set; }

    }
}
