using Infin8.Coapp.Dto;
using Infin8.Coapp.Repository;

namespace Infin8.Coapp.BusinessLogic
{
    /// <summary>Income tax reports, ported from TSIS frmReportViewer.Print_IT* (headers as in TSIS).</summary>
    public class ReportsITHandler : IReportsITHandler
    {
        public const int ITLoans = 109;
        public const int ITShareCapital = 110;
        public const int ITMembers = 111;
        public const int ITFixedDeposits = 112;

        readonly IUnitOfWork _unitOfWork;

        public ReportsITHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ITReportData> BuildAsync(rptReportITObject req)
        {
            string brCode = req.BrCode ?? "";
            string from = req.FromDate.ToString("dd-MM-yyyy"), to = req.ToDate.ToString("dd-MM-yyyy");
            var data = new ITReportData();
            switch (req.ReportId)
            {
                case ITLoans:
                    data.DataSetName = "Ds_ITLoanTSIS";
                    data.Rows = await _unitOfWork.ReportsIT.GetITLoansAsync(req.LoanType, req.FromDate, req.ToDate, brCode);
                    data.Parameters["paramReportHeader"] = $"Loan Given to Member for the Period From {from} To {to}";
                    break;
                case ITShareCapital:
                    data.DataSetName = "Ds_ITShareCapitalTSIS";
                    data.Rows = await _unitOfWork.ReportsIT.GetITShareCapitalAsync(req.FromDate, req.ToDate, brCode);
                    data.Parameters["paramReportHeader"] = $"Members Share Capital for the Period From {from} To {to}";
                    break;
                case ITMembers:
                    data.DataSetName = "Ds_ITMembersTSIS";
                    data.Rows = await _unitOfWork.ReportsIT.GetITMembersAsync(req.AsOnDate, brCode);
                    data.Parameters["paramReportHeader"] = $"Members Details As On {req.AsOnDate:dd-MM-yyyy}";
                    break;
                case ITFixedDeposits:
                    data.DataSetName = "Ds_ITFixedDepositHSIS";
                    data.Rows = await _unitOfWork.ReportsIT.GetITFixedDepositsAsync(req.FromDate, req.ToDate, brCode);
                    data.Parameters["paramReportHeader"] = $"Fixed Deposit Reports for Income Tax from {from} to {to}";
                    data.Parameters["paramFromDate"] = from;
                    data.Parameters["paramToDate"] = to;
                    break;
                default:
                    data.Message = $"Report {req.ReportId} is not an income tax report.";
                    break;
            }
            return data;
        }
    }
}
