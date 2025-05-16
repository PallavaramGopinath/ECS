using System.ComponentModel.DataAnnotations;
namespace Infin8.Coapp.Models
{
    using System;

    public partial class Mem_PassBook
    {
        [Key]
        public decimal PassBook_Id { get; set; }
        public decimal Mem_Id { get; set; }
        public int Statement_Id { get; set; }
        public int EndLineNo { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        
        public int TrnSlNo { get; set; }
        public string? BrCode { get; set; }
    }
}
