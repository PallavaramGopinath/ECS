using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Infin8.Coapp.Reporting;
using Infin8.Coapp.ReportServices.Concrete;
using Infin8.Coapp.ReportServices.Interface;
using Infin8.Coapp.Repository;
using Infin8.Coapp.Repository.Entities;
using Infin8.Coapp.UI.Client.Pages;
using Infin8.Coapp.UI.Client.Providers;
using Infin8.Coapp.UI.Components;
using Infin8.Coapp.Utility;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
var builder = WebApplication.CreateBuilder(args);

/// server client time out end
StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();
builder.Services.AddControllers();

builder.Services.AddAntiforgery();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

var jwtSettings = builder.Configuration.GetSection("Jwt");

// Ensure we don't pass a possibly-null string to Uri. Read issuer once and validate when not in Development.
var issuer = jwtSettings["Issuer"];
var baseAddressString = builder.Environment.IsDevelopment()
    ? "https://localhost:7073/"
    : issuer ?? throw new InvalidOperationException("Configuration value 'Jwt:Issuer' is missing or null.");

builder.Services.AddScoped(sp => new HttpClient(new HttpClientHandler())
{
    BaseAddress = new Uri(baseAddressString)
});

builder.Services.AddScoped(typeof(DbContext), typeof(CSISContext));

//builder.Services.AddDbContext<CSISContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("CSISDatabase")));

builder.Services.AddDbContext<CSISContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("CSISDatabase")

    ));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", builder =>
    {
        builder.WithOrigins(jwtSettings["Issuer"])
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});


// JWT
builder.Services
    .AddAuthentication("CookiesJwt")
    .AddJwtBearer("CookiesJwt", options =>
    {
        //options.Audience = "";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["Key"]!)),
        };

        // Read token from cookie
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Cookies["auth-token"];
                return Task.CompletedTask;
            },
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
builder.Services.AddHttpContextAccessor();

#region State Services
builder.Services.AddSingleton<TransactionStateService>();
builder.Services.AddSingleton<AppState>();
#endregion 

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IDashboardMemberHandler, DashboardMemberHandler>();
builder.Services.AddScoped<IGeneralHandler, GeneralHandler>();
builder.Services.AddScoped<IAuthenticationHandler, JwtAuthenticationHandler>();

#region accounts
builder.Services.AddScoped<IAccountsHandler, AccountsHandler>();
builder.Services.AddScoped<IFinVoucherTrnHandler, FinVoucherTrnHandler>();
#endregion 

#region Fin Ledger
builder.Services.AddScoped<IFinLedgerHandler, FinLedgerHandler>();
builder.Services.AddScoped<IFinLedgerFnlHandler, FinLedgerFnlHandler>();
builder.Services.AddScoped<IFinLedgerGroupHandler , FinLedgerGroupHandler>();
builder.Services.AddScoped<IFinLedgerTrnHandler, FinLedgerTrnHandler>();
#endregion 
#region Loan
builder.Services.AddScoped<ILoanSchemeHandler, LoanSchemeHandler>();
builder.Services.AddScoped<ILoanSchemeGroupHandler, LoanSchemeGroupHandler>();
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
builder.Services.AddScoped<ICreateReportsHandler, CreateReportsHandler>();
builder.Services.AddScoped<IReportsHandler, ReportsHandler>();
builder.Services.AddScoped<IReportsTermDepositsHandler, ReportsTermDepositsHandler>();
builder.Services.AddScoped<IReportsJewelLoanHandler, ReportsJewelLoanHandler>();
builder.Services.AddScoped<IReportsMemberHandler, ReportsMemberHandler>();
builder.Services.AddScoped<IReportsLoanHandler, ReportsLoanHandler>();
builder.Services.AddScoped<IReportsFinalAccountsHandler, ReportsFinalAccountsHandler>();
builder.Services.AddScoped<IReportsAccountHandler, ReportsAccountHandler>();
builder.Services.AddScoped<IReportsEmployeeHandler, ReportsEmployeeHandler>();
//builder.Services.AddScoped<IReportsAudit, ReportsAudit>();
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

builder.Services.AddScoped<IJwtService, JwtService>();

builder.Services.AddScoped<ITransactionsHandler, TransactionsHandler>();

#region staging
builder.Services.AddScoped<IStagingMasterHandler, StagingMasterHandler>();
builder.Services.AddScoped<IStagingDetailsHandler, StagingDetailsHandler>();
builder.Services.AddScoped<IStagingHistoryHandler, StagingHistoryHandler>();
builder.Services.AddScoped<IStagingBalanceHandler, StagingBalanceHandler>();
#endregion 

#region SBAccount
builder.Services.AddScoped<ISBCAMasterHandler, SBCAMasterHandler>();
builder.Services.AddScoped<ISBCASchemesHandler, SBCASchemesHandler>();
#endregion 

#region Report Servces
//builder.Services.AddScoped<IInstant_Reports, Instant_Reports>();
#endregion 

#region Menu Services
builder.Services.AddScoped<IMenuMainHandler, MenuMainHandler>();
#endregion 

#region user
builder.Services.AddScoped <IUserHandler, UserHandler>();
#endregion 

#region DayProcess
builder.Services.AddScoped<ICalendarHandler, CalendarHandler>();
#endregion 

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

app.UseCors("AllowBlazor");

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

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
// Enable static file serving from wwwroot
app.UseStaticFiles();

app.Run();
