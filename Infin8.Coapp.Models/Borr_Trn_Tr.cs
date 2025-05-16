namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Borr_Trn_Tr
    {
        [Key]
        public int Trn_Tr_Id { get; set; }
        public int Trn_Id { get; set; }
        public int Borr_Id { get; set; }
        public Nullable<System.DateTime> Due_Date { get; set; }
        public int Due_Month { get; set; }
        public int Due_Year { get; set; }
        public double Prl_Dem { get; set; }
        public double Int_Calc_Amt { get; set; }
        public Nullable<System.DateTime> Int_Calc_Date { get; set; }
        public int Int_Calc_Month { get; set; }
        public int Int_Calc_Year { get; set; }
        public double PI_Calc_Amt { get; set; }
        public Nullable<System.DateTime> PI_Calc_Date { get; set; }
        public int PI_Calc_Month { get; set; }
        public int PI_Calc_year { get; set; }
        public double Prl_Coll { get; set; }
        public double Int_Coll { get; set; }
        public double PI_Coll { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; }
    }
}
