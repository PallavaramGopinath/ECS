using Microsoft.EntityFrameworkCore;

using Infin8.Coapp.Repository;
using Infin8.Coapp.BusinessLogic;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Infin8.Coapp.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi.Models;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddControllers();

/// anti forgery token
//builder.Services.AddAntiforgery(options =>
//{
//    options.HeaderName = "X-CSRF-TOKEN";
//    options.Cookie.Name = "CSRF-TOKEN";
//    options.Cookie.SameSite = SameSiteMode.Strict;
//    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
//});
/// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorFrontend",
        builder => builder
            .WithOrigins("https://localhost:7073") // Allow requests from Blazor front end
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// Configure JWT authentication
//var jwtSettings = builder.Configuration.GetSection("Jwt");
//var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? "");
//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//})
//.AddJwtBearer(options =>
//{
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true,
//        ValidIssuer = jwtSettings["Issuer"],
//        ValidAudience = jwtSettings["Audience"],
//        IssuerSigningKey = new SymmetricSecurityKey(key)
//    };
//});

//builder.Services.AddAuthorization(options =>
//{
//    // Define policies for each role
//    options.AddPolicy("MakerOnly", policy => policy.RequireRole(RoleConstants.Maker));
//    options.AddPolicy("CheckerOnly", policy => policy.RequireRole(RoleConstants.Checker));
//    options.AddPolicy("AdminOnly", policy => policy.RequireRole(RoleConstants.Admin));
//    options.AddPolicy("ConfiguratorOnly", policy => policy.RequireRole(RoleConstants.Configurator));

//    // Optional: Define combined policies for multiple roles
//    options.AddPolicy("MakerOrChecker", policy =>
//        policy.RequireRole(RoleConstants.Maker, RoleConstants.Checker));
//});

//builder.Services.AddScoped<IJwtService, JwtService>();
//if (builder.Configuration["Authentication:Type"] == "JWT")
//{
//    builder.Services.AddScoped<Infin8.Coapp.BusinessLogic.IAuthenticationHandler, JwtAuthenticationHandler>();
//}else
//{
//    throw new InvalidOperationException("Invalid authentication type configured.");
//}


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
//    {
//        Title="jwtToken_Auth_API",
//        Version="v1"
//    });
//    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
//    {
//        Name="Authorization",
//        Type=Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
//        Scheme="Bearer",
//        BearerFormat="JWT",
//        In=Microsoft.OpenApi.Models.ParameterLocation.Header,
//        Description="Here Enter JWT Token with bearer format like bearer[space]token"
//    });
//    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
//    {
//        {
//            new OpenApiSecurityScheme
//            {
//                Reference=new OpenApiReference
//                {
//                    Type=ReferenceType.SecurityScheme,
//                    Id="Bearer"
//                }
//            },
//            new string[]
//            {

//            }
//        }
//    });
//});


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

////builder.Services.AddDbContextFactory<CSISContext>(options => 
////    options.UseSqlServer(builder.Configuration.GetConnectionString("CSISDatabase")));

////builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
////                .AddEntityFrameworkStores<CSISContext>()
////                .AddDefaultTokenProviders();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IDashboardMemberHandler, DashboardMemberHandler>();
builder.Services.AddScoped<IGeneralHandler, GeneralHandler>();

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

#region Reference
builder.Services.AddScoped<IReferenceHandler, ReferenceHandler>();
builder.Services.AddScoped<IAreaMasterHandler, AreaMasterHandler>();
builder.Services.AddScoped<IBankMasterHandler, BankMasterHandler>();
builder.Services.AddScoped<IReferenceConstituencyHandler, ReferenceConstituencyHandler>();
#endregion 

#region Reports
builder.Services.AddScoped<IReportsHandler, ReportsHandler>();
builder.Services.AddScoped<IReportsJewelLoanRepository, ReportsJewelLoanRepository>();
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

builder.Services.AddScoped<INewAccountNoHandler, NewAccountNoHandler>();

builder.Services.AddScoped<IVerifyHandler, VerifyHandler>();
//builder.Services.AddScoped<IVerifyRepository, VerifyRepository>();

builder.Services.AddScoped<IUtilityHandler, UtilityHandler>();
builder.Services.AddScoped<ILoanSchemeHandler, LoanSchemeHandler>();

builder.Services.AddScoped<IUserHandler , UserHandler>();
builder.Services.AddScoped<IJwtService, JwtService>();

builder.Services.AddScoped<ITransactionsHandler, TransactionsHandler>();

//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowBlazorClient",
//        policy => policy
//            .WithOrigins("https://localhost:7073")
//            .AllowAnyMethod()
//            .AllowAnyHeader());
//});



var app = builder.Build();

//// In the Configure method
//app.UseCors("AllowBlazorClient");

/// Use CORS policy
app.UseCors("AllowBlazorFrontend");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
