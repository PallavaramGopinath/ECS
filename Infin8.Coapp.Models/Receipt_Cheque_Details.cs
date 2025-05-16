
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Receipt_Cheque_Details
    {
        [Key]
        public int Rpt_Cheque_Id { get; set; }
        public int Rpt_Cheque_ReceiptId { get; set; }
        public int Rpt_Cheque_LoanId { get; set; }
        public string? Rpt_Cheque_No { get; set; }
        public Nullable<System.DateTime> Rpt_Cheque_Date { get; set; }
        public string? Rpt_Cheque_Bank { get; set; }
        public Nullable<double> Rpt_Cheque_Amt { get; set; }
        public int Rpt_Cheque_AccCode { get; set; }
        public int Rpt_Cheque_YrId { get; set; }
        public string? Rpt_Cheque_VoucherNo { get; set; }
        public string? BrCode { get; set; }
    }
}
