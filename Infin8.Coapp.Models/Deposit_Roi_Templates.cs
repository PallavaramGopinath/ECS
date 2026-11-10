namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Deposit_Roi_Templates
    {
        [Key]
        public decimal Roi_Id { get; set; }
        public decimal Deposit_Id { get; set; }
        public DateOnly? Wef { get; set; }
        public double Interest_Rate { get; set; }
        public double Penal_Interest_Rate { get; set; }
        public bool is_Active { get; set; }
        public string? BrCode { get; set; }
        public int Usr_Id { get; set; }
        public int Yr_Id { get; set; }
        
    }
}
