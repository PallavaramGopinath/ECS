using Infin8.Coapp.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IReportsMemberHandler
    {
        Task<List<rptMemberTrn>> GetRptMemberTrn(DateTime fromDate, DateTime toDate, int trnType, string brCode);
        Task<List<rptMemberTrn>> GetRptMemberTrnNew(DateTime fromDate, DateTime toDate, int trnType, string brCode);
        Task<List<rptMemberTrn>> GetRptMemberTrnOS(DateTime toDate, int trnType, string brCode);
        Task<List<rptMemberCancellation>> GetMemberCancellation(DateTime fromDate, DateTime toDate, string brCode);
        Task<List<rptMemberList>> GetMemberList(DateTime asOnDate, List<int> memberTypeList, int memberStatus, string brCode);
        Task<List<rptMemberRegister>> GetMemberRegister(decimal memId);
        Task<List<rptMemberVoutersList>> GetMemberVoutersList(DateTime asOnDate, List<int> memberTypeList, List<int> memberStatusList, int minimumSCBalance, string brCode);
        Task<List<rptMemberVoutersList>> GetMemberAddress(string fromMemNo, string toMemNo, List<int> memberTypeList, List<int> memberStatusList);
        Task<List<rptMemberNewAdmission>> GetRptNewMembers(DateTime fromDate, DateTime toDate, string brCode);
        //Task<List<rptMemberKYC>> GetMemberKYC(decimal memId);
        Task<byte[]> GetShareCapitalToPrintAsBytes(string datasetName, DateTime fromDate, DateTime toDate, int trnType, string brCode, FileStream reportStream, Dictionary<string, string> parameters);
        Task<byte[]> GetMemberTrnOSToPrintAsBytes(string datasetName,  DateTime toDate, int trnType, string brCode, FileStream reportStream, Dictionary<string, string> parameters);
        Task<byte[]> GetMemberListToPrintAsBytes(string datasetName, DateTime asOnDate, List<int> memberTypeList, int memberStatus, string brCode, FileStream reportStream, Dictionary<string, string> parameters);
        Task<byte[]> GetMemberVoutersListToPrintAsBytes(string datasetName, DateTime asOnDate, List<int> memberTypeList, int memberStatus, int minimumSCBalance, string brCode, FileStream reportStream, Dictionary<string, string> parameters);
        Task<byte[]> GetMemberRegisterToPrintAsBytes(string datasetName, decimal memId, FileStream reportStream, Dictionary<string, string> parameters);

        Task<byte[]> GetNewMembersToPrintAsBytes(string datasetName, DateTime fromDate, DateTime toDate, string brCode, FileStream reportStream, Dictionary<string, string> parameters);
        Task<byte[]> GetMemberKYCToPrintAsBytes(string datasetName, decimal memId, string brCode, FileStream reportStream, Dictionary<string, string> parameters);
    }
}
