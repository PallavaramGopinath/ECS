using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public  class DtoCheckerDashboard
    {
        public decimal Staging_Id { get; set; }
        public decimal Member_Id { get; set; }
        public string? Member_No { get; set; }
        public string? PerNo { get; set; }
        public string? Member_Name { get; set; }
        public string? Type { get; set; }
        public double Receipt_Amount { get; set; }
        public double Payment_Amount { get; set; }
        public decimal Created_By { get; set; }
        public DateTime Created_Date { get; set; }
        public string? Created_By_Name { get; set; }
        public string? BrCode { get; set; }
    }
}
