using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Utility
{
    public static class GetModalObject
    {
        #region Member
        public static mem_master GetMemberMasterObjectNew(int mem_Id, string memberNo, int memberType, string PerNo, string ResolutionNo, DateTime? ResolutionDate, byte[] PhotoImage, byte[] SignatureImage, byte[] AadharImage, string memberName, string FatherName, int Gender, int casteId, DateTime? dob, int age, string Alternative_Mobileno,
            string OfficePhoneNo, string mobileNo, string EMailId, DateTime? DOJ, DateTime? DOR, int DesignationId, bool IsPermanent, int OfficeId, string TicketTokenGangNo,
            string PreAdd1, string PreAdd2, string PreAdd3, string PrePin, int PreCity, string PerAdd1, string PerAdd2, string PerAdd3, string PerPin,
            int PerCity, double BasicPay, int MemberStatus, bool IsAccountClosed, DateTime? AccountClosedDate, bool IsExistingMember, int ExistingMemberId, string ExistingMemberNo,
            DateTime? ExistingDoc, bool IsMember_OtherSociety, string NomineeName, int NomineeAge, string NomineeRelationShip, bool MemberDelete, bool IsNewMember, int SurityMem_Id,
            int usr_Id, int Yr_Id, double Income, int Comm_id, int Occ_id, int religion_id, bool Member_Oe, string SectionCode, string InActiveStatus, bool IsMemExpired, DateTime? ExpiredDate,
            string Designation, string Section, int FactorySection_Id, int FactoryTrade_Id, string SBAccountNo, string BankName, string IFCCode, DateTime? AdmissionDate, string Token_PersonNo,
            int GrossPay, string GPF_No, string PANNo, string AadharNo, string SmartCardNo,string brCode)
        {
            mem_master memMaster = new mem_master();
            try
            {
                memMaster.mem_id = mem_Id;
                memMaster.memberno = memberNo;
                memMaster.membertype = memberType;
                memMaster.perno = PerNo;
                memMaster.resolutionno = ResolutionNo;
                memMaster.resolutiondate = ResolutionDate;
                memMaster.memberphoto = null;
                memMaster.memberesignature = null;

                memMaster.membername = memberName;
                memMaster.fathername = FatherName;
                memMaster.gender = Gender;
                memMaster.caste_id = casteId;
                memMaster.dob = dob;
                memMaster.age = age;
                memMaster.alternative_mobileno = Alternative_Mobileno;
                memMaster.officephoneno = OfficePhoneNo;
                memMaster.mobileno = mobileNo;

                memMaster.emailid = EMailId;
                memMaster.doj = DOJ;
                memMaster.dor = DOR;
                memMaster.designation_id = DesignationId;
                memMaster.ispermanent = IsPermanent;
                memMaster.office_id = OfficeId;
                memMaster.tickettokengangno = TicketTokenGangNo;
                memMaster.passingofficername = "";

                memMaster.preadd1 = PreAdd1;
                memMaster.preadd2 = PreAdd2;
                memMaster.preadd3 = PreAdd3;
                memMaster.prepin = PrePin;
                memMaster.precity = PreCity;

                memMaster.peradd1 = PerAdd1;
                memMaster.peradd2 = PerAdd2;
                memMaster.peradd3 = PerAdd3;
                memMaster.perpin = PerPin;
                memMaster.percity = PerCity;

                memMaster.basicpay = BasicPay;
                memMaster.memberstatus = MemberStatus;
                memMaster.isaccountclosed = IsAccountClosed;
                memMaster.accountcloseddate = AccountClosedDate;

                memMaster.isexistingmember = IsExistingMember;
                memMaster.existing_memberno = ExistingMemberNo;
                memMaster.existingdoc = ExistingDoc;

                memMaster.ismember_othersociety = IsMember_OtherSociety;

                memMaster.nomineename = NomineeName;
                memMaster.nomineeage = NomineeAge;
                memMaster.nomineerelationship = NomineeRelationShip;

                memMaster.memberdelete = MemberDelete;

                memMaster.isnewmember = IsNewMember;

                memMaster.suretymem_id = SurityMem_Id;
                memMaster.usr_id = usr_Id;
                memMaster.yr_id = Yr_Id;
                memMaster.income = Income;

                memMaster.comm_id = Comm_id;
                memMaster.occ_id = Occ_id;
                memMaster.religion_id = religion_id;
                memMaster.member_oe = Member_Oe;
                memMaster.sectioncode = SectionCode;
                memMaster.inactivestatus = InActiveStatus;
                memMaster.expireddate = ExpiredDate;

                memMaster.designation = Designation;
                memMaster.sectioncode = Section;

                memMaster.factorysection_id = FactorySection_Id;
                memMaster.factorytrade_id = FactoryTrade_Id;

                memMaster.sbaccountno = SBAccountNo;
                memMaster.bankname = BankName;
                memMaster.ifsccode = IFCCode;

                memMaster.admissiondate = AdmissionDate;

                memMaster.token_personno = Token_PersonNo;
                memMaster.grosspay = GrossPay;
                memMaster.gpf_no = GPF_No;
                memMaster.panno = PANNo;
                memMaster.aadharno = AadharNo;
                memMaster.aadharcardpath = null;
                memMaster.smartcardno = SmartCardNo;
                memMaster.brcode = brCode;
                
            }
            catch (Exception)
            {
                memMaster = new();
            }
            return memMaster;
        }

        public static Mem_Trn GetMemTrnObject(byte TrnType, decimal MemId, decimal LedId, DateTime TrnDate, double RptAmt, double PmtAmt, bool memOE, bool MemDelete, decimal VocId, decimal UsrId,
            decimal YrId, int TrnSlNo, int IntCalcAmt, DateTime? IntCalcDate, int IntPaidAmt, string Status, int PbleMasterId, decimal AccId, int DemandAmt,string brCode)
        {
            Mem_Trn mem = new ();
            try
            {
                mem.Mem_Trn_Id = 0;
                mem.Trn_Type = TrnType;
                mem.Mem_Id = MemId;
                mem.Led_Id = LedId;
                mem.Trn_Date = TrnDate;
                mem.Rpt_Amt = RptAmt;
                mem.Pmt_Amt = PmtAmt;
                mem.MemTrn_OE = memOE;
                mem.MemTrn_Delete = MemDelete;
                mem.Voc_Id = VocId;
                mem.Usr_Id = UsrId;
                mem.Yr_Id = YrId;
                mem.Trn_SlNo = TrnSlNo;
                mem.IntCalc_Amt = IntCalcAmt;
                mem.IntCalc_Date = IntCalcDate;
                mem.IntPaid_Amt = IntPaidAmt;
                mem.Amt_CB = 0;
                mem.Int_CB = 0;
                mem.Amt_OB = 0;
                mem.PbleMaster_Id = PbleMasterId;
                mem.Acc_Id = AccId;
                mem.Status = Status;
                mem.Demand_Amt = DemandAmt;
                mem.BrCode = brCode;
            }
            catch (Exception )
            {
            }
            return mem;
        }

        public static Mem_Transfer_Account GetMemTransferAccountObject(DateOnly Transfer_Date, decimal FromMem_Id, decimal FromLed_Id, double FromPmt_Amt, decimal ToMem_Id, decimal ToLed_Id, double ToRpt_Amt, decimal Voc_Id, decimal Usr_Id, decimal Yr_Id,string brCode)
        {
            Mem_Transfer_Account mem = new ();
            try
            {
                mem.MemTransfer_Id = 0;
                mem.Transfer_Date = Transfer_Date;
                mem.FromMem_Id = FromMem_Id;
                mem.FromLed_Id = FromLed_Id;
                mem.FromPmt_Amt = FromPmt_Amt;
                mem.ToMem_Id = ToMem_Id;
                mem.ToLed_Id = ToLed_Id;
                mem.ToRpt_Amt = ToRpt_Amt;
                mem.Voc_Id = Voc_Id;
                mem.MemTransfer_Delete = false;
                mem.Usr_Id = Usr_Id;
                mem.Yr_Id = Yr_Id;
                mem.BrCode = brCode;
            }
            catch (Exception)
            {
                mem = new();
            }
            return mem;
        }

        public static Bank_Master GetBankMasterObject(int Bank_Id, string Bank_ShortName, string Bank_Name, bool Bank_Delete, int usrId, int yrId)
        {
            Bank_Master bank = new();
            try
            {
                bank.Bank_Id = Bank_Id;
                bank.Bank_ShortName = Bank_ShortName;
                bank.Bank_Name = Bank_Name; bank.Bank_Delete = Bank_Delete;
                bank.Usr_Id = usrId;
                bank.Yr_Id = yrId;
            }
            catch (Exception )
            {
            }
            return bank;
        }
        #endregion

        #region Accounts
        public static Fin_Voucher GetFinVoucherObject(decimal vocId, string vocNo, int vocSlNo, DateTime vocDate, int vocType, string vocMode, double vocAmt,
            bool vocBuss, int vocRptSlNo, string? vocRptNo, string? vocRptMode, int vocPmtSlNo, string? vocPmtNo, string? vocPmtMode, decimal gUsrId, decimal gYrId,
            string brCode,decimal MemId = 0, string Narration = "")
        {
            Fin_Voucher voc = new();
            try
            {
                vocNo = "";
                if (!string.IsNullOrWhiteSpace(vocRptNo))
                    vocNo = vocRptNo;
                if (!string.IsNullOrWhiteSpace(vocPmtNo))
                {
                    if (!string.IsNullOrWhiteSpace(vocNo))
                        vocNo += "-" + vocPmtNo;
                    else
                        vocNo = vocPmtNo;
                }

                voc.Voc_Id = vocId;
                voc.Voc_SlNo = vocSlNo;
                voc.Voc_No = vocNo;
                voc.Voc_Rpt_SlNo = vocRptSlNo;
                voc.Voc_Rpt_No = vocRptNo;
                voc.Voc_Rpt_Mode = vocRptMode;
                voc.Voc_Pmt_SlNo = vocPmtSlNo;
                voc.Voc_Pmt_No = vocPmtNo;
                voc.Voc_Pmt_Mode = vocPmtMode;
                voc.Voc_Date = vocDate.Date;
                voc.Voc_Type = vocType;
                voc.Voc_Mode = vocMode;
                voc.Voc_Amt = vocAmt;
                voc.Voc_Narration = Narration;
                voc.Voc_Bus = vocBuss;
                voc.Voc_Delete = false;
                voc.Usr_Id = gUsrId;
                voc.Yr_Id = gYrId;
                voc.Mem_Id = MemId;
                voc.BrCode = brCode;

            }
            catch (Exception)
            {
                voc = new();
            }
            return voc;
        }

        public static Fin_Voucher_Trn GetFinVoucherTrObject(decimal VocId, decimal LedId, double ReceiptAmount, double Paymentamount, int TrnType, string Narration, bool TrDeleted, decimal UsrId,
            decimal YrId, string Status, string Description, decimal MemId, string brCode, decimal loanId = 0, double outstanding = 0, double balance = 0)
        {
            Fin_Voucher_Trn voucherTr = new ();
            try
            {

                voucherTr.Voc_Trn_Id = 0;
                voucherTr.Voc_Id = VocId;
                voucherTr.Led_Id = LedId;
                voucherTr.Voc_Rpt = ReceiptAmount;
                voucherTr.Voc_Pmt = Paymentamount;
                voucherTr.Voc_Trn_Type = TrnType;
                voucherTr.Voc_Narr = Narration;
                voucherTr.Voc_Cash_Adj_Id = 0;
                voucherTr.FinVocTr_Delete = TrDeleted;
                voucherTr.Usr_Id = UsrId;
                voucherTr.Yr_Id = YrId;
                voucherTr.Status = Status;
                voucherTr.Description = Description;
                voucherTr.Mem_Id = MemId;
                voucherTr.Loan_Id = loanId;
                voucherTr.Outstanding = outstanding;
                voucherTr.Balance = balance;
                voucherTr.BrCode = brCode;
            }
            catch (Exception)
            {

            }
            return voucherTr;
        }

        public static Fin_Voucher_Bank GetFinVocBankObject(DateTime TrnDate, decimal MemId, string BankMode, string BankName, decimal VocId, decimal LedId, double Amount, string ChequeNo,
            DateTime? Chequedate, DateTime? ChequeRealisedDate, double ChequeRealisedAmount, bool IsChequeReturned, DateTime? ChequeReturnedDate, string ChequeReturnedReason,
            DateTime? ChequeReturnDate, decimal ChequeReturnedVoc_Id, decimal UsrId, decimal YrId, bool gvbDelete, string brCode)
        {
            Fin_Voucher_Bank genVoc = new ();
            try
            {
                genVoc.Fvb_Id = 0;
                genVoc.Fvb_Date = TrnDate;
                genVoc.Mem_Id = MemId;
                genVoc.Fvb_Bank_Mode = BankMode;
                genVoc.Fvb_Bank_Name = BankName;
                genVoc.Voc_Id = VocId;
                genVoc.Led_Id = LedId;
                genVoc.Fvb_Amount = Amount;
                genVoc.Fvb_Cheque_No = ChequeNo;
                genVoc.Fvb_Cheque_Date = Chequedate;
                genVoc.Fvb_Cheque_Real_Date = ChequeRealisedDate;
                genVoc.Fvb_Real_Amount = ChequeRealisedAmount;
                genVoc.Fvb_Cheque_Return = IsChequeReturned;
                genVoc.Fvb_Cheque_Return_Date = ChequeReturnedDate;
                genVoc.Fvb_Reason = ChequeReturnedReason;
                genVoc.Fvb_Return_Voc_Id = ChequeReturnedVoc_Id;
                genVoc.Usr_Id = UsrId;
                genVoc.Yr_Id = YrId;
                genVoc.Fvb_Delete = gvbDelete;
                genVoc.BrCode = brCode;
            }
            catch (Exception)
            {
                genVoc = new();
            }
            return genVoc;
        }

        public static Fin_Ledger_Grp GetFinLedgerGrp(int Grp_Id, string Grp_Name, int Fnl_Id, bool Grp_Mapped, int usr_Id, int Grp_SlNo, bool Grp_Delete,string brCode)
        {
            Fin_Ledger_Grp grp = new ();
            try
            {
                grp.Grp_Id = Grp_Id;
                grp.Grp_Name = Grp_Name;
                grp.Fnl_Id = Fnl_Id;
                grp.Grp_Mapped = Grp_Mapped;
                grp.Usr_Id = usr_Id;
                grp.Grp_SlNo = Grp_SlNo;
                grp.Grp_Delete = Grp_Delete;
                grp.BrCode = brCode;
            }
            catch (Exception)
            {
                grp = new();
            }
            return grp;
        }

        #endregion

        #region Loan
        public static Loan_Master GetLoanMasterObject(int LoanSchemeId, string LoanNo, decimal MemId, int AppId, string AppNo, DateTime? LedDate, string ResNo,
            DateTime? ResDate, double SanAmt, DateTime SanDate, int LoanType, int PurId, int subGrpId, int GrpId, int AgsId, int PrlPrd, int IntPrd,
            DateTime FirstIntDueDate, DateTime? FirstPrlDueDate, int InstDate, string ReimLNo, string ReimDVNo, string MortSlNo, DateTime? MortExeDate, DateTime? MortRegDate,
            int MortSRO, double ROI, double PI, DateTime? DisbDate, double InstAmt, bool LoanOE, bool LoanDelete, bool IsAccountClosed, decimal VocId, decimal UsrId,
            decimal YrId, double SecurityFaceValue, double DrawingPower, string brCode,  int LoanId = 0, double IntDueAmt = 0, int SecurityType = 0)
        {
            Loan_Master lnMaster = new Loan_Master();
            try
            {
                lnMaster.Loan_Id = LoanId;
                lnMaster.Scheme_Id = LoanSchemeId;
                lnMaster.Loan_No = LoanNo;
                lnMaster.Mem_Id = MemId;
                lnMaster.App_Id = AppId;
                lnMaster.App_No = AppNo;
                lnMaster.Led_Date = LedDate;
                lnMaster.Res_No = ResNo;
                lnMaster.Res_Date = ResDate;
                lnMaster.San_Amt = SanAmt;
                lnMaster.San_Date = SanDate;
                lnMaster.Loan_Type = LoanType;
                lnMaster.Loan_Status = 1;
                lnMaster.Pur_Id = PurId; ;
                lnMaster.SubGrp_Id = subGrpId;
                lnMaster.Grp_Id = GrpId;
                lnMaster.Ags_Id = AgsId;
                lnMaster.Diff_Prd = 0;
                lnMaster.DiffIntRec_Prd = 0;
                lnMaster.Grace_Prd = 0;
                lnMaster.Prl_Prd = PrlPrd;
                lnMaster.Int_Prd = IntPrd;
                lnMaster.FirstInt_DueDate = FirstIntDueDate.Date;
                if (FirstPrlDueDate != null)
                    lnMaster.FirstPrl_DueDate = FirstPrlDueDate.Value.Date;
                else
                    lnMaster.FirstPrl_DueDate = null;
                lnMaster.Last_DueDate = null;
                lnMaster.LastDemand_Date = null;
                lnMaster.Inst_Date = InstDate;
                lnMaster.Reim_LNo = ReimLNo;
                lnMaster.Reim_DvNo = ReimDVNo;
                lnMaster.Mort_SlNo = MortSlNo;
                lnMaster.Mort_ExeDt = MortExeDate;
                lnMaster.Mort_RegDate = MortRegDate;
                lnMaster.Mort_Sro = MortSRO;
                lnMaster.Roi = ROI;
                lnMaster.Pi = PI;
                lnMaster.Inst_Amt = InstAmt;

                lnMaster.LastIntApplication_Date = null;
                lnMaster.NextIntApplication_Date = null;
                lnMaster.LastPIApplication_Date = null;
                lnMaster.LastIODApplication_Date = null;
                lnMaster.Loan_Oe = LoanOE;
                lnMaster.Loan_Delete = LoanDelete;
                lnMaster.IsAccountClosed = IsAccountClosed;
                lnMaster.AccountClosedDate = null;
                lnMaster.Voc_Id = VocId;
                lnMaster.Usr_Id = UsrId;
                lnMaster.Yr_Id = YrId;
                lnMaster.SecurityFaceValue = SecurityFaceValue;
                lnMaster.DrawingPower = DrawingPower;
                lnMaster.SecurityType = SecurityType;
                lnMaster.BrCode = brCode;
            }
            catch (Exception)
            {
                lnMaster = new();
            }
            return lnMaster;
        }

        public static Loan_Trn GetLoanTrnObject(decimal LoanId, int DemandId, string Status, DateTime TrnDate, DateTime? DueDate, DateTime? DisbDate, double DisbAmt, double PrlSched,
            double PrlDem, double PICalcAmt, DateTime? PICalcDate, double IODCalcAmt, DateTime? IODCalcDate, double IntCalcAmt, DateTime? IntCalcDate, double PICollAmt,
            double IODCollAmt, double IntCollAmt, double PrlCollAmt, double PrlReimSchedule, double PrlReimDemand, double PIReimCalcAmt, DateTime? PIReimCalcDate, double IODReimCalcAmt,
            DateTime? IODReimCalcDate, double IntReimCalcAmt, DateTime? IntReimCalcDate, double SOCROI, double SOCPI, double ReimROI, double ReimPI, bool TrnOE, bool TrnDelete,
            decimal VocId, decimal UsrId, decimal YrId, int TrnSlNo, bool ExpiredDeletion, double PrlOS, double PrlOD, double IntBal, double PIBal, double IODBal, string brCode)
        {
            Loan_Trn lnTrn = new Loan_Trn();
            try
            {
                lnTrn.Trn_Id = 0;
                lnTrn.Loan_Id = LoanId;
                lnTrn.Demand_Id = DemandId;
                lnTrn.Trn_Status = Status;
                lnTrn.Trn_Date = TrnDate;
                lnTrn.Due_Date = DueDate;
                lnTrn.Disb_Date = DisbDate;
                lnTrn.Disb_Amt = DisbAmt;
                lnTrn.Prl_Sched = PrlSched;
                lnTrn.Prl_Dem = PrlDem;
                lnTrn.PICalc_Amt = PICalcAmt;
                lnTrn.PICalc_Date = PICalcDate;
                lnTrn.IODCalc_Amt = IODCalcAmt;
                lnTrn.IODCalc_Date = IODCalcDate;
                lnTrn.IntCalc_Amt = IntCalcAmt;
                lnTrn.IntCalc_Date = IntCalcDate;
                lnTrn.PIColl_Amt = PICollAmt;
                lnTrn.IODColl_Amt = IODCollAmt;
                lnTrn.IntColl_Amt = IntCollAmt;
                lnTrn.PrlColl_Amt = PrlCollAmt;
                lnTrn.PrlReim_Schedule = PrlReimSchedule;
                lnTrn.PrlReim_Demand = PrlReimDemand;
                lnTrn.PIReimCalc_Amt = PIReimCalcAmt;
                lnTrn.PIReimCalc_Date = PIReimCalcDate;
                lnTrn.IODReimCalc_Amt = IODReimCalcAmt;
                lnTrn.IODReimCalc_Date = IODReimCalcDate;
                lnTrn.IntReimCalc_Amt = IntReimCalcAmt;
                lnTrn.IntReimCalc_Date = IntReimCalcDate;
                lnTrn.Soc_Roi = SOCROI;
                lnTrn.SocPI_Rate = SOCPI;
                lnTrn.Reim_Roi = ReimROI;
                lnTrn.ReimPi_Rate = ReimPI;
                lnTrn.TrnTr_Oe = TrnOE;
                lnTrn.TrnTr_Delete = TrnDelete;
                lnTrn.Voc_Id = VocId;
                lnTrn.Usr_Id = UsrId;
                lnTrn.Yr_Id = YrId;
                lnTrn.Trn_SlNo = 0;
                lnTrn.ExpiredDeletion = ExpiredDeletion;
                lnTrn.BrCode = brCode;
            }
            catch (Exception)
            {
                lnTrn = new();
            }
            return lnTrn!;
        }

        public static Loan_Disb GetLoanDisbursementObject(decimal LoanId, DateTime DisbDate, int SocDisbNo, double SocDisbAmt, DateTime? SocDisbDate, DateTime? SocDisbChequeDate,
            string SocDisbChequeNo, DateTime? SocEncashDate, double ReimAmt, DateTime? ReimDate, string ReimChequeNo, DateTime? ReimChequeDate, bool DisbOE, int DisbSlNo, bool isFinalDisbursement,
            bool disbDelete, decimal VocId, decimal UsrId, decimal Yrid,string brCode)
        {
            Loan_Disb lnDisb = new Loan_Disb();
            try
            {
                lnDisb.Disb_Id = 0;
                lnDisb.Loan_Id = LoanId;
                lnDisb.Disb_Date = DisbDate;
                lnDisb.SocDisb_No = SocDisbNo;
                lnDisb.SocDisb_Amt = SocDisbAmt;
                lnDisb.SocDisb_Date = SocDisbDate;
                lnDisb.SocDisbCheque_Date = SocDisbChequeDate;
                lnDisb.SocDisbCheque_No = SocDisbChequeNo;
                lnDisb.SocEncash_Date = SocEncashDate;
                lnDisb.Reim_Amt = ReimAmt;
                lnDisb.Reim_Date = ReimDate;
                lnDisb.ReimCheque_No = ReimChequeNo;
                lnDisb.ReimCheque_Date = ReimChequeDate;
                lnDisb.Disb_Oe = DisbOE;
                lnDisb.Disb_SlNo = DisbSlNo;
                lnDisb.Is_FinalDisbursement = isFinalDisbursement;
                lnDisb.LoanDisb_Delete = disbDelete;
                lnDisb.Voc_Id = VocId;
                lnDisb.Usr_Id = UsrId;
                lnDisb.Yr_Id = Yrid;
                lnDisb.BrCode = brCode;
            }
            catch (Exception)
            {
                lnDisb = new();
            }
            return lnDisb;
        }

        public static Loan_Roi GetLoanROIObject(decimal LoanId, string Agency, DateTime Wef, double ROI, double PI, int Prd, double InstalAmt, bool roiDelete, bool roiOE, decimal VocId,
            decimal UsrId, decimal YrId,string brCode)
        {
            Loan_Roi lnRoi = new Loan_Roi();
            try
            {
                lnRoi.Roi_Id = 0;
                lnRoi.Loan_Id = LoanId;
                lnRoi.Agency = Agency;
                lnRoi.Roi_Wef = Wef;
                lnRoi.Roi = ROI;
                lnRoi.Pi = PI;
                lnRoi.Prd = Prd;
                lnRoi.InstallmentAmount = InstalAmt;
                lnRoi.Loanroi_Delete = roiDelete;
                lnRoi.LoanRoi_Oe = roiOE;
                lnRoi.Voc_Id = VocId;
                lnRoi.Usr_Id = UsrId;
                lnRoi.Yr_Id = YrId;
                lnRoi.BrCode = brCode;
            }
            catch (Exception)
            {
                lnRoi = new();
            }
            return lnRoi!;
        }

        public static Loan_Inst GetLoanInstalmentObject(decimal LoanId, DateTime InstWef, double InstAmt, bool InstOE, bool InstDelete, decimal VocId, decimal UsrId, decimal YrId, string brCode)
        {
            Loan_Inst inst = new Loan_Inst();
            try
            {
                inst.Inst_Id = 0;
                inst.Loan_Id = LoanId;
                inst.Inst_Wef = InstWef;
                inst.Inst_Amt = InstAmt;
                inst.Inst_Oe = InstOE;
                inst.Inst_Delete = InstDelete;
                inst.Voc_Id = VocId;
                inst.Usr_Id = UsrId;
                inst.Yr_Id = YrId;
                inst.BrCode = brCode;
            }
            catch (Exception ex)
            {
                inst = new();
            }
            return inst;
        }

        public static Lien GetLienObject(decimal LoanId, double TDAmt, double LienAmt, double LienRecovered, string SecurityDescription, decimal TDId, bool LienClosed, bool LienDelete,
            decimal VocId, decimal UsrId, decimal YrId,string brCode)
        {
            Lien lien = new Lien();
            try
            {
                lien.Lien_Id = 0;
                lien.Loan_Id = LoanId;
                lien.TD_Amount = TDAmt;
                lien.Lien_Amount = LienAmt;
                lien.Lien_Recovered = LienRecovered;
                lien.SecurityDescription = SecurityDescription;
                lien.TD_Id = TDId;
                lien.Lien_Closed = LienClosed;
                lien.Lien_Delete = LienDelete;
                lien.Voc_Id = VocId;
                lien.Usr_Id = UsrId;
                lien.Yr_Id = YrId;
                lien.BrCode = brCode;
            }
            catch (Exception ex)
            {
                lien = new();
            }
            return lien;
        }

        public static Lien_Trn GetLienTrObject(decimal LienId, decimal LoanId, decimal TDId, double TDTrAmt, double DrawingPower, double LienTrAmt, double LienTrRecovered,
            bool closed, bool deleted, decimal VocId, decimal UsrId, decimal YrId,string brCode)
        {
            Lien_Trn lientr = new Lien_Trn();
            try
            {
                lientr.LienTr_Id = 0;
                lientr.LienTr_Id = LienId;
                lientr.Loan_Id = LoanId;
                lientr.TD_Id = TDId;
                lientr.TDTr_Amount = TDTrAmt;
                lientr.DrawingPower = DrawingPower;
                lientr.LienTr_Amount = LienTrAmt;
                lientr.LienTr_Recovered = LienTrRecovered;
                lientr.LienTr_Closed = closed;
                lientr.LienTr_Delete = deleted; ;
                lientr.Voc_Id = VocId;
                lientr.Usr_Id = UsrId;
                lientr.Yr_Id = YrId;
                lientr.BrCode = brCode;
            }
            catch (Exception)
            {
                lientr = new();
            }
            return lientr!;
        }

        public static Loan_Schemes GetLoanSchemesObject(int Scheme_Id, string Scheme_Name, string Is_MemberOrStaff, bool Is_Member,
            bool Is_Staff, int Loan_Type, int Disb_Type, bool IsCompound_Int, int Compound_Prd, int Inst_Type, int Dem_Frequency, int Int_Frequency, int Int_Application, int PI_Application,
            int IOD_Application, int Adv_Prl_Application, int PILed_Id, int IODLed_Id, int IntLed_Id, int PrlLed_Id, int ReimPILed_Id, int ReimIODLed_Id, int ReimIntLed_Id, int ReimPrlLed_id,
            double MaximumLoanAmount, int MaximumPrincipalPeriod, int MaximumInterestPeriod, bool Scheme_Delete, int usrId, int yrId, bool IsDeductBalances, bool IntFromTemplate,
            int StaffLoan_Int_Type, bool IsStaffAdvance, string LoanNoStartWith, int AdoptLoanEligibility, int DeductPreviousLoans, int MatchShareCapital, int AdoptLoanLimit,
            int IssurityMemberRequired,string brCode)
        {
            Loan_Schemes scheme = new Loan_Schemes();
            try
            {
                scheme.Scheme_Id = Scheme_Id;
                scheme.Scheme_Name = Scheme_Name;
                scheme.Is_MemberOrStaff = Is_MemberOrStaff;
                scheme.Is_Member = Is_Member;
                scheme.Is_Staff = Is_Staff;
                scheme.Loan_Type = Loan_Type;
                scheme.Disb_Type = Disb_Type;
                scheme.IsCompound_Int = IsCompound_Int;
                scheme.Compound_Prd = Compound_Prd;
                scheme.Inst_Type = Inst_Type;
                scheme.Dem_Frequency = Dem_Frequency;
                scheme.Int_Frequency = Int_Frequency;
                scheme.Int_Application = Int_Application;
                scheme.PI_Application = PI_Application;
                scheme.IOD_Application = IOD_Application;
                scheme.Adv_Prl_Application = Adv_Prl_Application;
                scheme.PILed_Id = PILed_Id;
                scheme.IODLed_Id = IODLed_Id;
                scheme.IntLed_Id = IntLed_Id;
                scheme.PrlLed_Id = PrlLed_Id;
                scheme.ReimPILed_Id = ReimPILed_Id;
                scheme.ReimIODLed_Id = ReimIODLed_Id;
                scheme.ReimIntLed_Id = ReimIntLed_Id;
                scheme.ReimPrlLed_Id = ReimPrlLed_id;
                scheme.MaximumLoanAmount = MaximumLoanAmount;
                scheme.MaximumPrincipalPeriod = MaximumPrincipalPeriod;
                scheme.MaximumInterestPeriod = MaximumInterestPeriod;
                scheme.Scheme_Delete = Scheme_Delete;
                scheme.Usr_Id = usrId;
                scheme.Yr_Id = yrId;
                scheme.IsDeductBalances = IsDeductBalances;
                scheme.IntFromTemplate = IntFromTemplate;
                scheme.StaffLoan_Int_Type = StaffLoan_Int_Type;
                scheme.IsStaffAdvance = IsStaffAdvance;
                scheme.LoanNoStartWith = LoanNoStartWith;
                scheme.AdoptLoanEligibility = AdoptLoanEligibility;
                scheme.DeductPreviousLoans = DeductPreviousLoans;
                scheme.MatchShareCapital = MatchShareCapital;
                scheme.AdoptLoanLimit = AdoptLoanLimit;
                scheme.IssurityMemberRequired = IssurityMemberRequired;
                scheme.BrCode = brCode;

            }
            catch (Exception )
            {
                scheme = new();
            }
            return scheme;
        }

        public static Loan_Roi_Template GetLoanROITemplateObject(decimal roi_Id, int Scheme_Id, string Agency, DateTime wef, double roi, double pi, bool updt, bool roitempalate_delete, decimal usrId, decimal yrId, string brCode)
        {
            Loan_Roi_Template roiTemplate = new Loan_Roi_Template();
            try
            {
                roiTemplate.Roi_Id = roi_Id;
                roiTemplate.Scheme_Id = Scheme_Id;
                roiTemplate.Agency = Agency;
                roiTemplate.Wef = wef;
                roiTemplate.Roi = roi;
                roiTemplate.Pi = pi;
                roiTemplate.Updt = updt;
                roiTemplate.RoiTemplate_Delete = roitempalate_delete;
                roiTemplate.Usr_Id = usrId;
                roiTemplate.Yr_Id = yrId;
                roiTemplate.BrCode = brCode;
            }
            catch (Exception)
            {
                roiTemplate = new();
            }
            return roiTemplate;
        }

        public static Loan_Members GetLoanMembersObject(decimal loanmem_id, decimal loan_id, decimal mem_id, int mem_slno, int mem_status, int mem_guarator, bool loanmem_delete,
            decimal voc_id, decimal usr_id, decimal yr_id,string brCode)
        {
            Loan_Members mem = new Loan_Members();
            try
            {
                mem.LoanMem_Id = loanmem_id;
                mem.Loan_Id = loan_id;
                mem.Mem_Id = mem_id;
                mem.Mem_SlNo = mem_slno;
                mem.Mem_Status = mem_status;
                mem.Mem_Guarantor = mem_guarator;
                mem.Mem_Delete = loanmem_delete;
                mem.Voc_Id = voc_id;
                mem.Usr_Id = usr_id;
                mem.Yr_Id = yr_id;
                mem.BrCode = brCode;
            }
            catch (Exception)
            {
                mem = new();
            }
            return mem;
        }

        public static Loan_Repayment_Schedule GetLoanRepaymentSchedule(decimal DemId, decimal LoanId, string Status, DateTime DueDate, double PrlDem, double IntDem, double NonODAmt,decimal usrId, decimal yrId, string brCode)
        {
            Loan_Repayment_Schedule repayment = new Loan_Repayment_Schedule();
            try
            {
                repayment.Dem_Id = DemId;
                repayment.Loan_Id = LoanId;
                repayment.Dem_Status = Status;
                repayment.Due_Date = DueDate.Date;
                repayment.Principal_Demand = PrlDem;
                repayment.Interest_Demand = IntDem;
                repayment.Non_OD_Amt = NonODAmt;
                repayment.IsDemandRaised = false;
                repayment.Voc_Id = 0;
                repayment.Usr_Id = usrId;
                repayment.Yr_Id = yrId;
                repayment.BrCode = brCode;
            }
            catch (Exception)
            {
                repayment = new();
            }
            return repayment;
        }
        #endregion

        #region Jewel Loan
        public static JL_Details GetJLDetails(decimal JL_Id, decimal Loan_Id, DateTime JL_DueDate, double RatePerGram, double GrossWeight, double Wastage, double NetWeight,
            double NetValue, double MarketRatePerGram, double EligiblePercentageOnMarketValue, string jewelImagePath, bool JL_oe, bool JL_Delete, decimal voc_id, decimal usr_id, decimal Yr_id,string brCode)
        {
            JL_Details jldetails = new JL_Details();
            try
            {
                jldetails.JL_Id = JL_Id;
                jldetails.Loan_Id = Loan_Id;
                jldetails.JL_DueDate = JL_DueDate;
                jldetails.RatePerGram = RatePerGram;        /// adopted rate per gram ex: 4000
                jldetails.GrossWeight = GrossWeight;
                jldetails.Wastage = Wastage;
                jldetails.NetWeight = NetWeight;
                jldetails.NetValue = NetValue;
                jldetails.JL_Oe = JL_oe;
                jldetails.JL_Delete = JL_Delete;
                jldetails.Voc_Id = voc_id;
                jldetails.Usr_Id = usr_id;
                jldetails.Yr_Id = Yr_id;
                jldetails.JewelsImagePath  = jewelImagePath;
                jldetails.MarketRatePerGram = MarketRatePerGram;
                jldetails.EligiblePercentageOnMarketValue = EligiblePercentageOnMarketValue;
                jldetails.BrCode = brCode;
            }
            catch (Exception)
            {
                jldetails = new();
            }
            return jldetails;
        }

        public static JL_Ornments GetJLOrnments(decimal JLO_Id, decimal Loan_Id, string JLO_Name, int JLO_Nos, double JLO_GWt, double JLO_Wastage, double JLO_NWt,
            double JLO_Value, bool JLO_OE, bool JLO_Delete, decimal Voc_Id, decimal usr_Id, decimal Yr_Id,string brCode)
        {
            JL_Ornments obj = new JL_Ornments();
            try
            {
                obj.JLO_Id = JLO_Id;
                obj.Loan_Id = Loan_Id;
                obj.JLO_Name = JLO_Name;
                obj.JLO_Nos = JLO_Nos;
                obj.JLO_GWt = JLO_GWt;
                obj.JLO_Wastage = JLO_Wastage;
                obj.JLO_NWt = JLO_NWt;
                obj.JLO_Value = JLO_Value;
                obj.JLO_OE = JLO_OE;
                obj.JLO_Delete = JLO_Delete;
                obj.JLO_Delete = JLO_Delete;
                obj.Voc_Id = Voc_Id;
                obj.Usr_Id = usr_Id;
                obj.Yr_Id = Yr_Id;
                obj.BrCode = brCode;
            }
            catch (Exception)
            {
                obj = new();
            }
            return obj;
        }
        #endregion

        #region TermDeposit
        public static TermDeposit_Master GetTermDepositMaster(decimal TDId, string TDNo, int TDSchemeId, decimal MemId, string TDHName, int TDHAge, int ModeOfOperation,
            DateTime AccountOpenDate, DateTime ValueDate, double DepositAmount, int PeriodInMonths, int PeriondInDays, double ROI, DateTime MaturityDate, double MaturityAmount,
            bool IsNomineeProvided, int InterInterestPayableFrequency, double PIRate, bool IsDiscountRate, bool IscompoundInterest, int CompoundFrequency, string Nominee1Name,
            int Nominee1Age, string Nominee1Relation, string Nominee2Name, int Nominee2Age, string Nominee2Relation, bool TDOE, bool TDDelete, bool IsAccountClosed, decimal VocId, decimal UsrId, decimal YrId,
            string status, decimal RenewalTD_Id, string RenewalTD_No,string brCode)
        {
            TermDeposit_Master tdMaster = new();
            try
            {
                tdMaster.TD_Id = 0;
                tdMaster.TD_No = TDNo;
                tdMaster.TDScheme_Id = TDSchemeId;
                tdMaster.Mem_Id = MemId;
                tdMaster.TDH_Name = TDHName;
                tdMaster.TDH_Age = TDHAge;
                tdMaster.ModeOfOperation = ModeOfOperation;
                tdMaster.AccountOpenDate = AccountOpenDate;
                tdMaster.ValueDate = ValueDate;
                tdMaster.DepositAmount = DepositAmount;
                tdMaster.PeriodInMonths = PeriodInMonths;
                tdMaster.PeriodInDays = PeriondInDays;
                tdMaster.RateOfInterest = ROI;
                tdMaster.MaturityDate = MaturityDate;
                tdMaster.MaturityAmount = MaturityAmount;
                tdMaster.LeinAmount = 0;
                tdMaster.IsNomineeProvided = IsNomineeProvided;
                tdMaster.InterestPayableFrequency = InterInterestPayableFrequency;
                tdMaster.PenalRate = PIRate;
                tdMaster.IsDiscountRate = IsDiscountRate;
                tdMaster.IsCompoundInterest = IscompoundInterest;
                tdMaster.CompoundFrequency = CompoundFrequency;
                tdMaster.Nominee1Name = Nominee1Name;
                tdMaster.Nominee1Age = Nominee1Age;
                tdMaster.Nominee1Relationship = Nominee1Relation;
                tdMaster.Nominee2Name = Nominee2Name;
                tdMaster.Nominee2Age = Nominee2Age;
                tdMaster.Nominee2Relationship = Nominee2Relation;
                tdMaster.LienMarked = false;
                tdMaster.Loan_Id = 0;
                tdMaster.TD_OE = TDOE;
                tdMaster.TD_Delete = TDDelete;
                tdMaster.AccountClosed = IsAccountClosed;
                tdMaster.Voc_Id = VocId;
                tdMaster.Usr_Id = UsrId;
                tdMaster.Yr_Id = YrId;
                tdMaster.Status = status;
                tdMaster.RenewalTD_Id = RenewalTD_Id;
                tdMaster.RenewalTD_No = RenewalTD_No;
                tdMaster.BrCode = brCode;
            }
            catch (Exception)
            {
                tdMaster = new();
            }
            return tdMaster;
        }

        public static TermDeposit_Trn GetTermDepositTrn(decimal TDTrnId, DateTime TrnDate, decimal TDId, double DepositDemandAmount, double DepositReceiptAmount, double MaturityAmount,
            double IntCalcAmt, DateTime? IntAppliedDate, double IntPaidAmount, double DepositPaidAmount, double PICalcAmount, DateTime? PICalcDate, double PIReceived, int NoOfInstalments, int InstalmentAmount,
            DateTime? LastInstalmentDate, DateTime? LastPaymentDate, bool TDOE, bool TDDelete, decimal VocId, decimal UsrId, decimal YrId, int SlNo, decimal DemandId,string brCode)
        {
            TermDeposit_Trn TDTrn = new TermDeposit_Trn();
            try
            {
                TDTrn.TDTrn_Id = 0;
                TDTrn.Trn_Date = TrnDate;
                TDTrn.TD_Id = TDId;
                TDTrn.DepositDemandAmount = DepositDemandAmount;
                TDTrn.DepositReceiptAmount = DepositReceiptAmount;
                TDTrn.MaturityAmount = MaturityAmount;
                TDTrn.InterestCalculatedAmount = IntCalcAmt;
                TDTrn.InterestAppliedDate = IntAppliedDate;
                TDTrn.InterestPaidAmount = IntPaidAmount;
                TDTrn.DepositPaidAmount = DepositPaidAmount;
                TDTrn.PenalCalculatedAmount = PICalcAmount;
                TDTrn.PenalAppliedDate = PICalcDate;
                TDTrn.PenalReceivedAmount = PIReceived;
                TDTrn.NoOfInstalments = NoOfInstalments;
                TDTrn.InstalmentAmount = InstalmentAmount;
                TDTrn.LastInstalmentDate = LastInstalmentDate;
                TDTrn.LastPaymentDate = LastPaymentDate;
                TDTrn.TD_OE = TDOE;
                TDTrn.TD_Delete = TDDelete;
                TDTrn.Voc_Id = VocId;
                TDTrn.Usr_Id = UsrId;
                TDTrn.Yr_Id = YrId;
                TDTrn.Trn_SlNo = 0;
                TDTrn.Demand_Id = DemandId;
                TDTrn.BrCode = brCode;
            }
            catch (Exception)
            {
                TDTrn = new();
            }
            return TDTrn;
        }
        public static TermDeposit_Members GetTermDepositMembers(decimal TDMemId, decimal TDId, decimal memId, int memSlNo, bool memDelete, decimal VocId, decimal usrId, decimal yrId,string brCode)
        {
            TermDeposit_Members TDMems = new ();
            try
            {
                TDMems.TDMem_Id = 0;
                TDMems.TD_Id = TDId;
                TDMems.Mem_Id = memId;
                TDMems.Mem_SlNo = memSlNo;
                TDMems.TDMem_Delete = memDelete;
                TDMems.Voc_Id = VocId;
                TDMems.Usr_Id = usrId;
                TDMems.Yr_Id = yrId;
                TDMems.BrCode = brCode;
            }
            catch (Exception)
            {
                TDMems = new();
            }
            return TDMems;
        }

        public static TermDeposit_IntCalc_Calendar GetTermDepositIntCalcCalenderObject(decimal tdcalc_id, DateTime tdcalc_date, DateTime tdnextcalc_date, bool tdcalc_delete, decimal usr_id, decimal yr_id,string brCode)
        {
            TermDeposit_IntCalc_Calendar calender = new ();
            try
            {
                calender.TDCalc_Id = tdcalc_id;
                calender.TDCalc_Date = tdcalc_date;
                calender.TDNextCalc_Date = tdnextcalc_date;
                calender.TDCalc_Delete = tdcalc_delete;
                calender.Usr_Id = usr_id;
                calender.Yr_Id = yr_id;
                calender.BrCode = brCode;
            }
            catch (Exception)
            {
                calender = new();
            }
            return calender;
        }

        public static TermDeposit_Schemes GetTermDepositSchem(int TDScheme_Id, string TDScheme_Name, string TDSchemeType, decimal Led_Id, decimal IntLed_Id, decimal PiLed_Id, bool TDScheme_Delete, decimal usrId, decimal yrId,string brCode)
        {
            TermDeposit_Schemes scheme = new ();
            try
            {
                scheme.TDScheme_Id = TDScheme_Id;
                scheme.TDScheme_Name = TDScheme_Name;
                scheme.MinimumPeriod = 0;
                scheme.MaximumPeriod = 0;
                scheme.PeriodType = "M";
                scheme.TDSchemeType = TDSchemeType;
                scheme.Led_Id = Led_Id;
                scheme.IntLed_Id = IntLed_Id;
                scheme.PiLed_Id = PiLed_Id;
                scheme.TDScheme_Delete = TDScheme_Delete;
                scheme.Usr_Id = usrId;
                scheme.Yr_Id = yrId;
                scheme.BrCode = brCode;
            }
            catch (Exception)
            {
                scheme = new();
            }
            return scheme;
        }
        #endregion

        #region SB Account
        public static SBCA_Master GetSBCAMasterObject(decimal AccId, int SchemeId, decimal MemId, string AccNo, int Acc_Status, bool AccOE, bool AccDelete, decimal VocId, decimal UsrId, decimal YrId, string brCode)
        {
            SBCA_Master sbAcc = new();
            try
            {
                sbAcc.Acc_Id = AccId;
                sbAcc.Scheme_Id = SchemeId;
                sbAcc.Mem_Id = MemId;
                sbAcc.Acc_No = AccNo;
                sbAcc.Acc_Status = Acc_Status;
                sbAcc.Acc_OE = AccOE;
                sbAcc.Acc_Delete = AccDelete;
                sbAcc.Voc_Id = VocId;
                sbAcc.Usr_Id = UsrId;
                sbAcc.Yr_Id = YrId;
                sbAcc.BrCode = brCode;
            }
            catch (Exception)
            {
                sbAcc = new();
            }
            return sbAcc;
        }
        public static SBCA_Schemes GetSBCAScheme(int schemeId, string schemeName, decimal LedId, decimal IntLedId, int period,string brCode)
        {
            SBCA_Schemes scheme = new SBCA_Schemes();
            try
            {
                scheme.Scheme_Id = schemeId;
                scheme.SBCA_Name = schemeName;
                scheme.SBCA_Led_Id = LedId;
                scheme.SBCA_Int_Led_Id = IntLedId;
                scheme.InterestCalcPeriod = period;
                scheme.SBCA_Delete = false;
                scheme.BrCode = brCode;
            }
            catch (Exception)
            {
                scheme = new();
            }
            return scheme;
        }
        #endregion 
    }
}
