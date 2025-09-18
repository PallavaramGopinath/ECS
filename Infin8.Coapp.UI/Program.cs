using Infin8.Coapp.UI.Client.Pages;
using Infin8.Coapp.UI.Components;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Repository;
using Infin8.Coapp.Utility;
using Microsoft.EntityFrameworkCore;
using Infin8.Coapp.ReportServices.Interface;
using Infin8.Coapp.ReportServices.Concrete;
var builder = WebApplication.CreateBuilder(args);

/// server client time out end
StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();
builder.Services.AddControllers();

//builder.Services.AddScoped(http => new HttpClient
//{
//    BaseAddress = new Uri(builder.Configuration.GetSection("BaseUri").Value!),
//});


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();


builder.Services.AddScoped(typeof(DbContext), typeof(CSISContext));

//builder.Services.AddDbContext<CSISContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("CSISDatabase")));

builder.Services.AddDbContext<CSISContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("CSISDatabase")
    //npgsqlOptions =>
    //{
    //    npgsqlOptions.CommandTimeout(300); // 5 minutes
    //    npgsqlOptions.EnableRetryOnFailure(
    //        maxRetryCount: 3,
    //        maxRetryDelay: TimeSpan.FromSeconds(30),
    //        errorCodesToAdd: null);
    //}

    ));

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
builder.Services.AddHttpContextAccessor();

#region State Services
builder.Services.AddSingleton<TransactionStateService>();
builder.Services.AddSingleton<AppState>();
#endregion 

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IDashboardMemberHandler, DashboardMemberHandler>();
builder.Services.AddScoped<IGeneralHandler, GeneralHandler>();


#region accounts
builder.Services.AddScoped<IAccountsHandler, AccountsHandler>();
builder.Services.AddScoped<IFinVoucherTrnHandler, FinVoucherTrnHandler>();
#endregion 

#region Loan
builder.Services.AddScoped<ILoanSchemeHandler, LoanSchemeHandler>();
builder.Services.AddScoped<ILoanMasterHandler, LoanMasterHandler>();
builder.Services.AddScoped<ILoanTrnHandler, LoanTrnHandler>();
builder.Services.AddScoped<ILoanDisburementHandler, LoanDisburementHandler>();
builder.Services.AddScoped<ILoanInstalmentHandler, LoanInstalmentHandler>();
builder.Services.AddScoped<ILoanROIHandler, LoanROIHandler>();
builder.Services.AddScoped<ILoanROITemplateHandler, LoanROITemplateHandler>();
builder.Services.AddScoped<ILoanMemberHandler, LoanMemberHandler>();
builder.Services.AddScoped<IJLDetailsHandler, JLDetailsHandler>();
builder.Services.AddScoped<IJLOrnmentHandler, JLOrnmentHandler>();
builder.Services.AddScoped<IJLEligibleHandler, JLEligibleHandler>();
builder.Services.AddScoped<IJLMaximimumLimitHandler, JLMaximimumLimitHandler>();
builder.Services.AddScoped<IJLDailyMarketRateHandler, JLDailyMarketRateHandler>();
builder.Services.AddScoped<ILienHandler, LienHandler>();
builder.Services.AddScoped<ILienTrnHandler, LienTrnHandler>();
#endregion 

#region Member
builder.Services.AddScoped<ICRMHandler, CRMHandler>();

builder.Services.AddScoped<IMemTrnHandler, MemTrnHandler>();
#endregion 

#region Employee
builder.Services.AddScoped<IEmpMasterHandler, EmpMasterHandler>();
#endregion 
#region Reference
builder.Services.AddScoped<IReferenceHandler, ReferenceHandler>();
builder.Services.AddScoped<IAreaMasterHandler, AreaMasterHandler>();
builder.Services.AddScoped<IBankMasterHandler, BankMasterHandler>();
builder.Services.AddScoped<IReferenceConstituencyHandler, ReferenceConstituencyHandler>();
#endregion 

#region Reports
builder.Services.AddScoped<IReportsHandler, ReportsHandler>();
builder.Services.AddScoped<IReportsTermDepositsHandler, ReportsTermDepositsHandler>();
builder.Services.AddScoped<IReportsJewelLoanHandler, ReportsJewelLoanHandler>();
builder.Services.AddScoped<IReportsMemberHandler, ReportsMemberHandler>();
builder.Services.AddScoped<IReportsLoanHandler, ReportsLoanHandler>();
builder.Services.AddScoped<IReportsFinalAccountsHandler, ReportsFinalAccountsHandler>();
builder.Services.AddScoped<IReportsAccountHandler, ReportsAccountHandler>();
builder.Services.AddScoped<IReportsEmployeeHandler, ReportsEmployeeHandler>();
#endregion 

#region TermDeposit
builder.Services.AddScoped<ITermDepositMasterHandler, TermDepositMasterHandler>();
builder.Services.AddScoped<ITermDepositTrnHandler, TermDepositTrnHandler>();
builder.Services.AddScoped<ITermDepositMemberHandler, TermDepositMemberHandler>();
builder.Services.AddScoped<ITermDepositSchemeHandler, TermDepositSchemeHandler>();
builder.Services.AddScoped<ITermDepositLoanEligibleTemplateHandler, TermDepositLoanEligibleTemplateHandler>();
builder.Services.AddScoped<ITermDepositROITemplateHandler, TermDepositROITemplateHandler>();
builder.Services.AddScoped<ITermDepositIntCalcCalendarHandler, TermDepositIntCalcCalendarHandler>();
builder.Services.AddScoped<ITermDepositFCTemplateHandler, TermDepositFCTemplateHandler>();
#endregion 

#region Pay
builder.Services.AddScoped<IPayInitHandler, PayInitHandler>();
builder.Services.AddScoped<IPaySlipHandler, PaySlipHandler>();
builder.Services.AddScoped<IPayPFTemplateHandler, PayPFTemplateHandler>();
#endregion 

builder.Services.AddScoped<INewAccountNoHandler, NewAccountNoHandler>();

builder.Services.AddScoped<IVerifyHandler, VerifyHandler>();
//builder.Services.AddScoped<IVerifyRepository, VerifyRepository>();

builder.Services.AddScoped<IUtilityHandler, UtilityHandler>();
builder.Services.AddScoped<ILoanSchemeHandler, LoanSchemeHandler>();

builder.Services.AddScoped<IUserHandler, UserHandler>();
builder.Services.AddScoped<IJwtService, JwtService>();

builder.Services.AddScoped<ITransactionsHandler, TransactionsHandler>();

#region staging
builder.Services.AddScoped<IStagingMasterHandler, StagingMasterHandler>();
builder.Services.AddScoped<IStagingDetailsHandler, StagingDetailsHandler>();
#endregion 

#region SBAccount
builder.Services.AddScoped<ISBCAMasterHandler, SBCAMasterHandler>();
builder.Services.AddScoped<ISBCASchemesHandler, SBCASchemesHandler>();
#endregion 

#region reports
builder.Services.AddScoped<IReportsHandler, ReportsHandler>();
builder.Services.AddScoped<IReportsAudit, ReportsAudit>();
#endregion 


#region Report Servces
//builder.Services.AddScoped<IInstant_Reports, Instant_Reports>();
#endregion 

#region Menu Services
builder.Services.AddScoped<IMenuMainHandler, MenuMainHandler>();
#endregion 

//builder.Services.AddHttpClient().ConfigurePrimaryHttpMessageHandler(() =>
//{
//    var handler = new HttpClientHandler();
//    handler.ServerCertificateCustomValidationCallback =
//        (message, cert, chain, errors) => true;
//    return handler;
//});

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

/// Use CORS policy
app.UseCors("AllowBlazorFrontend");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Infin8.Coapp.UI.Client._Imports).Assembly);

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
// Enable static file serving from wwwroot
app.UseStaticFiles();

app.Run();
