using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface ICreateReportsHandler
    {
        public byte[] CreateLocalReport(string dsName, Stream path, IEnumerable result, Dictionary<string, string> parameterDictionary);
    }
}
