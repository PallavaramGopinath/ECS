using Microsoft.Reporting.NETCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Reporting
{
    public static class MicrosoftReport
    {
        public static byte[] CreateLocalReport(string dsName, Stream stream, IEnumerable result, Dictionary<string, string> parameterDictionary)
        {
            byte[] pdfAsBytes = Array.Empty<byte>();

            if (result.Equals(0))
            {
                return pdfAsBytes;
            }

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding.GetEncoding("windows-1252");

            LocalReport localReport = new LocalReport
            {
                EnableExternalImages = true
            };
            // using (FileStream stream = System.IO.File.OpenRead(path))
            {
                localReport.LoadReportDefinition(stream);
            }

            localReport.DataSources.Add(new ReportDataSource(dsName, result));

            List<ReportParameter> rParameters = new List<ReportParameter>();
            foreach (var param in parameterDictionary)
            {
                ReportParameter rParam = new ReportParameter(param.Key, param.Value);
                rParameters.Add(rParam);
            }

            localReport.SetParameters(rParameters);

            try
            {
                pdfAsBytes = localReport.Render("PDF");
                return pdfAsBytes;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return pdfAsBytes;
            }
        }
    }
}
