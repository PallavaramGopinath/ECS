namespace Infin8.Coapp.Dto
{
    public class rptDemand
    {
        public int MemDemand_Id { get; set; }
        public int Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? PerNo { get; set; }
        public string? TicketTokenGangNo { get; set; }
        public string? MemberName { get; set; }
        public int OfficeId { get; set; }
        public string? ReferName { get; set; }
        public string? SectionCode { get; set; }
        public string? DemandType { get; set; }
        public int? Scheme_Id { get; set; }
        public string? Scheme_Name { get; set; }
        public int? Acc_Id { get; set; } /// Loan_Id, DM_Id, TDScheme_Id, Led_Id(for share capital, due to, due by rec, due by demand)
        public string? Acc_No { get; set; }
        public double Loan_OS { get; set; }
        public double Roi { get; set; }
        public double PrlOD_Amt { get; set; }
        public double Arrear_PI { get; set; }
        public double Current_PI { get; set; }
        public double Total_PI { get; set; }
        public double Arrear_Int { get; set; }
        public double Current_Int { get; set; }
        public double Total_Int { get; set; }
        public double Arrear_Prl { get; set; }
        public double Current_Prl { get; set; }
        public double Total_Prl { get; set; }
        public double Total_PIFWD { get; set; }
        public double Total_Deposit { get; set; }
        public double Total_PIRD { get; set; }
        public double Total_RD { get; set; }
        public double Total_DueTo { get; set; }
        public double Total_Dueby { get; set; }
        public double Grand_Total { get; set; }

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
        public double RestrictedDemand { get; set; }

    }
}
