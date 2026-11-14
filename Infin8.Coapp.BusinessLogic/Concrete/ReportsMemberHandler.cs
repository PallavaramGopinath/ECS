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

        public async Task<List<rptMemberVoutersList>> GetMemberAddress(List<int> memberTypeList, int memberStatus, string brCode)
        {
            return await _unitOfWork.ReportsMember.GetMemberAddress(memberTypeList, memberStatus, brCode);
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
        // ---------------------------------------------------------------------------------------------
        // Personal register (report 84), ported from TSIS GetPersonalRegister / GetPersonalRegisterObject.
        // One line per movement in the period, with the account's running opening and closing balance.
        // Departures from TSIS (bugs there): rows are not duplicated per ledger; loans first disbursed in the
        // period are included; penal received on a deposit is shown once (on the deposit line) instead of twice;
        // FR counts only money received, never declared dividend or booked interest.
        // ---------------------------------------------------------------------------------------------

        public async Task<byte[]> GetPersonalRegisterToPrintAsBytes(string datasetName, string fromMemNo, string toMemNo, DateTime fromDate, DateTime toDate, string brCode, FileStream reportStream)
        {
            var rows = await GetPersonalRegisterAsync(fromMemNo, toMemNo, fromDate, toDate, brCode);
            // PersonalRegisterMember.rdlc declares no report parameters
            return _createReportsHandler.CreateLocalReport(datasetName, reportStream, rows, new Dictionary<string, string>());
        }

        public async Task<List<rptPersonalRegister>> GetPersonalRegisterAsync(string fromMemNo, string toMemNo, DateTime fromDate, DateTime toDate, string brCode)
        {
            DateTime periodFrom = fromDate.Date;
            var members = await _unitOfWork.ReportsMember.GetRegisterMembersAsync(fromMemNo, toMemNo, brCode);
            if (members.Count == 0) return new List<rptPersonalRegister>();

            var map = await _unitOfWork.MapGeneral.GetMapGeneralAsync(brCode);
            var suretyIds = members.Where(m => m.SuretyMem_Id > 0).Select(m => m.SuretyMem_Id).Distinct().ToList();
            var sureties = (await _unitOfWork.ReportsMember.GetRegisterMembersByIdAsync(suretyIds, brCode)).ToDictionary(m => m.Mem_Id);
            var shareCapital = await _unitOfWork.ReportsMember.GetShareCapitalBalancesAsync(
                members.Select(m => m.Mem_Id).Concat(suretyIds), map?.ShareCapital_Led_Id ?? 0, toDate, brCode);
            var movements = await _unitOfWork.ReportsMember.GetRegisterMovementsAsync(members.Select(m => m.Mem_Id), toDate, brCode);

            var movementsByMember = movements.ToLookup(m => m.Mem_Id);
            var result = new List<rptPersonalRegister>();
            foreach (var member in members)
            {
                sureties.TryGetValue(member.SuretyMem_Id, out var surety);
                var header = new rptPersonalRegister
                {
                    Mem_Id = member.Mem_Id,
                    MemberNo = member.MemberNo,
                    Per_No = member.PerNo,
                    MemberName = member.MemberName,
                    TokenNo = member.TokenNo,
                    ReferName = member.OfficeName,
                    Mem_SC = shareCapital.GetValueOrDefault(member.Mem_Id),
                    Surety_memberNo = surety?.MemberNo,
                    Surety_Per_No = surety?.PerNo,
                    Surety_memberName = surety?.MemberName,
                    Surety_SC = surety == null ? 0 : shareCapital.GetValueOrDefault(surety.Mem_Id)
                };

                var accounts = movementsByMember[member.Mem_Id]
                                        .GroupBy(m => (m.Module, m.AccountKey))
                                        .OrderBy(g => ModuleOrder(g.Key.Module)).ThenBy(g => g.First().Acc_No).ThenBy(g => g.First().Acc_Name);
                foreach (var account in accounts)
                {
                    var before = account.Where(m => m.Trn_Date < periodFrom).ToList();
                    var during = account.Where(m => m.Trn_Date >= periodFrom).OrderBy(m => m.Trn_Date).ThenBy(m => m.Trn_SlNo).ToList();
                    if (during.Count == 0) continue;
                    result.AddRange(AccountLines(header, account.Key.Module, account.Key.AccountKey, before, during, map?.Dividend_Led_Id ?? 0));
                }
            }
            return result.OrderBy(r => r.MemberNo).ThenBy(r => r.Trn_Date).ToList();
        }

        private static int ModuleOrder(string module) => module switch
        {
            "SC" => 1, "LOAN" => 2, "DEP" => 3, "RD" => 4, "DUEBY" => 5, "DUETO" => 6, _ => 9
        };

        /// <summary>Register lines for one account: a principal line per movement, plus accrual lines (dividend / interest).</summary>
        private static IEnumerable<rptPersonalRegister> AccountLines(rptPersonalRegister header, string module, decimal key,
            List<PersonalRegisterMovement> before, List<PersonalRegisterMovement> during, decimal dividendLedId)
        {
            // principal balance: money in raises share capital, deposits and dues-by; loans and dues-to run the other way
            bool receiptRaises = module is "SC" or "DEP" or "RD" or "DUEBY";
            double Sign(PersonalRegisterMovement m) => receiptRaises ? m.Rpt - m.Pmt : m.Pmt - m.Rpt;
            double prl = before.Sum(Sign);
            double accrual = before.Sum(m => m.IntCalc - m.IntPaid);
            double penal = before.Sum(m => m.PICalc - m.PIPaid);

            foreach (var m in during)
            {
                string status = m.Voc_Type == 2 ? "F" : (string.IsNullOrEmpty(m.Voc_Rpt_No) ? "" : m.Voc_Rpt_No.Substring(0, 1));
                double prlOb = prl, accrualOb = accrual, penalOb = penal;
                prl += Sign(m);
                accrual += m.IntCalc - m.IntPaid;
                penal += m.PICalc - m.PIPaid;

                bool principalMoved = m.Rpt > 0 || m.Pmt > 0;
                if (module == "LOAN")
                {
                    if (principalMoved || m.IntCalc > 0 || m.IntPaid > 0 || m.PICalc > 0 || m.PIPaid > 0)
                    {
                        var line = Line(header, m, status, m.Acc_Name, module, key);
                        (line.Prl_OB, line.Prl_Rpt, line.Prl_Pmt, line.Prl_CB) = (prlOb, m.Rpt, m.Pmt, prl);
                        (line.Int_OB, line.IntCalc_Amt, line.Int_Rpt, line.Int_CB) = (accrualOb, m.IntCalc, m.IntPaid, accrual);
                        (line.PI_OB, line.PICalc_Amt, line.PI_Rpt, line.PI_CB) = (penalOb, m.PICalc, m.PIPaid, penal);
                        line.FR = status == "F" ? m.Rpt + m.IntPaid + m.PIPaid : 0;
                        yield return line;
                    }
                    continue;
                }

                if (principalMoved || ((module is "DEP" or "RD") && m.PIPaid > 0))
                {
                    var line = Line(header, m, status, m.Acc_Name, module, key);
                    (line.Prl_OB, line.Prl_Rpt, line.Prl_Pmt, line.Prl_CB) = (prlOb, m.Rpt, m.Pmt, prl);
                    if (module is "DEP" or "RD") line.Int_Rpt = m.PIPaid;      // penal received with the instalment
                    line.FR = status == "F" ? m.Rpt + line.Int_Rpt : 0;
                    yield return line;
                }
                if (m.IntCalc > 0 || m.IntPaid > 0)
                {
                    // accrual line: dividend on share capital, interest on deposits; shown as declared / paid / balance
                    string name = module == "SC" ? "Dividend" : "Int-" + m.Acc_Name;
                    var line = Line(header, m, status, name, module, key);
                    if (module == "SC") line.Led_Id = dividendLedId;
                    (line.Prl_OB, line.Prl_Rpt, line.Prl_Pmt, line.Prl_CB) = (accrualOb, m.IntCalc, m.IntPaid, accrual);
                    yield return line;
                }
            }
        }

        private static rptPersonalRegister Line(rptPersonalRegister h, PersonalRegisterMovement m, string status, string? accName, string module, decimal key) => new rptPersonalRegister
        {
            Mem_Id = h.Mem_Id, MemberNo = h.MemberNo, Per_No = h.Per_No, MemberName = h.MemberName, TokenNo = h.TokenNo, ReferName = h.ReferName,
            Mem_SC = h.Mem_SC, Surety_memberNo = h.Surety_memberNo, Surety_Per_No = h.Surety_Per_No, Surety_memberName = h.Surety_memberName,
            Surety_SC = h.Surety_SC,
            Loan_Id = module == "LOAN" ? key : 0,
            TD_Id = module == "RD" ? key : 0,
            DM_Id = module == "DEP" ? (int)key : 0,
            Led_Id = module is "SC" or "DUEBY" or "DUETO" ? key : 0,
            Trn_Date = m.Trn_Date,
            Status = status,
            Voc_No = m.Voc_No,
            Voc_Type = m.Voc_Type,
            Acc_No = m.Acc_No ?? "",
            Acc_Name = accName,
            MonthYear = $"{m.Trn_Date:yyyy MM}",
            MonthYearString = $"{m.Trn_Date:MMM yyyy}"
        };

        public async Task<byte[]> GetMemberAddressToPrintAsBytes(string datasetName, List<int> memberTypeList, int memberStatus, string brCode, FileStream reportStream, Dictionary<string, string> parameters)
        {
            var rows = await _unitOfWork.ReportsMember.GetMemberAddress(memberTypeList, memberStatus, brCode);
            return _createReportsHandler.CreateLocalReport(datasetName, reportStream, rows, parameters);
        }
        public async Task<byte[]> GetMemberCancellationToPrintAsBytes(string datasetName, DateTime fromDate, DateTime toDate, string brCode, FileStream reportStream, Dictionary<string, string> parameters)
        {
            var rows = await _unitOfWork.ReportsMember.GetMemberCancellation(fromDate, toDate, brCode);
            return _createReportsHandler.CreateLocalReport(datasetName, reportStream, rows, parameters);
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
