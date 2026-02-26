using Infin8.Coapp.Reporting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class CreateReportsHandler : ICreateReportsHandler
    {
        //public byte[] CreateLocalReport(string dsName, Stream path, IEnumerable result, Dictionary<string, string> parameterDictionary)
        //{
        //     //return MicrosoftReport.CreateLocalReport(dsName, path, result, parameterDictionary);
        //}
        public byte[] CreateLocalReport(string dsName, Stream path, IEnumerable result, Dictionary<string, string> parameterDictionary)
        {
            //throw new NotImplementedException();
            return MicrosoftReport.CreateLocalReport(dsName, path, result, parameterDictionary);
        }
    }
}
