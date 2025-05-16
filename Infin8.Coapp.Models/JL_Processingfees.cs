using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Models
{
    public class JL_Processingfees
    {
        public int Id { get; set; }
        public int Version_Id { get; set; }
        public double From_Value { get; set; }
        public double To_Value { get; set; }
        public double Appraisal_Fee { get; set; }
        public double Bank_Charges { get; set; }
        public double Service_Charges { get; set; }
        public bool JL_ProcessingFee_Delete { get; set; }

    }
}
