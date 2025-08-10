using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Models
{
    public class Staging_Master
    {
        [Key]
        public decimal Staging_Id { get; set; }
        public string? Session_Id { get; set; }
        public decimal Member_Id { get; set; }
        public decimal Created_By { get; set; }
        public DateTime Created_Date { get; set; }
        public decimal Checked_By { get; set; }
        public DateTime? Checked_Date { get; set; }
        public string? Staging_Status { get; set; }
        public string? BrCode { get; set; }
        public string? Type { get; set; }
        public decimal? Voc_Id { get; set; }
    }
}
