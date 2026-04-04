using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public  class DtoReferArea
    {
        public decimal Area_Id { get; set; }
        public int Taluk_Id  { get; set; }
        public int District_Id { get; set; }
        public string? Area_Name { get; set; }
        public string? Taluk_Name { get; set; }
        public string? District_Name { get; set; }
        public string? Area_Notes { get; set; }
        public string? BrCode { get; set; }
        public bool Area_Delete { get; set; }
    }
}
