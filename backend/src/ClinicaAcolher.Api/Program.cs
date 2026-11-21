using ClinicaAcolher.Infrastructure;
using ClinicaAcolher.Api.Errors;
using ClinicaAcolher.Application.Patients;
using ClinicaAcolher.Application.PatientAccess;
using ClinicaAcolher.Application.ClinicalNotes;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using ClinicaAcolher.Application.Professionals;
using ClinicaAcolher.Application.Relationships;
using ClinicaAcolher.Application.Appointments;
using ClinicaAcolher.Application.Attendance;
using ClinicaAcolher.Application.Availability;
using ClinicaAcolher.Api.Workers;
using ClinicaAcolher.Infrastructure.Identity;
using ClinicaAcolher.Infrastructure.Persistence;
using ClinicaAcolher.Infrastructure.Security;
using ClinicaAcolher.Infrastructure.Auditing;
using ClinicaAcolher.Api.Setup;
using ClinicaAcolher.Api.Auth;
using ClinicaAcolher.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddOptions<EncryptionOptions>()
    .Bind(builder.Configuration.GetSection(EncryptionOptions.Section))
    .Validate(o => o.IsValid(),
        "Configuração de criptografia inválida: defina Encryption:ActiveKeyId e Encryption:Keys com chaves de 32 bytes em Base64.")
    .ValidateOnStart();
builder.Services.AddOptions<AuditOptions>()
    .Bind(builder.Configuration.GetSection(AuditOptions.Section))
    .Validate(o => o.IsValid(), "Configuração de auditoria inválida: defina Audit:HmacKey com pelo menos 32 bytes em Base64.")
    .ValidateOnStart();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.AddScoped<AccessControl>();
builder.Services.AddScoped<PatientService>();
builder.Services.AddScoped<IPatientAccounts, IdentityPatientAccounts>();
builder.Services.AddScoped<PatientInvitationService>();
builder.Services.AddScoped<ProfessionalService>();
builder.Services.AddScoped<RelationshipService>();
builder.Services.AddScoped<AppointmentService>();
builder.Services.AddScoped<SeriesService>();
builder.Services.AddScoped<AttendancePolicyService>();
builder.Services.AddScoped<AvailabilityService>();
builder.Services.AddScoped<ClinicalNoteService>();
if (builder.Configuration.GetValue("Workers:SeriesGeneration", true))
    builder.Services.AddHostedService<SeriesGenerationWorker>();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ProblemExceptionHandler>();
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services
    .AddIdentityCore<ApplicationUser>(o =>
    {
        // RN-SEG-02: tamanho mínimo pesa mais que exigir símbolos e maiúsculas
        o.Password.RequiredLength = 10;
        o.Password.RequireDigit = false;
        o.Password.RequireLowercase = false;
        o.Password.RequireUppercase = false;
        o.Password.RequireNonAlphanumeric = false;

        o.User.RequireUniqueEmail = true;

        o.Lockout.MaxFailedAccessAttempts = 5;
        o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        o.Lockout.AllowedForNewUsers = true;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddClaimsPrincipalFactory<AppClaimsPrincipalFactory>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme).AddIdentityCookies();

builder.Services.ConfigureApplicationCookie(o =>
{
    o.Cookie.Name = "acolher.sessao";
    o.Cookie.HttpOnly = true;
    o.Cookie.SameSite = SameSiteMode.Strict;
    o.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;

    // RN-SEG-04: a sessão expira depois de 30 minutos sem uso
    o.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    o.SlidingExpiration = true;

    // Uma API responde 401 e 403; quem decide para onde ir é o frontend
    o.Events.OnRedirectToLogin = ctx =>
    {
        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    o.Events.OnRedirectToAccessDenied = ctx =>
    {
        ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

builder.Services.Configure<CookieAuthenticationOptions>(IdentityConstants.TwoFactorUserIdScheme, o =>
{
    o.Cookie.HttpOnly = true;
    o.Cookie.SameSite = SameSiteMode.Strict;
    o.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    o.ExpireTimeSpan = TimeSpan.FromMinutes(5);
});

builder.Services.AddSingleton<IAuthorizationHandler, StaffMfaHandler>();

builder.Services.AddAuthorizationBuilder()
    // Seguro por padrão: toda rota é da equipe, com 2FA, a não ser que diga o contrário
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .RequireRole(Roles.Admin, Roles.Professional)
        .AddRequirements(new StaffMfaRequirement())
        .Build())
    .AddPolicy(Policies.LoggedIn, policy => policy.RequireAuthenticatedUser())
    .AddPolicy(Policies.Patient, policy => policy.RequireAuthenticatedUser().RequireRole(Roles.Patient))
    .AddPolicy(Policies.StaffOrPatient, policy => policy
        .RequireAuthenticatedUser()
        .RequireRole(Roles.Admin, Roles.Professional, Roles.Patient)
        .AddRequirements(new StaffMfaRequirement()));

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("convites", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1) }));
});

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("A connection string 'Default' não foi configurada.");

builder.Services.AddInfrastructure(connectionString);
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

if (args.Length > 0 && args[0] == "criar-usuario")
{
    await UserSetup.CreateUserAsync(app.Services, args);
    return;
}

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();

app.Run();
public partial class Program;
