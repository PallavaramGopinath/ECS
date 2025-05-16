
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Emp_Pf
    {
        [Key]
        public decimal Pf_Id { get; set; }
        public decimal Mem_Id { get; set; }
        public Nullable<System.DateTime> Pf_Date { get; set; }
        public double Pf_Subscription { get; set; }
        public double Pf_Withdrawn { get; set; }
        public double Pf_Balance { get; set; }
        public int No_Of_Days { get; set; }
        public double Pf_Product { get; set; }
        public double Vpf_Contribution { get; set; }
        public double Vpf_Withdrawn { get; set; }
        public double Vpf_Balance { get; set; }
        public int Vpf_No_Of_Days { get; set; }
        public double Vpf_Product { get; set; }
        public double Bpf_Contribution { get; set; }
        public double Bpf_Withdrawn { get; set; }
        public double Bpf_Balance { get; set; }
        public int Bpf_No_Of_Days { get; set; }
        public double Bpf_Product { get; set; }
        public double Pf_Int_Accrued { get; set; }
        public double Pf_Int_Withdrawn { get; set; }
        public double Pf_Int_Balance { get; set; }
        public Nullable<System.DateTime> Int_Calc_Upto { get; set; }
        public decimal Voc_Id { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public int SlNo { get; set; }
        public double Pf_Interest { get; set; }
        public double Vpf_Interest { get; set; }
        public double Epf_Interest { get; set; }
        public double Bpf_Interest { get; set; }
        public double Epf_Int_Withdrawn { get; set; }
        public double Epf_Int_Balance { get; set; }
        public double Bpf_Int_Withdrawn { get; set; }
        public double Bpf_Int_Balance { get; set; }
        public string? Status { get; set; }
        public bool Pf_Delete { get; set; }
        public bool Pf_Oe { get; set; }
        public Nullable<System.DateTime> Last_Int_ApplicationDate { get; set; }
        public int Pf_Type { get; set; }
        public string? BrCode { get; set; }
    }
}
