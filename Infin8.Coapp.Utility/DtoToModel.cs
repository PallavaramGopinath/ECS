using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Utility
{
    public static class DtoToModel
    {
        /// Loan
        #region Loan
        public static Loan_Master GetLoanMasterObject(int LoanSchemeId, string LoanNo, int MemId,  int AppId, string AppNo, DateTime? LedDate, string ResNo,
            DateTime? ResDate, double SanAmt, DateTime SanDate, int LoanType, int PurId, int subGrpId, int GrpId, int AgsId, int PrlPrd, int IntPrd,
            DateTime FirstIntDueDate, DateTime? FirstPrlDueDate, int InstDate, string ReimLNo, string ReimDVNo, string MortSlNo, DateTime? MortExeDate, DateTime? MortRegDate,
            int MortSRO, double ROI, double PI, DateTime? DisbDate, double InstAmt, bool LoanOE, bool LoanDelete, bool IsAccountClosed, int VocId, int UsrId,
            int YrId, double SecurityFaceValue, double DrawingPower, out string errorMessage, int LoanId = 0, double IntDueAmt = 0, int SecurityType = 0)
        {
            errorMessage = "";
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
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                lnMaster = null;

            }
            return lnMaster;
        }

        public static Loan_Trn GetLoanTrnObject(int LoanId, int DemandId, string Status, DateTime TrnDate, DateTime? DueDate, DateTime? DisbDate, double DisbAmt, double PrlSched,
            double PrlDem, double PICalcAmt, DateTime? PICalcDate, double IODCalcAmt, DateTime? IODCalcDate, double IntCalcAmt, DateTime? IntCalcDate, double PICollAmt,
            double IODCollAmt, double IntCollAmt, double PrlCollAmt, double PrlReimSchedule, double PrlReimDemand, double PIReimCalcAmt, DateTime? PIReimCalcDate, double IODReimCalcAmt,
            DateTime? IODReimCalcDate, double IntReimCalcAmt, DateTime? IntReimCalcDate, double SOCROI, double SOCPI, double ReimROI, double ReimPI, bool TrnOE, bool TrnDelete,
            int VocId, int UsrId, int YrId, int TrnSlNo, bool ExpiredDeletion, double PrlOS, double PrlOD, double IntBal, double PIBal, double IODBal, out string errorMessage)
        {
            errorMessage = "";
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
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                lnTrn = null;
            }
            return lnTrn!;
        }

        public static Loan_Disb GetLoanDisbursementObject(int LoanId, DateTime DisbDate, int SocDisbNo, double SocDisbAmt, DateTime? SocDisbDate, DateTime? SocDisbChequeDate,
            string SocDisbChequeNo, DateTime? SocEncashDate, double ReimAmt, DateTime? ReimDate, string ReimChequeNo, DateTime? ReimChequeDate, bool DisbOE, int DisbSlNo, bool isFinalDisbursement,
            bool disbDelete, int VocId, int UsrId, int Yrid, out string errorMessage)
        {
            errorMessage = "";
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
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                lnDisb = null;

            }
            return lnDisb;
        }

        public static Loan_Roi GetLoanROIObject(int LoanId, string Agency, DateTime Wef, double ROI, double PI, int Prd, double InstalAmt, bool roiDelete, bool roiOE, int VocId,
            int UsrId, int YrId, out string errorMessage)
        {
            errorMessage = "";
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
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                lnRoi = null;
            }
            return lnRoi!;
        }

        public static Loan_Inst GetLoanInstalmentObject(int LoanId, DateTime InstWef, double InstAmt, bool InstOE, bool InstDelete, int VocId, int UsrId, int YrId, out string errorMessage)
        {
            errorMessage = "";
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
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
            return inst;
        }

        public static Lien GetLienObject(int LoanId, double TDAmt, double LienAmt, double LienRecovered, string SecurityDescription, int TDId, bool LienClosed, bool LienDelete,
            int VocId, int UsrId, int YrId, out string errorMessage)
        {
            errorMessage = "";
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
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                lien = null;
            }
            return lien;
        }

        public static Lien_Trn GetLienTrObject(int LienId, int LoanId, int TDId, double TDTrAmt, double DrawingPower, double LienTrAmt, double LienTrRecovered,
            bool closed, bool deleted, int VocId, int UsrId, int YrId, out string errorMessage)
        {
            errorMessage = "";
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
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                lientr = null;
            }
            return lientr!;
        }

        public static Loan_Schemes GetLoanSchemesObject(int Scheme_Id,  string Scheme_Name, string Is_MemberOrStaff, bool Is_Member,
            bool Is_Staff, int Loan_Type, int Disb_Type, bool IsCompound_Int, int Compound_Prd, int Inst_Type, int Dem_Frequency, int Int_Frequency, int Int_Application, int PI_Application,
            int IOD_Application, int Adv_Prl_Application, int PILed_Id, int IODLed_Id, int IntLed_Id, int PrlLed_Id, int ReimPILed_Id, int ReimIODLed_Id, int ReimIntLed_Id, int ReimPrlLed_id,
            double MaximumLoanAmount, int MaximumPrincipalPeriod, int MaximumInterestPeriod, bool Scheme_Delete, int usrId, int yrId, bool IsDeductBalances, bool IntFromTemplate,
            int StaffLoan_Int_Type, bool IsStaffAdvance, string LoanNoStartWith, int AdoptLoanEligibility, int DeductPreviousLoans, int MatchShareCapital, int AdoptLoanLimit,
            int IssurityMemberRequired, out string errorMessage)
        {
            errorMessage = "";
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

            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
            return scheme;
        }

        public static Loan_Roi_Template GetLoanROITemplateObject(int roi_Id, int Scheme_Id, string Agency, DateTime wef, double roi, double pi, bool updt, bool roitempalate_delete, int usrId, int yrId, out string errorMessage)
        {
            Loan_Roi_Template roiTemplate = new Loan_Roi_Template();
            errorMessage = "";
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
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
            return roiTemplate;
        }

        public static Loan_Members GetLoanMembersObject(int loanmem_id, int loan_id, int mem_id, int mem_slno, int mem_status, int mem_guarator, bool loanmem_delete,
            int voc_id, int usr_id, int yr_id, out string errorMessage)
        {
            Loan_Members mem = new Loan_Members();
            errorMessage = "";
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
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
            return mem;
        }

        public static Loan_Repayment_Schedule GetLoanRepaymentSchedule(int DemId, int LoanId, string Status, DateTime DueDate, double PrlDem, double IntDem, double NonODAmt, out string errorMessage)
        {
            errorMessage = "";
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
                repayment.Usr_Id = 11001;
                repayment.Yr_Id = 11001;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
            return repayment;
        }
        #endregion
    }
}
