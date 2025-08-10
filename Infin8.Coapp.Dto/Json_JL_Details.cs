using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class Json_JL_Details
    {
        public DateTime? Due_Date { get; set; }
        public double Market_Rate { get; set; }
        public double Rate_Per_Gram { get; set; }
        public double Gross_Weight { get; set; }
        public double Wastage { get; set; }
        public double Net_Weight { get; set; }
        public double Net_Value { get; set; }
        public string? Jewels_Image_Path { get; set; }

    }
}
