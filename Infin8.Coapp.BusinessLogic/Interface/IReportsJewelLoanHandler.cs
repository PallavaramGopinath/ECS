using Infin8.Coapp.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IReportsJewelLoanHandler
    {
        //Task<List<rptJewelLoanVerificationList>> GetJewelLoanVerificationList(DateTime asOnDate);
        //Task<List<rptJewelLoanStockRegisterList>> GetJewelLoanStockRegisteList(DateTime fromDate, DateTime toDate, int loanType);
        //Task<List<rptJewelLoanOutstandingList>> GetJewelLoanOutstandingList(DateTime asOnDate);
        //Task<List<rptJewelLoanOverdueList>> GetJewelLoanOverdueList(DateTime asOnDate);
        //Task<List<rptJewelLoanOverdueList>> GetJewelLoanAboveLimitList(DateTime asOnDate, double limitAmt);
        //Task<List<rptJewelLoanIssueRegister>> GetJewelLoanIssueRegisterList(DateTime fromDate, DateTime toDate);
        //Task<List<rptJewelLoanRedemptionList>> GetJewelLoanRedemptionList(DateTime fromDate, DateTime toDate);
        //Task<List<rptJewelLoanOutstandingList>> GetJewelLoanOutstandingForMember(decimal memId, DateTime asOnDate);
        //Task<List<rptJewelLoanOutstandingSchemeWiseList>> GetJewelLoanOutstandingSchemeWiseList(DateTime asOnDate);
        //Task<List<rptJewelLoanLedgerToMember>> GetJewelLoanLedgerToMembers(List<decimal> loanIdList, DateTime fromDate, DateTime toDate);
        //Task<List<rptJewelLoanLedgerMain>> Print_JewelLoanLedgerMain(List<decimal> loanIdList);
        //Task<List<rptJewelLoanOverdueSchemeWiseList>> GetJewelLoanOverdueSchemeWiseList(DateTime asOnDate);
        Task<List<rptJewelLoanVerificationList>> GetJewelLoanVerificationList(DateTime asOnDate, string brCode);
        Task<List<rptJewelLoanStockRegisterList>> GetJewelLoanStockRegisteList(DateTime fromDate, DateTime toDate, int loanType, string brCode);
        Task<List<rptJewelLoanOutstandingList>> GetJewelLoanOutstandingList(DateTime asOnDate, string brCode);
        Task<List<rptJewelLoanOverdueList>> GetJewelLoanOverdueList(DateTime asOnDate, string brCode);
        Task<List<rptJewelLoanOverdueList>> GetJewelLoanAboveLimitList(DateTime asOnDate, double limitAmt, string brCode);
        Task<List<rptJewelLoanIssueRegister>> GetJewelLoanIssueRegisterList(DateTime fromDate, DateTime toDate, string brCode);
        Task<List<rptJewelLoanRedemptionList>> GetJewelLoanRedemptionList(DateTime fromDate, DateTime toDate, string brCode);
        Task<List<rptJewelLoanOutstandingList>> GetJewelLoanOutstandingForMember(decimal memId, DateTime asOnDate);
        Task<List<rptJewelLoanOutstandingSchemeWiseList>> GetJewelLoanOutstandingSchemeWiseList(DateTime asOnDate, string brCode);
        Task<List<rptJewelLoanLedgerToMember>> GetJewelLoanLedgerToMembers(List<decimal> loanIdList, DateTime fromDate, DateTime toDate);
        Task<List<rptJewelLoanLedgerMain>> Print_JewelLoanLedgerMain(List<decimal> loanIdList);
        Task<List<rptJewelLoanOverdueSchemeWiseList>> GetJewelLoanOverdueSchemeWiseList(DateTime asOnDate, string brCode);
    }
}
