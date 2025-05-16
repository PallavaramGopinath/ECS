
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Pay_Association
    {
        [Key]
        public int Ass_Id { get; set; }
        public Nullable<System.DateTime> Ass_Date { get; set; }
        public Nullable<System.DateTime> Ass_WEF { get; set; }
        public int Emp_Id { get; set; }
        public int Ass_Type_Id { get; set; }
        public int Ass_Desgn_Id { get; set; }
        public int Ass_Cat_Id { get; set; }
        public int Ass_Grade_Id { get; set; }
        public double Ass_Basic { get; set; }
        public double Ass_VPF { get; set; }
        public string? Ass_Scale { get; set; }
        public bool Ass_Curr { get; set; }
        public bool Ass_Delete { get; set; }
        public int Usr_Id { get; set; }
        public int Yr_Id { get; set; }
        public string? BrCode { get; set; }
    }
}
