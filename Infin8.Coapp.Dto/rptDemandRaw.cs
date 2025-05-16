namespace Infin8.Coapp.Dto
{
    public class rptDemandRaw
    {
        public int MemDemand_Id { get; set; }
        public int Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? GPF_No { get; set; }
        public string? TicketTokenGangNo { get; set; }
        public string? MemberName { get; set; }
        public string? SectionCode { get; set; }
        public int OfficeId { get; set; }
        public string? ReferName { get; set; }
        public string? DemandType { get; set; }
        public int? Scheme_Id { get; set; }
        public string? Scheme_Name { get; set; }
        public int? DM_Id { get; set; } 
        public string? DMName { get; set; }
        public int? TD_Id { get; set; }
        public string? TD_No { get; set; }
        public int? SuspenseLed_Id { get; set; }
        public string? DueToLedName { get; set; }
        public int? DueByDemandLed_Id { get; set; }
        public string? DueByLedName { get; set; }
        public int? SuspenseDueByLed_Id { get; set; }
        public string? DueByLedNameRec { get; set; }
        public int? Loan_Id { get; set; }
        public string? Loan_No { get; set; }
        public double? San_Amt { get; set; }
        public DateTime? San_Date { get; set; }
        public double? Loan_OS { get; set; }
        public int? Prl_Prd { get; set; }
        public double? Roi { get; set; }
        public double? PrlOD_Amt { get; set; }
        public double PIArrearDemand { get; set; }
        public double IntArrearDemand { get; set; }
        public double PrlArrearDemand { get; set; }
        public double PICurrentDemand { get; set; }
        public double IntCurrentDemand { get; set; }
        public double PrlCurrentDemand { get; set; }
        public double PITotalDemand { get; set; }
        public double IntTotalDemand { get; set; }
        public double PrlTotalDemand { get; set; }
        public double TotalPIOnDeposit { get; set; }
        public double TotalDepositAmount { get; set; }
        public double PIOnRDTotalAmount { get; set; }
        public double RDDemandAmount { get; set; }
        public double TotalSuspenseAmount { get; set; }
        public double RDTotalDemandAmount { get; set; }
        public double TotalDueByDemandAmount { get; set; }
        public double TotalDemandAmount { get; set; }
        public double PITotalCollection { get; set; }
        public double IntTotalCollection { get; set; }
        public double PrlTotalCollection { get; set; }
        public double TotalPIOnDepositCollection { get; set; }
        public double TotalDepositCollection { get; set; }
        public double PIOnRDReceiptAmount { get; set; }
        public double RDReceiptAmount { get; set; }
        public double RDTotalReceiptAmount { get; set; }
        public double TotalSuspenseCollection { get; set; }
        public double TotalDueByCollection { get; set; }
        public double TotalDueByCollectionOnDemand { get; set; }
        public double TotalCollectionAmount { get; set; }
    }
}
