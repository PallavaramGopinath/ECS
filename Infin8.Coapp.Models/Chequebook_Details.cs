
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Chequebook_Details
    {
        [Key]
        public int Chequebook_DetailsId { get; set; }
        public short ChequeBook_Id { get; set; }
        public int Chequebook_leafno { get; set; }
        public byte chequebook_Status { get; set; }
        public string? Chequebook_Particulars { get; set; }
        public Nullable<System.DateTime> Chequebook_Useddate { get; set; }
        public double Chequebook_Amount { get; set; }
        public string? BrCode { get; set; }
        public string? Voc_Status { get; set; }
    }
}
