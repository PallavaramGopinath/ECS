namespace Infin8.Coapp.Dto
{
    public class RecoveryAppropriationListVM
    {
        public int MemDemand_Id { get; set; }
        public int Mem_Id { get; set; }
        public int Loan_Id { get; set; }
        public double    PIArrearCollection { get; set; }
        public double PICurrentCollection { get; set; }
        public double PITotalCollection { get; set; }

        public double IntArrearCollection { get; set; }
        public double IntCurrentCollection { get; set; }
        public double IntTotalCollection { get; set; }

        public double PrlArrearCollection { get; set; }
        public double PrlCurrentCollection { get; set; }
        public double PrlTotalCollection { get; set; }

        public int DM_Id { get; set; }
        public double ArrearPIOnDepositCollection { get; set; }
        public double CurrentPIOnDepositCollection { get; set; }
        public double TotalPIOnDepositCollection { get; set; }
        public double ArrearDepositCollection { get; set; }
        public double CurrentDepositCollection { get; set; }
        public double TotalDepositCollection { get; set; }

        public int SuspenseLed_Id { get; set; }
        public double ArrearSuspenseCollection { get; set; }
        public double CurrentSuspenseCollection { get; set; }
        public double TotalSuspenseCollection { get; set; }

        public int TD_Id { get; set; }
        public int PIOnRDReceiptAmount { get; set; }
        public int RDReceiptAmount { get; set; }
        public int RDDemandAmount2 { get; set; }
        public int RDNoOfInstalments2 { get; set; }
        public int RDTotalReceiptAmount { get; set; }

        public int SuspenseDueByLed_Id { get; set; }
        public double TotalDueByCollection { get; set; }

        public int DueByDemandLed_Id { get; set; }
        public int ArrearDueByCollectionOnDemand { get; set; }
        public int CurrentDueByCollectionOnDemand { get; set; }
        public int TotalDueByCollectionOnDemand { get; set; }
        public DateTime RecoveryDate { get; set; }
    }
}
