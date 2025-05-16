using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class ReportsMemberHandler : IReportsMemberHandler
    {

        readonly IUnitOfWork _unitOfWork;
        public ReportsMemberHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<List<rptMemberTrn>> GetRptMemberTrn(DateTime fromDate, DateTime toDate, int trnType,string brCode)
        {
            return await _unitOfWork.ReportsMember.GetRptMemberTrn (fromDate , toDate,  trnType,brCode  );
        }

        public async Task<List<rptMemberTrn>> GetRptMemberTrn(DateTime toDate, int trnType,string brCode)
        {
            return await _unitOfWork.ReportsMember.GetRptMemberTrn(toDate, trnType,brCode  );
        }

        public async Task<List<rptMemberCancellation>> GetMemberCancellation(DateTime fromDate, DateTime toDate, string brCode)
        {
            return await _unitOfWork.ReportsMember.GetMemberCancellation( fromDate, toDate,brCode  );
        }

        public async Task<List<rptMemberList>> GetMemberList(DateTime asOnDate, List<int> memberTypeList, List<int> memberStatusList, string brCode)
        {
            return await _unitOfWork.ReportsMember.GetMemberList(asOnDate, memberTypeList, memberStatusList,brCode  );
        }

        public async Task<List<rptMemberRegister>> GetMemberRegister(int memId)
        {
            return await _unitOfWork.ReportsMember.GetMemberRegister(memId);
        }

        public async Task<List<rptMemberVoutersList>> GetMemberVoutersList(DateTime asOnDate, List<int> memberTypeList, List<int> memberStatusList, int minimumSCBalance,string brCode)
        {
            return await _unitOfWork.ReportsMember.GetMemberVoutersList(asOnDate,memberTypeList, memberStatusList, minimumSCBalance, brCode);
        }
            
        public async Task<List<rptMemberVoutersList>> GetMemberAddress(string fromMemNo, string toMemNo, List<int> memberTypeList, List<int> memberStatusList)
        {
            return await _unitOfWork.ReportsMember.GetMemberAddress(fromMemNo, toMemNo, memberTypeList,memberStatusList);
        }
        public async Task<List<rptMemberNewAdmission>> GetRptNewMembers(DateTime fromDate, DateTime toDate, string brCode)
        {
            return await _unitOfWork.ReportsMember.GetRptNewMembers(fromDate, toDate, brCode);
        }

        public async Task<List<rptMemberKYC>> GetMemberKYC(int memId)
        {
            return await _unitOfWork.ReportsMember.GetMemberKYC(memId);
        }

        
    }
}
