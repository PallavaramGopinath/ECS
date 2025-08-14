using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Models
{
    public class Staging_History
    {
        [Key]
        public decimal Id { get; set; }
        public decimal Staging_Id { get; set; }
        public decimal Member_Id { get; set; }
        public decimal Ledger_Id { get; set; }
        public int Related_Account_Id { get; set; }
        public double Receipt_Amount { get; set; }
        public double Payment_Amount { get; set; }
        public string? Module_Name { get; set; }
        public int Cash_Or_Adjustment { get; set; }
        [Column(TypeName = "jsonb")]
        public string? Related_Account_Data { get; set; }
        public decimal Created_By { get; set; }
        public DateTime Created_Date { get; set; }
        public decimal Checked_By { get; set; }
        public DateTime? Checked_Date { get; set; }
        public string? Staging_Status { get; set; }
        public string? BrCode { get; set; }
        public string? Cheque_No { get; set; }
        public DateTime? Cheque_Date { get; set; }
        public string? Issue_Bank_Name { get; set; }
        public decimal? Voc_Id { get; set; }
    }
}
