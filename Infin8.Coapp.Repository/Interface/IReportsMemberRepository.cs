using Infin8.Coapp.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface IReportsMemberRepository
    {
        Task<List<rptMemberTrn>> GetRptMemberTrn(DateTime fromDate, DateTime toDate, int trnType,string brCode);
        Task<List<rptMemberTrn>> GetRptMemberTrnNew(DateTime fromDate, DateTime toDate, int trnType, string brCode);
        Task<List<rptMemberTrn>> GetRptMemberTrnOS(DateTime toDate, int trnType, string brCode);
        /// <summary>Members whose number lies in [fromMemNo, toMemNo], for the personal register (report 84).</summary>
        Task<List<PersonalRegisterMember>> GetRegisterMembersAsync(string fromMemNo, string toMemNo, string brCode);
        Task<List<PersonalRegisterMember>> GetRegisterMembersByIdAsync(IEnumerable<decimal> memIds, string brCode);
        /// <summary>Every movement of the members up to toDate: share capital, dues by/to, loans, deposits, RDs.</summary>
        Task<List<PersonalRegisterMovement>> GetRegisterMovementsAsync(IEnumerable<decimal> memIds, DateTime toDate, string brCode);
        /// <summary>Share capital balance (Trn_Type 3, the share capital ledger) per member as on a date.</summary>
        Task<Dictionary<decimal, double>> GetShareCapitalBalancesAsync(IEnumerable<decimal> memIds, decimal shareCapitalLedId, DateTime asOnDate, string brCode);
        Task<List<rptMemberCancellation>> GetMemberCancellation(DateTime fromDate, DateTime toDate, string brCode);
        Task<List<rptMemberList>> GetMemberList(DateTime asOnDate, List<int> memberTypeList, int memberStatus, string brCode);
        Task<List<rptMemberRegister>> GetMemberRegister(decimal memId);
        Task<List<rptMemberVoutersList>> GetMemberVoutersList(DateTime asOnDate, List<int> memberTypeList, List<int> memberStatusList, int minimumSCBalance, string brCode);
        Task<List<rptMemberVoutersList>> GetMemberAddress(List<int> memberTypeList, int memberStatus, string brCode);
        Task<List<rptMemberNewAdmission>> GetRptNewMembers(DateTime fromDate, DateTime toDate, string brCode);
        Task<List<rptMemberKYC>> GetMemberKYC(decimal memId,string brCode);
    }
}
