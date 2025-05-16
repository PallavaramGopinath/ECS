
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Round_Template
    {
        [Key]
        public int Round_Id { get; set; }
        public Nullable<System.DateTime> Wef { get; set; }
        public int Module_Type { get; set; }
        public int Paise { get; set; }
        public string? BrCode { get; set; }
    }
}
