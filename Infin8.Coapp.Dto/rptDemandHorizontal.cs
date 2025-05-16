namespace Infin8.Coapp.Dto
{
    public class rptDemandHorizontal
    {
        public int Mem_Id { get; set; }
        public int Demand_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? TicketTokenGangNo { get; set; }
        public string? MemberName { get; set; }
        public string? GPF_No { get; set; }
        public int OfficeId { get; set; }
        public string? ReferName { get; set; }
        public string? SectionCode { get; set; }
        public int Scheme_Id { get; set; }
        public string? SL_LoanNo { get; set; }
        public DateTime SL_San_date { get; set; }
        public double SL_San_amt { get; set; }
        public int SL_Prl_Prd { get; set; }
        public double SL_roi { get; set; }
        public double SL_Loan_OS { get; set; }
        public double SL_PrlOD_Amt { get; set; }
        public double SL_PrlTotalDemand { get; set; }
        public double SL_IntTotalDemand { get; set; }
        public double SL_PITotalDemand { get; set; }
        public string? EL_LoanNo { get; set; }
        public DateTime EL_San_date { get; set; }
        public double EL_San_amt { get; set; }
        public int EL_Prl_Prd { get; set; }
        public double EL_roi { get; set; }
        public double EL_Loan_OS { get; set; }
        public double EL_PrlOD_Amt { get; set; }
        public double EL_PrlTotalDemand { get; set; }
        public double EL_IntTotalDemand { get; set; }
        public double EL_PITotalDemand { get; set; }
        public int DM_Id { get; set; }
        public double TD_Total_Deposit { get; set; }
        public double FWD_Total_Deposit { get; set; }
        public double FWD_Total_PI { get; set; }
        public double SRF_Total_Deposit { get; set; }
        public double RDDemandAmount { get; set; }
        public double PIOnRDTotalAmount { get; set; }
        public double DueToDemand { get; set; }
        public int DueByDemand_Id { get; set; }
        public double ShareCapitalDemand { get; set; }
        public double GroupInsuranceDemand { get; set; }
        public double TotalDemand { get; set; }
        public double RestrictedDemand { get; set; }

        public double SL_PrlTotalCollection { get; set; }
        public double SL_IntTotalCollection { get; set; }
        public double SL_PITotalCollection { get; set; }
        public double EL_PrlTotalCollection { get; set; }
        public double EL_IntTotalCollection { get; set; }
        public double EL_PITotalCollection { get; set; }
        public double TD_TotalDepositCollection { get; set; }
        public double FWD_TotalDepositCollection { get; set; }
        public double FWD_TotalPIOnDepositCollection { get; set; }
        public double SRF_TotalDepositCollection { get; set; }
        public double RDReceiptAmount { get; set; }
        public double PIOnRDReceiptAmount { get; set; }
        public double DueToColl { get; set; }
        public double DueByColl { get; set; }
        public double ShareCapitalColl { get; set; }
        public double GroupInsuranceColl { get; set; }
        public double TotalColl { get; set; }
        public double NonReceiptAmt { get; set; }

    }
}
