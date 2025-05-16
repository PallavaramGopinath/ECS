using System.ComponentModel.DataAnnotations;
namespace Infin8.Coapp.Models
{
    using System;

    public partial class Mem_Fees_Tr
    {
        [Key]
        public int Fee_Tr_Id { get; set; }
        public Nullable<int> Fee_Id { get; set; }
        public Nullable<double> Rpt_Amt { get; set; }
        public Nullable<int> Led_Id { get; set; }
        public Nullable<int> Voc_Id { get; set; }
        public Nullable<int> Usr_Id { get; set; }
        public Nullable<int> Yr_Id { get; set; }
    }
}
