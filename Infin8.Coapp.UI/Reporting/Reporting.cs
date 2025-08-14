using Infin8.Coapp.BusinessLogic;
using Microsoft.Reporting.NETCore;
using Microsoft.ReportingServices.ReportProcessing.ReportObjectModel;
using System.Threading.Tasks;

namespace Infin8.Coapp.UI.Reporting
{
    public interface IReporting
    {
        public void CreateLocalReport(string path, ReportDataSource dataSource, IEnumerable<ReportParameter> parameters,string brCode);
    }

    public class Reporting : IReporting
    {
        private readonly IGeneralHandler _generalHandler;
        public Reporting( IGeneralHandler generalHandler )
        {
            _generalHandler = generalHandler;
        }
        
        public void CreateLocalReport(string path, ReportDataSource dataSource, IEnumerable<ReportParameter> parameters, string brCode)
        {
            byte[] pdfAsBytes = Array.Empty<byte>();
            try
            {
                LocalReport localReport = new LocalReport
                {
                    EnableExternalImages = true,
                };
                localReport.ReportPath = path;

                localReport.DataSources.Add(dataSource);
                localReport.SetParameters(parameters);

                localReport.DataSources.Clear();
                localReport.DataSources.Add(new ReportDataSource("Ds_FALedgerTrn", dataSource));
                localReport.SetParameters(parameters);
                localReport.Refresh();
                pdfAsBytes = localReport.Render("PDF");
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
