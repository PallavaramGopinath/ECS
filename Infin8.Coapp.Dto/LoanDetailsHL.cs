namespace Infin8.Coapp.Dto
{
    public class LoanDetailsHL
    {
        public decimal Loan_Id { get; set; }
        public string? Loan_No { get; set; }
        public decimal Mem_Id { get; set; }
        public DateTime? FirstPrl_DueDate { get; set; }
        public DateTime FirstInt_DueDate { get; set; }
        public int Prl_Prd { get; set; }
        public double Disb_Amt { get; set; }
        public DateTime Trn_Date { get; set; }
        public DateTime Disb_Date { get; set; }
        public double Prl_Dem { get; set; }
        public double Prl_Sched { get; set; }
        public double PrlColl_Amt { get; set; }
        public DateTime? Due_Date { get; set; }
        public double IntCalc_Amt { get; set; }
        public DateTime? IntCalc_Date { get; set; }
        public double IntColl_Amt { get; set; }
        public double PICalc_Amt { get; set; }
        public DateTime? PICalc_Date { get; set; }
        public double PIColl_Amt { get; set; }
        public double IODCalc_Amt { get; set; }
        public DateTime? IODCalc_Date { get; set; }
        public double IODColl_Amt { get; set; }
        public double IntCalc_AmtCurrent { get; set; }
        public DateTime? IntCalc_DateCurrent { get; set; }
        public double PICalc_AmtCurrent { get; set; }
        public DateTime? PICalc_DateCurrent { get; set; }
        public double IODCalc_AmtCurrent { get; set; }
        public DateTime? IODCalc_DateCurrent { get; set; }
        public double Inst_Amt { get; set; }
        public double Prl_DemCurrent { get; set; }
        public int Inst_Type { get; set; }
        public int Dem_Frequency { get; set; }
        public int Int_Frequency { get; set; }
        public int Int_Application { get; set; }
        public int PI_Application { get; set; }
        public int IOD_Application { get; set; }
        public int Adv_Prl_Application { get; set; }
        public decimal PrlLed_Id { get; set; }
        public decimal IntLed_Id { get; set; }
        public decimal PILed_Id { get; set; }
        public decimal IODLed_Id { get; set; }
        public string? Scheme_Name { get; set; }
    }
}
