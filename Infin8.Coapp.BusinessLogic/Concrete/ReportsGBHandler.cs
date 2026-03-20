using Infin8.Coapp.BusinessLogic.Interface;
using Infin8.Coapp.Dto;
using Infin8.Coapp.Repository;
using Microsoft.ReportingServices.ReportProcessing.ReportObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic.Concrete
{
    public class ReportsGBHandler : IReportsGBHandler
    {
        readonly IUnitOfWork _unitOfWork;
        readonly ICreateReportsHandler _createReportsHandler;
        public ReportsGBHandler(IUnitOfWork unitOfWork, ICreateReportsHandler createReportsHandler)
        {
            _unitOfWork = unitOfWork;
            _createReportsHandler = createReportsHandler;
        }


        public async Task<byte[]> GetDividendWorkingSheet(string datasetName, decimal pbleMasterId, string brCode, FileStream reportStream, Dictionary<string, string> parameters)
        {
            var rptResult = await _unitOfWork.ReportsGB.GetDividendWorkingSheet(pbleMasterId, brCode);
            var rptResultEnumerable = rptResult as IEnumerable<rptDividendTDFWDWorking> ?? rptResult.ToList();
            var pdfAsBytes = _createReportsHandler.CreateLocalReport(datasetName, reportStream, rptResultEnumerable, parameters);
            return pdfAsBytes;
        }
        public async Task<byte[]> GetDividendPendingList(string datasetName, DateTime asOnDate, string brCode, FileStream reportStream, Dictionary<string, string> parameters)
        {
            var rptResult = await _unitOfWork.ReportsGB.GetDividendPendingList(asOnDate, brCode);
            var rptResultEnumerable = rptResult as IEnumerable<rptDividendIntOnTDPendingList> ?? rptResult.ToList();
            var pdfAsBytes = _createReportsHandler.CreateLocalReport(datasetName, reportStream, rptResultEnumerable, parameters);
            return pdfAsBytes;
        }

        public async Task<byte[]> GetDividendPaidList(string datasetName, DateTime fromDate, DateTime toDate, string brCode, FileStream reportStream, Dictionary<string, string> parameters)
        {
            var rptResult = await _unitOfWork.ReportsGB.GetDividendPaidList(fromDate, toDate, brCode);
            var rptResultEnumerable = rptResult as IEnumerable<rptDividendPaid> ?? rptResult.ToList();
            var pdfAsBytes = _createReportsHandler.CreateLocalReport(datasetName, reportStream, rptResultEnumerable, parameters);
            return pdfAsBytes;
        }
    }
}
