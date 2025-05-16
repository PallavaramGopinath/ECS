
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Depositor_Option
    {
        [Key]
        public int DO_Id { get; set; }
        public int DM_Id { get; set; }
        public int Mem_Id { get; set; }
        public Nullable<System.DateTime> DO_Wef { get; set; }
        public double OptionAmount { get; set; }
        public bool IsDO_Closed { get; set; }
        public Nullable<System.DateTime> DO_ClosedDate { get; set; }
        public bool DO_Delete { get; set; }
        public int Usr_Id { get; set; }
        public int Yr_Id { get; set; }
        public int Voc_Id { get; set; }
        public string? BrCode { get; set; }
    }
}
