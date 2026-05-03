
using Infin8.Coapp.Repository.Interface;
using Microsoft.AspNetCore.Connections.Abstractions;
using System.Net;

namespace Infin8.Coapp.Repository
{
    public interface IUnitOfWork : IDisposable
    {

        #region Accounts
        IAccountsRepository Accounts { get; }
        #endregion 

        ICalendarRepository Calendars { get; }
        IEmpJobHistoryRepository EmployeeJobHistory { get; }
        IEmpMasterRepository EmployeeMaster { get; }
        IEmpPFRepository EmployeePF { get; }
        IEmpPfCalendarRepository EmployeePFCalendar { get; }
        IEmpQualificationRepository EmployeeQualification { get; }
        IGeneralRepository General { get; }
        ICRMRepository Members { get; }
        IReferenceRepository References { get; }
        IVerifyRepository Verify { get; }
        INewAccountNoRepository NewAccountNo { get; }
        IBankMasterRepository BankMaster { get; }
        IDistrictMasterRepository District { get; }
        IAreaMasterRepository Area { get; }
        IFinLedgerRepository FinLedger { get; }
        IFinLedgerGroupRepository FinLedgerGroup { get; }
        IFinLedgerSubGroupRepository FinLedgerSubGroup { get; }
        IFinLedgerFnlRepository FinLedgerFnl { get; }
        IFinLedgerTrnRepository FinLedgerTrn { get; }
        IFinVoucherRepository FinVoucher { get; }
        IFinVoucherBankRepository FinVoucherBank { get; }
        IFinVoucherBankTrnRepository FinVoucherBankTrn { get; }
        IFinVoucherTrnRepository FinVoucherTrn { get; }
        IFinYearMasterRepository FinYearMaster { get; }
        ILoanSchemeRepository LoanScheme { get; }
        ILoanMasterRepository LoanMaster { get; }
        ILoanDisbursementRepository LoanDisbursement { get; }
        ILoanInstalmentRepository LoanInstalment { get; }
        ILoanROIRepository LoanROI { get; }
        ILoanROITemplateRepository LoanROITemplate { get; }
        ILoanMemberRepository LoanMember { get; }
        ILoanSchemeGroupRepository LoanSchemeGroup { get; }
        ILoanRepaymentScheduleRepository LoanRepaymentSchedule { get; }
        ILoanSanctionRepository LoanSanction { get; }
        ILoanSanctionTrnRepository LoanSanctionTrn { get; }
        ILoanTrnRepository LoanTrn { get; }
        IJLDailyMarketRateRepository JLDailyMarketRate { get; }
        IJLDetailsRepository JLDetails { get; }
        IJLEligibleRepository JLLoanEligible { get; }
        IJLMaximumLimitRepository JLMaximumLimit { get; }
        IJLOrnmentRepository JLOrnment { get; }
        ILienRepository Lien { get; }
        ILienTrnRepository LienTrn { get; }
        IDashboardMemberRepository DashboardMember { get; }

        IMapBanksRepository MapBanks { get; }
        IMapGeneralRepository MapGeneral { get; }   
        IMapSuspenseAccountsRepository MapSuspenseAccounts { get; }
        IMemAddressRepository MemAddress { get; }
        IMemDemandMasterRepository MemDemandMaster { get; }
        IMemPassbookRepository MemPassbook { get; }
        IMemPayableRepository MemPayable { get; }
        IMemPayableMasterRepository MemPayableMaster { get; }
        IMemPayableNotNEFTRepository MemPayableNotNEFT { get; }
        IMemTrnRepository MemTrn { get; }

        #region Pay
        IPayAllDedRepository PayAllDed { get; }
        IPayAllDedMasterRepository PayAllDedMaster { get; }
        IPayAttanceRepository PayAttance { get; }
        IPayBasicPayRepository PayBasicPay { get; }
        IPayDATemplateRepository PayDATemplate { get; }
        IPayGenInfoRepository PayGenInfo { get; }
        IPayInitRepository PayInit { get; }
        IPayLeaveEligibleRepository PayLeaveEligible { get; }
        IPayPFRoiTemplateRepository PayPFRoiTemplate { get; }
        IPayPFTemplateRepository PayPFTemplate { get; }
        IPayRetirementRepository PayRetirement { get; }
        IPaySlipRepository PaySlip { get; }
        IPaySlipLoanTrnRepository PaySlipLoanTrn { get; }
        IPaySlipTrnRepository PaySlipTrn { get; }
        IPayTemplateRepository PayTemplate { get; }
        IPayVPFRepository PayVPF { get; }
        IPayComponentRepository PayComponent { get; }
        IPayComponentAssignmentsRepository PayComponentAssignments { get; }
        IPayGenInfoRepository PayGenInfoRepository { get; }
        #endregion 

        IReferenceConstituencyRepository ReferenceConstituency { get; }
        ISBCAMasterRepository SBCAMaster { get; }
        ISBCASchemesRepository SBCASchemes { get; }

        #region termdeposit
        ITermDepositFCTemplateRepository TermDepositFCTemplate { get; }
        ITermDepositIntCalcCalendarRepository TermDepositIntCalcCalendar { get; }
        ITermDepositLoanEligibleTemplateRepository TermDepositLoanEligibleTemplate { get; } 
        ITermDepositROITemplateRepository TermDepositROITemplate { get; }
        ITermDepositMasterRepository TermDepositMaster { get; }
        ITermDepositMemberRepository TermDepositMember { get; }
        ITermDepositSchemeRepository TermDepositScheme { get; }
        ITermDepositTrnRepository TermDepositTrn { get; }
        #endregion 

        IRefreshTokenRepository RefreshTokenRepository { get; }

        IUserRepository UserRepository { get; }

        #region reports repository
        IReportsRepository ReportsMaster { get; }
        IReportsJewelLoanRepository ReportsJewelLoan { get; }
        IReportsMemberRepository ReportsMember { get; }
        IReportsLoanRepository ReportsLoan { get; }
        IReportsAccountRepository ReportsAccount { get; }
        IReportsTermDepositsRepository ReportsTermDeposits { get; }
        IReportsFinalAccountsRepository ReportsFinalAccounts { get; }
        IReportsEmployeeRepository ReportsEmployee { get; }
        IReportsGBRepository ReportsGB { get; }
        #endregion

        #region staging
        IStagingMasterRepository StagingMaster { get; }
        IStagingDetailsRepository StagingDetails { get; }
        IStagingHistoryRepository StagingHistory { get; }
        IStagingBalanceRepository StagingBalance { get; }
        #endregion

        #region Menu
        IMenuMainRepository MenuMain { get; }
        #endregion

        #region Locker
        ILockerSizeMasterRepository LockerSizeMaster { get; }
        ILockerStatusMasterRepository LockerStatusMaster { get; }
        ILockerSettingsRepository LockerSettings { get; }
        ILockerRentTemplateRepository LockerRentTemplate { get; }
        ILockersRepository Lockers { get; }
        ILockerAllotmentsRepository LockerAllotments { get; }
        ILockerRentAdjustmentsRepository LockerRentAdjustments { get; }
        ILockerClosuresRepository LockerClosures { get; }
        #endregion

        ITransactionsRepository TransactionsRepository { get; }
        IMaxId MaxId { get; }
        int Complete();
        Task<int> CompleteAsync();
        void BeginTransaction();
        void RollBack();
        void CommitTransaction();
    }
}

