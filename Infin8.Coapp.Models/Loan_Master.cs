using System.ComponentModel.DataAnnotations;
namespace Infin8.Coapp.Models
{
    using System;

    public partial class Loan_Master
    {
        [Key]
        public decimal Loan_Id { get; set; }
        public int Scheme_Id { get; set; }
        public string? Loan_No { get; set; }
        public decimal Mem_Id { get; set; }
        public int App_Id { get; set; }
        public string? App_No { get; set; }
        public Nullable<System.DateTime> Led_Date { get; set; }
        public string? Res_No { get; set; }
        public Nullable<System.DateTime> Res_Date { get; set; }
        public double San_Amt { get; set; }
        public DateTime San_Date { get; set; }
        public int Loan_Type { get; set; }
        public int Loan_Status { get; set; }
        public int Pur_Id { get; set; }
        public int SubGrp_Id { get; set; }
        public int Grp_Id { get; set; }
        public int Ags_Id { get; set; }
        public int Diff_Prd { get; set; }
        public int DiffIntRec_Prd { get; set; }
        public int Grace_Prd { get; set; }
        public int Prl_Prd { get; set; }
        public int Int_Prd { get; set; }
        public Nullable<System.DateTime> FirstInt_DueDate { get; set; }
        public Nullable<System.DateTime> FirstPrl_DueDate { get; set; }
        public Nullable<System.DateTime> FirstDiff_DueDate { get; set; }
        public Nullable<System.DateTime> Last_DueDate { get; set; }
        public Nullable<System.DateTime> LastDemand_Date { get; set; }
        public int Inst_Date { get; set; }
        public string? Reim_LNo { get; set; }
        public string? Reim_DvNo { get; set; }
        public string? Mort_SlNo { get; set; }
        public Nullable<System.DateTime> Mort_ExeDt { get; set; }
        public Nullable<System.DateTime> Mort_RegDate { get; set; }
        public decimal Mort_Sro { get; set; }
        public double Roi { get; set; }
        public double Pi { get; set; }
        public double Inst_Amt { get; set; }
        public Nullable<System.DateTime> LastIntApplication_Date { get; set; }
        public Nullable<System.DateTime> NextIntApplication_Date { get; set; }
        public Nullable<System.DateTime> LastPIApplication_Date { get; set; }
        public Nullable<System.DateTime> LastIODApplication_Date { get; set; }
        public bool Loan_Oe { get; set; }
        public bool Loan_Delete { get; set; }
        public bool IsAccountClosed { get; set; }
        public Nullable<System.DateTime> AccountClosedDate { get; set; }
        public decimal Voc_Id { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public double SecurityFaceValue { get; set; }
        public double DrawingPower { get; set; }
        public int SecurityType { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; }
    }
}
