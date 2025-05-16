namespace Infin8.Coapp.Dto
{
    public class DepositIntCalcDatesVM
    {
        public int PbleMaster_Id { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime? Transfered_Date { get; set; }

        public double ROI_Pble { get; set; }
        public double ROI_Trnble { get; set; }

        public int Voc_Id { get; set; }

    }
}
