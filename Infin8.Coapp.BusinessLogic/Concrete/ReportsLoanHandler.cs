using Infin8.Coapp.Dto;
using Infin8.Coapp.Repository;
using Microsoft.ReportingServices.ReportProcessing.ReportObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class ReportsLoanHandler : IReportsLoanHandler
    {
        readonly IUnitOfWork _unitOfWork;
        readonly ICreateReportsHandler _createReportsHandler;
        public ReportsLoanHandler(IUnitOfWork unitOfWork, ICreateReportsHandler createReportsHandler)
        {
            _unitOfWork = unitOfWork;
            _createReportsHandler = createReportsHandler;
        }

        public async Task<List<rptLoanLedger>> GetLoanLedger(List<decimal> loanIdList, DateTime fromDate, DateTime toDate,string brCode)
        {
            return await _unitOfWork.ReportsLoan.GetLoanLedger(loanIdList, fromDate, toDate, brCode);
        }
        public async Task<byte[]> GetLoanOutstandingWithAgewise(string datasetName, DateTime toDate, int loanType,string brCode, FileStream reportStream, Dictionary<string, string> parameters)
        {
            var rptResult =  await _unitOfWork.ReportsLoan.GetLoanOutstandingWithAgewise(toDate, loanType,brCode );
            var rptResultEnumerable = rptResult as IEnumerable<rptLoanOutstanding> ?? rptResult.ToList();
            var pdfAsBytes = _createReportsHandler.CreateLocalReport(datasetName, reportStream, rptResultEnumerable, parameters);
            return pdfAsBytes;
        }

        public async Task<byte[]> GetLoanOutstanding(string datasetName, DateTime toDate, int loanType, string brCode, FileStream reportStream, Dictionary<string, string> parameters)
        {
            var rptResult = await _unitOfWork.ReportsLoan.GetLoanOutstanding(toDate, loanType, brCode);
            var rptResultEnumerable = rptResult as IEnumerable<rptLoanOutstanding> ?? rptResult.ToList();
            var pdfAsBytes = _createReportsHandler.CreateLocalReport(datasetName, reportStream, rptResultEnumerable, parameters);
            return pdfAsBytes;
        }
        public async Task<List<rptLoanDCB>> GetLoanDCB(DateTime fromDate, DateTime toDate,string brCode)
        {
            List<rptLoanDCB> dcbList = new List<rptLoanDCB>();
            double _dueToAmt = 0;
            try
            {
                dcbList = await _unitOfWork.ReportsLoan.GetLoanDCB(fromDate, toDate,brCode);
                foreach (var dcb in dcbList) 
                {
                    _dueToAmt = 0;
                    _dueToAmt = await _unitOfWork.MemTrn.GetmemTrnTotalSuspenseAmount(dcb.Mem_Id, 1,brCode ); 
                    dcb.Due_To = _dueToAmt;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return dcbList;
        }

        public async Task<List<rptLoanDisbursementHSIS>> GetLoanDisbursement(DateTime fromDate, DateTime toDate, int loanType,string brCode)
        {
            return await _unitOfWork.ReportsLoan.GetLoanDisbursement(fromDate, toDate, loanType,brCode );
        }
        
    }
}
