using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using Microsoft.EntityFrameworkCore.Storage;
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
        readonly ICreateReportsHandler _createReportsHandler;
        public ReportsMemberHandler(IUnitOfWork unitOfWork, ICreateReportsHandler createReportsHandler)
        {
            _unitOfWork = unitOfWork;
            _createReportsHandler = createReportsHandler;
        }
        public async Task<List<rptMemberTrn>> GetRptMemberTrn(DateTime fromDate, DateTime toDate, int trnType, string brCode)
        {
            List<rptMemberTrn> memTrnList = new();
            double OB = 0, CB = 0;
            decimal memId = 0, ledId = 0;
            try
            {
                var trnList = await _unitOfWork.ReportsMember.GetRptMemberTrn(fromDate, toDate, trnType, brCode);
                if (trnList != null && trnList.Any())
                {
                    memTrnList = trnList.ToList();
                    foreach (var trn in trnList)
                    {
                        if (memId == trn.Mem_Id && ledId != trn.Led_Id)
                        {
                            ledId = trn.Led_Id;
                            OB = trn.Amt_OB;
                            CB = trn.Amt_OB;
                        }

                        if (memId != trn.Mem_Id && ledId != trn.Led_Id)
                        {
                            memId = trn.Mem_Id;
                            ledId = trn.Led_Id;
                            OB = trn.Amt_OB;
                            CB = trn.Amt_OB;
                        }

                        if (memId != trn.Mem_Id && ledId == trn.Led_Id)
                        {
                            memId = trn.Mem_Id;
                            OB = trn.Amt_OB;
                            CB = trn.Amt_OB;
                        }
                        switch (trn.Trn_Type)
                        {
                            case 1:
                            case 5:
                                CB += Convert.ToDouble(trn.Pmt_Amt) - Convert.ToDouble(trn.Rpt_Amt);
                                break;
                            case 2:
                            case 3:
                            case 6:
                                CB += Convert.ToDouble(trn.Rpt_Amt) - Convert.ToDouble(trn.Pmt_Amt);
                                break;
                        }
                        trn.Amt_CB = CB;
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in fetching member transaction list from handler " + ex.Message);
                memTrnList = new();
            }
            return memTrnList;
            //return await _unitOfWork.ReportsMember.GetRptMemberTrn (fromDate , toDate,  trnType,brCode  );
        }

        public async Task<List<rptMemberTrn>> GetRptMemberTrnNew(DateTime fromDate, DateTime toDate, int trnType, string brCode)
        {
            return await _unitOfWork.ReportsMember.GetRptMemberTrnNew( fromDate, toDate, trnType, brCode);
        }

        public async Task<List<rptMemberTrn>> GetRptMemberTrnOS(DateTime toDate, int trnType, string brCode)
        {
            return await _unitOfWork.ReportsMember.GetRptMemberTrnOS(toDate, trnType, brCode);
        }

        public async Task<List<rptMemberCancellation>> GetMemberCancellation(DateTime fromDate, DateTime toDate, string brCode)
        {
            return await _unitOfWork.ReportsMember.GetMemberCancellation(fromDate, toDate, brCode);
        }

        public async Task<List<rptMemberList>> GetMemberList(DateTime asOnDate, List<int> memberTypeList, int memberStatus, string brCode)
        {
            return await _unitOfWork.ReportsMember.GetMemberList(asOnDate, memberTypeList, memberStatus, brCode);
        }

        public async Task<List<rptMemberRegister>> GetMemberRegister(decimal memId)
        {
            return await _unitOfWork.ReportsMember.GetMemberRegister(memId);
        }

        public async Task<List<rptMemberVoutersList>> GetMemberVoutersList(DateTime asOnDate, List<int> memberTypeList, List<int> memberStatusList, int minimumSCBalance, string brCode)
        {
            return await _unitOfWork.ReportsMember.GetMemberVoutersList(asOnDate, memberTypeList, memberStatusList, minimumSCBalance, brCode);
        }

        public async Task<List<rptMemberVoutersList>> GetMemberAddress(string fromMemNo, string toMemNo, List<int> memberTypeList, List<int> memberStatusList)
        {
            return await _unitOfWork.ReportsMember.GetMemberAddress(fromMemNo, toMemNo, memberTypeList, memberStatusList);
        }
        public async Task<List<rptMemberNewAdmission>> GetRptNewMembers(DateTime fromDate, DateTime toDate, string brCode)
        {
            return await _unitOfWork.ReportsMember.GetRptNewMembers(fromDate, toDate, brCode);
        }

        //public async Task<List<rptMemberKYC>> GetMemberKYC(decimal memId)
        public async Task<byte[]> GetShareCapitalToPrintAsBytes(string datasetName, DateTime fromDate, DateTime toDate,int trnType, string brCode, FileStream reportStream, Dictionary<string, string> parameters)
        {
            var ShareCapitalList = await GetRptMemberTrnNew(fromDate,toDate, trnType, brCode);
             var shareCapitalEnumerable = ShareCapitalList as IEnumerable<rptMemberTrn> ?? ShareCapitalList.ToList();
            var pdfAsBytes = _createReportsHandler.CreateLocalReport(datasetName, reportStream, shareCapitalEnumerable, parameters);
            return pdfAsBytes;
        }

        public async Task<byte[]> GetMemberTrnOSToPrintAsBytes(string datasetName,  DateTime toDate, int trnType, string brCode, FileStream reportStream, Dictionary<string, string> parameters)
        {
            var ShareCapitalList = await GetRptMemberTrnOS(toDate, trnType, brCode);
            var shareCapitalEnumerable = ShareCapitalList as IEnumerable<rptMemberTrn> ?? ShareCapitalList.ToList();
            var pdfAsBytes = _createReportsHandler.CreateLocalReport(datasetName, reportStream, shareCapitalEnumerable, parameters);
            return pdfAsBytes;
        }

        public async Task<byte[]> GetMemberListToPrintAsBytes(string datasetName, DateTime asOnDate, List<int> memberTypeList, int memberStatus, string brCode, FileStream reportStream, Dictionary<string, string> parameters)
        {
            var memberList = await GetMemberList(asOnDate,memberTypeList, memberStatus ,brCode);
            var memberListEnumerable = memberList as IEnumerable<rptMemberList> ?? memberList.ToList();
            var pdfAsBytes = _createReportsHandler.CreateLocalReport(datasetName, reportStream, memberListEnumerable, parameters);
            return pdfAsBytes;
        }

        public async Task<byte[]> GetMemberVoutersListToPrintAsBytes(string datasetName, DateTime asOnDate, List<int> memberTypeList, int memberStatus, int minimumSCBalance, string brCode, FileStream reportStream, Dictionary<string, string> parameters)
        {
            var memberList = await GetMemberVoutersList(asOnDate ,memberTypeList, new List<int> { memberStatus }, minimumSCBalance , brCode);
            var memberListEnumerable = memberList as IEnumerable<rptMemberVoutersList> ?? memberList.ToList();
            var pdfAsBytes = _createReportsHandler.CreateLocalReport(datasetName, reportStream, memberListEnumerable, parameters);
            return pdfAsBytes;
        }

        public async Task<byte[]> GetMemberRegisterToPrintAsBytes(string datasetName, decimal memId,  FileStream reportStream, Dictionary<string, string> parameters)
        {
            var memberRegister = await GetMemberRegister(memId);
            var memberListEnumerable = memberRegister as IEnumerable<rptMemberRegister> ?? memberRegister.ToList();
            var pdfAsBytes = _createReportsHandler.CreateLocalReport(datasetName, reportStream, memberListEnumerable, parameters);
            return pdfAsBytes;
        }

        public async Task<byte[]> GetNewMembersToPrintAsBytes(string datasetName, DateTime fromDate, DateTime toDate,  string brCode, FileStream reportStream, Dictionary<string, string> parameters)
        {
            var memberList = await GetRptNewMembers (fromDate, toDate, brCode);
            var memberListEnumerable = memberList as IEnumerable<rptMemberNewAdmission> ?? memberList.ToList();
            var pdfAsBytes = _createReportsHandler.CreateLocalReport(datasetName, reportStream, memberListEnumerable, parameters);
            return pdfAsBytes;
        }
        public async Task<byte[]> GetMemberKYCToPrintAsBytes(string datasetName, decimal memId, string brCode, FileStream reportStream, Dictionary<string, string> parameters)
        {
            var MemberKYC = await _unitOfWork.ReportsMember.GetMemberKYC(memId, brCode);
            var kycEnumerable = MemberKYC as IEnumerable<rptMemberKYC> ?? MemberKYC.ToList();
            var pdfAsBytes = _createReportsHandler.CreateLocalReport(datasetName, reportStream, kycEnumerable, parameters);
            return pdfAsBytes;
        }

    }
}
