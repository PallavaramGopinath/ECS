using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using Infin8.Coapp.Repository.Concrete;
using Infin8.Coapp.Repository.Interface;
using System.Xml.Linq;

namespace Infin8.Coapp.Repository
{
    public class UnitOfWork( CSISContext context) : IUnitOfWork
    {
        private bool _disposed;

        private readonly CSISContext _context = context;

        

        #region Accounts
        private AccountsRepository? _accountsRepository;
        #endregion 

        private CalendarRepository? _calendarRepository;

        private EmpJobHistoryRepository? _empJobHistoryRepository;
        private EmpMasterRepository? _empMasterRepository;
        private EmpPFRepository? _empPFRepository;
        private EmpPfCalendarRepository? _empPfCalendarRepository;
        private EmpQualificationRepository? _empQualificationRepository;

        private GeneralRepository? _generalRepository;
        private CRMRepository? _membersRepository;
        private ReferenceRepository? _referenceRepository;
        private VerifyRepository? _verifyRepository;
        private NewAccountNoRepository? _newAccouontNoRepository;
        private BankMasterRepository? _bankMasterRepository;
        private DistrictMasterRepository? _districtRepository;
        private AreaMasterRepository? _arearRepository;
        private FinLedgerRepository? _finLedgerRepository;
        private FinLedgerGroupRepository? _finLedgerGroupRepository;
        private FinLedgerSubGroupRepository? _finLedgerSubGroupRepository;
        private FinLedgerFnlRepository? _finLedgerFnlRepository;
        private IFinLedgerTrnRepository? _finLedgerTrnRepository;
        private IFinVoucherRepository? _finVoucherRepository;
        private IFinVoucherBankRepository? _finVoucherBankRepository;
        private IFinVoucherBankTrnRepository? _finVoucherBankTrnRepository;
        private IFinVoucherTrnRepository? _finVoucherTrnRepository;
        private IFinYearMasterRepository? _finYearMasterRepository;

        private LoanSchemeRepository? _loanSchemeRepository;
        private LoanMasterRepository? _loanMasterRepository;
        private LoanDisbursementRepository? _loanDisbursementRepository;
        private LoanInstalmentRepository? _loanInstalmentRepository;
        private LoanROIRepository? _loanROIRepository;  
        private LoanROITemplateRepository? _loanROITemplateRepository;
        private LoanMemberRepository? _loanMemberRepository;
        private LoanSchemeGroupRepository? _loanSchemeGroupRepository;
        private LoanRepaymentScheduleRepository? _loanRepaymentScheduleRepository;
        private LoanSanctionRepository? _loanSanctionRepository;
        private LoanSanctionTrnRepository? _loanSanctionTrnRepository;
        private LoanTrnRepository? _loanTrnRepository;
        private JLDailyMarketRateRepository? _jldailyMarketRateRepository;
        private JLDetailsRepository? _jldDetailsRepository;
        private JLEligibleRepository? _jLLoanEligibleRepository;
        private JlMaximumLimitRepository? _jlMaximumLimitRepository;
        private JLOrnmentRepository? _jlLornmentRepository;
        private LienRepository? _lienRepository;
        private LienTrnRepository? _lienTrnRepository;
        private DashboardMemberRepository? _dashboardRepository;

        private MapBanksRepository? _mapBanksRepository;
        private MapGeneralRepository? _mapGeneralRepository;
        private MapSuspenseAccountsRepository? _mapSuspenseAccountsRepository;
        private MemAddressRepository? _memAddressRepository;
        private MemDemandMasterRepository? _memDemandMasterRepository;
        private MemPassbookRepository? _memPassbookRepository;
        private MemPayableRepository? _memPayableRepository;
        private MemPayableMasterRepository? _memPayableMasterRepository;
        private MemPayableNotNEFTRepository? _memPayableNotNEFTRepository;
        private MemTrnRepository? _memTrnRepository;

        private PayAllDedRepository? _payAllDedRepository;
        private PayAllDedMasterRepository? _payAllDedMasterRepository;
        private PayAttanceRepository? _payAttanceRepository;
        private PayBasicPayRepository? _payBasicPayRepository;
        private PayDATemplateRepository? _payDATemplateRepository;
        private PayGenInfoRepository? _payGenInfoRepository;
        private PayInitRepository? _payInitRepository;
        private PayLeaveEligibleRepository? _payLeaveEligibleRepository;    
        private PayPFRoiTemplateRepository? _payPFRoiTemplateRepository;
        private PayPFTemplateRepository? _payPFTemplateRepository;
        private PayRetirementRepository? _payRetirementRepository;
        private PaySlipRepository? _paySlipRepository;
        private PaySlipLoanTrnRepository? _paySlipLoanTrnRepository;
        private PaySlipTrnRepository? _paySlipTrnRepository;
        private PayTemplateRepository? _payTemplateRepository;
        private PayVPFRepository? _payVPFRepository;

        private ReferenceConstituencyRepository? _referenceConstituencyRepository;

        private SBCAMasterRepository? _sbCAMasterRepository;
        private SBCASchemesRepository? _sbCASchemesRepository;

        #region termdeposit
        private TermDepositFCTemplateRepository? _termDepositFCTemplateRepository;
        private TermDepositIntCalcCalendarRepository? _termDepositIntCalcCalendarRepository;
        private TermDepositLoanEligibleTemplateRepository? _termDepositLoanEligibleTemplateRepository;
        private TermDepositROITemplateRepository? _termDepositROITemplateRepository;
        private TermDepositMasterRepository? _termDepositMasterRepository;
        private ITermDepositMemberRepository? _termDepositMemberRepository;    
        private TermDepositSchemeRepository? _termDepositSchemeRepository;
        private TermDepositTrnRepository? _termDepositTrnRepository;
        #endregion 

        private RefreshTokenRepository? _refreshTokenRepository;

        private UserRepository? _userRepository;

        private MaxId? _maxId;

        #region reports repository
        private ReportsRepository? _reportsRepository;
        private ReportsJewelLoanRepository? _reportsJewelLoanRepository;
        private ReportsMemberRepository? _reportsMemberRepository;
        private ReportsLoanRepository? _reportsLoanRepository;
        private ReportsAccountRepository? _reportsAccountRepository;
        private ReportsTermDepositsRepository? _reportsTermDepositsRepository;
        private ReportsFinalAccountsRepository? _reportsFinalAccountsRepository;
        private ReportsEmployeeRepository? _reportsEmployeeRepository;
        private ReportsGBRepository? _reportsGBRepository;
        #endregion

        #region staging
        private StagingMasterRepository? _stagingMasterRepository;
        private StagingDetailsRepository? _stagingDetailsRepository;
        private StagingHistoryRepository? _stagingHistoryRepository;
        private StagingBalanceRepository? _stagingBalanceRepository;
        #endregion

        #region Menu
        private MenuMainRepository? _menuMainRepository;
        #endregion 
        private TransactionsRepository? _transactionsRepository;

        

        #region Accounts
        public IAccountsRepository Accounts => _accountsRepository ??= new AccountsRepository(_context);
        #endregion

        public ICalendarRepository Calendars => _calendarRepository ??= new CalendarRepository(_context);
        public IEmpJobHistoryRepository EmployeeJobHistory => _empJobHistoryRepository ??= new EmpJobHistoryRepository(_context);    
        public IEmpMasterRepository EmployeeMaster => _empMasterRepository ??= new EmpMasterRepository(_context);
        public IEmpPFRepository EmployeePF => _empPFRepository ??= new EmpPFRepository(_context);   
        public IEmpPfCalendarRepository EmployeePFCalendar => _empPfCalendarRepository ??= new EmpPfCalendarRepository(_context);
        public IEmpQualificationRepository EmployeeQualification => _empQualificationRepository ??= new EmpQualificationRepository(_context);   

        public IGeneralRepository General => _generalRepository ??= new GeneralRepository(_context);
        public ICRMRepository Members => _membersRepository ??= new CRMRepository(_context);
        public IReferenceRepository References => _referenceRepository ??= new ReferenceRepository(_context);
        public IVerifyRepository Verify => _verifyRepository ??= new VerifyRepository(_context);
        public INewAccountNoRepository NewAccountNo => _newAccouontNoRepository ??= new NewAccountNoRepository(_context);
        public IBankMasterRepository BankMaster => _bankMasterRepository ??= new BankMasterRepository(_context);
        public IDistrictMasterRepository District => _districtRepository ??= new DistrictMasterRepository(_context);
        public IAreaMasterRepository Area => _arearRepository ??= new AreaMasterRepository(_context);
        public IFinLedgerRepository FinLedger => _finLedgerRepository ??= new FinLedgerRepository(_context);
        public IFinLedgerGroupRepository FinLedgerGroup => _finLedgerGroupRepository ??= new FinLedgerGroupRepository(_context);
        public IFinLedgerSubGroupRepository FinLedgerSubGroup => _finLedgerSubGroupRepository ??= new FinLedgerSubGroupRepository(_context);    
        public IFinLedgerFnlRepository FinLedgerFnl => _finLedgerFnlRepository ??= new FinLedgerFnlRepository(_context);
        public IFinLedgerTrnRepository FinLedgerTrn => _finLedgerTrnRepository ??= new FinLedgerTrnRepository(_context);
        public IFinVoucherRepository FinVoucher => _finVoucherRepository ??= new FinVoucherRepository(_context);
        public IFinVoucherBankRepository FinVoucherBank => _finVoucherBankRepository ??= new FinVoucherBankRepository(_context);
        public IFinVoucherBankTrnRepository FinVoucherBankTrn => _finVoucherBankTrnRepository ??= new FinVoucherBankTrnRepository(_context);
        public IFinVoucherTrnRepository FinVoucherTrn => _finVoucherTrnRepository ??= new FinVoucherTrnRepository(_context);
        public IFinYearMasterRepository FinYearMaster => _finYearMasterRepository ??= new FinYearMasterRepository(_context);
        public ILoanSchemeRepository LoanScheme => _loanSchemeRepository ??= new LoanSchemeRepository(_context);
        public ILoanMasterRepository LoanMaster => _loanMasterRepository ??= new LoanMasterRepository(_context);
        public ILoanDisbursementRepository LoanDisbursement => _loanDisbursementRepository ??= new LoanDisbursementRepository(_context);
        public ILoanInstalmentRepository LoanInstalment => _loanInstalmentRepository ??= new LoanInstalmentRepository(_context);
        public ILoanROIRepository LoanROI => _loanROIRepository ??= new LoanROIRepository(_context);
        public ILoanROITemplateRepository LoanROITemplate  => _loanROITemplateRepository ??= new LoanROITemplateRepository(_context);
        public ILoanMemberRepository LoanMember =>  _loanMemberRepository ??= new LoanMemberRepository(_context);
        public ILoanSchemeGroupRepository LoanSchemeGroup => _loanSchemeGroupRepository ??= new LoanSchemeGroupRepository(_context);    
        public ILoanRepaymentScheduleRepository LoanRepaymentSchedule => _loanRepaymentScheduleRepository ??= new LoanRepaymentScheduleRepository(_context);
        public ILoanSanctionRepository LoanSanction => _loanSanctionRepository ??= new LoanSanctionRepository(_context);
        public ILoanSanctionTrnRepository LoanSanctionTrn => _loanSanctionTrnRepository ??= new LoanSanctionTrnRepository(_context);
        public ILoanTrnRepository LoanTrn => _loanTrnRepository ??= new LoanTrnRepository(_context);
        public IJLDailyMarketRateRepository JLDailyMarketRate => _jldailyMarketRateRepository ??= new JLDailyMarketRateRepository(_context);    
        public IJLDetailsRepository JLDetails => _jldDetailsRepository ??= new JLDetailsRepository(_context);
        public IJLEligibleRepository JLLoanEligible => _jLLoanEligibleRepository ??= new JLEligibleRepository(_context);
        public IJLMaximumLimitRepository JLMaximumLimit => _jlMaximumLimitRepository ??= new JlMaximumLimitRepository(_context);
        public IJLOrnmentRepository JLOrnment => _jlLornmentRepository ??= new JLOrnmentRepository(_context);
        public ILienRepository Lien => _lienRepository ??= new LienRepository(_context);    
        public ILienTrnRepository LienTrn => _lienTrnRepository ??= new LienTrnRepository(_context);    
        public IDashboardMemberRepository DashboardMember => _dashboardRepository ??= new DashboardMemberRepository(_context);

        public IMapBanksRepository MapBanks => _mapBanksRepository ??= new MapBanksRepository(_context);
        public IMapGeneralRepository MapGeneral => _mapGeneralRepository ??= new MapGeneralRepository(_context);
        public IMapSuspenseAccountsRepository MapSuspenseAccounts => _mapSuspenseAccountsRepository ??= new MapSuspenseAccountsRepository(_context);
        public IMemAddressRepository MemAddress => _memAddressRepository ??= new MemAddressRepository(_context);
        public IMemDemandMasterRepository MemDemandMaster => _memDemandMasterRepository ??= new MemDemandMasterRepository(_context);
        public IMemPassbookRepository MemPassbook => _memPassbookRepository ??= new MemPassbookRepository(_context);
        public IMemPayableRepository MemPayable => _memPayableRepository ??= new MemPayableRepository(_context);
        public IMemPayableMasterRepository MemPayableMaster => _memPayableMasterRepository ??= new MemPayableMasterRepository(_context);
        public IMemPayableNotNEFTRepository MemPayableNotNEFT => _memPayableNotNEFTRepository ??= new MemPayableNotNEFTRepository(_context);
        public IMemTrnRepository MemTrn => _memTrnRepository ??= new MemTrnRepository(_context);
        public IPayAllDedRepository PayAllDed => _payAllDedRepository ??= new PayAllDedRepository(_context);
        public IPayAllDedMasterRepository PayAllDedMaster => _payAllDedMasterRepository ??= new PayAllDedMasterRepository(_context);
        public IPayAttanceRepository PayAttance => _payAttanceRepository ??= new PayAttanceRepository(_context);
        public IPayBasicPayRepository PayBasicPay => _payBasicPayRepository ??= new PayBasicPayRepository(_context);
        public IPayDATemplateRepository PayDATemplate => _payDATemplateRepository ??= new PayDATemplateRepository(_context);
        public IPayGenInfoRepository PayGenInfo => _payGenInfoRepository ??= new PayGenInfoRepository(_context);
        public IPayInitRepository PayInit => _payInitRepository ??= new PayInitRepository(_context);
        public IPayLeaveEligibleRepository PayLeaveEligible => _payLeaveEligibleRepository ??= new PayLeaveEligibleRepository(_context);
        public IPayPFRoiTemplateRepository PayPFRoiTemplate => _payPFRoiTemplateRepository ??= new PayPFRoiTemplateRepository(_context);
        public IPayPFTemplateRepository PayPFTemplate => _payPFTemplateRepository ??= new PayPFTemplateRepository(_context);
        public IPayRetirementRepository PayRetirement => _payRetirementRepository ??= new PayRetirementRepository(_context);
        public IPaySlipRepository PaySlip => _paySlipRepository ??= new PaySlipRepository(_context);
        public IPaySlipLoanTrnRepository PaySlipLoanTrn => _paySlipLoanTrnRepository ??= new PaySlipLoanTrnRepository(_context);
        public IPaySlipTrnRepository PaySlipTrn => _paySlipTrnRepository ??= new PaySlipTrnRepository(_context);
        public IPayTemplateRepository PayTemplate => _payTemplateRepository ??= new PayTemplateRepository(_context);
        public IPayVPFRepository PayVPF => _payVPFRepository ??= new PayVPFRepository(_context);

        public IReferenceConstituencyRepository ReferenceConstituency => _referenceConstituencyRepository ??= new ReferenceConstituencyRepository(_context);

        public ISBCAMasterRepository SBCAMaster => _sbCAMasterRepository ??= new SBCAMasterRepository(_context);
        public ISBCASchemesRepository SBCASchemes => _sbCASchemesRepository ??= new SBCASchemesRepository(_context);

        #region staging
        public IStagingMasterRepository StagingMaster => _stagingMasterRepository ??= new StagingMasterRepository(_context);
        public IStagingDetailsRepository StagingDetails => _stagingDetailsRepository ??= new StagingDetailsRepository(_context);
        public IStagingHistoryRepository StagingHistory => _stagingHistoryRepository ??= new StagingHistoryRepository(_context);
        public IStagingBalanceRepository StagingBalance => _stagingBalanceRepository ??= new StagingBalanceRepository(_context);
        #endregion

        #region Menu
        public IMenuMainRepository MenuMain => _menuMainRepository ??= new MenuMainRepository(_context);
        #endregion

        #region Termdeposit
        public ITermDepositFCTemplateRepository TermDepositFCTemplate => _termDepositFCTemplateRepository ??= new TermDepositFCTemplateRepository(_context);    
        public ITermDepositIntCalcCalendarRepository TermDepositIntCalcCalendar => _termDepositIntCalcCalendarRepository ??= new TermDepositIntCalcCalendarRepository(_context);
        public ITermDepositLoanEligibleTemplateRepository TermDepositLoanEligibleTemplate => _termDepositLoanEligibleTemplateRepository ??= new TermDepositLoanEligibleTemplateRepository(_context);      
        public ITermDepositROITemplateRepository TermDepositROITemplate => _termDepositROITemplateRepository ??new TermDepositROITemplateRepository(_context);
        public ITermDepositMasterRepository TermDepositMaster => _termDepositMasterRepository ??= new TermDepositMasterRepository(_context);  
        public ITermDepositMemberRepository TermDepositMember => _termDepositMemberRepository ??= new TermDepositMemberRepository(_context);
        public ITermDepositSchemeRepository TermDepositScheme => _termDepositSchemeRepository ??= new TermDepositSchemeRepository(_context);  
        public ITermDepositTrnRepository TermDepositTrn => _termDepositTrnRepository ??= new TermDepositTrnRepository(_context);
        #endregion 

        public IRefreshTokenRepository RefreshTokenRepository => _refreshTokenRepository ??= new RefreshTokenRepository(_context);
        
        public IUserRepository UserRepository => _userRepository ??= new UserRepository(_context);
        public IMaxId MaxId => _maxId ??= new MaxId(_context);

        #region reports repository
        public IReportsRepository ReportsMaster => _reportsRepository ??= new ReportsRepository(_context);
        public IReportsJewelLoanRepository ReportsJewelLoan => _reportsJewelLoanRepository ??= new ReportsJewelLoanRepository(_context);
        public IReportsMemberRepository ReportsMember => _reportsMemberRepository ??= new ReportsMemberRepository(_context);
        public IReportsLoanRepository ReportsLoan => _reportsLoanRepository ??= new ReportsLoanRepository(_context);
        public IReportsAccountRepository ReportsAccount => _reportsAccountRepository ??= new ReportsAccountRepository(_context);
        public IReportsTermDepositsRepository ReportsTermDeposits => _reportsTermDepositsRepository ??= new ReportsTermDepositsRepository(_context);
        public IReportsFinalAccountsRepository ReportsFinalAccounts => _reportsFinalAccountsRepository ??= new ReportsFinalAccountsRepository(_context);
        public IReportsEmployeeRepository ReportsEmployee => _reportsEmployeeRepository ??= new ReportsEmployeeRepository(_context);
        public IReportsGBRepository ReportsGB => _reportsGBRepository ??= new ReportsGBRepository(_context);
        #endregion

        public ITransactionsRepository TransactionsRepository => _transactionsRepository ??= new TransactionsRepository(_context);
        public void BeginTransaction()
        {
            try
            {
                _context.Database.BeginTransaction();
            }
            catch (Exception dbEx)
            {
                throw new InvalidOperationException(dbEx.Message, dbEx.InnerException);
            }
        }
        public void CommitTransaction()
        {
            try
            {
                _context.Database.CommitTransaction();
            }
            catch (Exception dbEx)
            {
                throw new InvalidOperationException(dbEx.Message, dbEx.InnerException);
            }
        }
        public int Complete()
        {
            try
            {
                return _context.SaveChanges();
            }
            catch (Exception dbEx)
            {
                throw new InvalidOperationException(dbEx.Message, dbEx.InnerException);
            }
        }
        public async Task<int> CompleteAsync()
        {
            try
            {
                return  await _context.SaveChangesAsync();
            }
            catch (Exception dbEx)
            {
                throw new InvalidOperationException(dbEx.Message, dbEx.InnerException);
            }
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            _disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public void RollBack()
        {
            try
            {
                _context.Database.RollbackTransaction();
            }
            catch (Exception dbEx)
            {
                throw new InvalidOperationException(dbEx.Message, dbEx.InnerException);
            }
        }
    }
}

