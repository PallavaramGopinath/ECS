
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Mem_Payable_Master
    {
        [Key]
        public decimal PbleMaster_Id { get; set; }
        public int PbleType { get; set; }
        public decimal Led_Id { get; set; }
        public Nullable<System.DateTime> Calculate_Date { get; set; }
        public Nullable<System.DateTime> Transfered_Date { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public double ROI_Pble { get; set; }
        public double ROI_Trnble { get; set; }
        public decimal Calc_YrId { get; set; }
        public bool Master_Delete { get; set; }
        public string? Master_Status { get; set; }
        public decimal Voc_Id { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
    }
}
