
namespace TsisEntities.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Borr_Details
    {
        [Key]
        public int Borr_Id { get; set; }
        public string? Borr_No { get; set; }
        public string? DV_No { get; set; }
        public Nullable<System.DateTime> Entry_Date { get; set; }
        public int Loan_Id { get; set; }
        public int Mem_Id { get; set; }
        public int Grace_Prd { get; set; }
        public int Prl_Prd { get; set; }
        public int Inst_type { get; set; }
        public int Dem_Frequency { get; set; }
        public int Int_Calc_Method { get; set; }
        public Nullable<System.DateTime> FirstInt_DueDate { get; set; }
        public Nullable<System.DateTime> FirstPrl_DueDate { get; set; }
        public int FirstInt_DueMonth { get; set; }
        public int FirstInt_DueYear { get; set; }
        public int FirstPrl_DueMonth { get; set; }
        public int FirstPrl_DueYear { get; set; }
        public bool Oe { get; set; }
        public int Voc_Id { get; set; }
        public int Pur_Id { get; set; }
        public int Usr_Id { get; set; }
        public int Yr_Id { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; }
    }
}
