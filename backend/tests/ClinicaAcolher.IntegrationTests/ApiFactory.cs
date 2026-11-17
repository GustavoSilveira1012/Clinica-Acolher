using System.Security.Cryptography;
using System.Net.Http.Json;
using System.Text.Json;
using ClinicaAcolher.Infrastructure.Identity;
using ClinicaAcolher.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;
using Testcontainers.PostgreSql;
using OtpNet;

namespace ClinicaAcolher.IntegrationTests;

public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string AdminEmail = "admin@exemplo.org";
    public const string AdminPassword = "senha-de-teste-123";

    private static readonly string TestEncryptionKey =
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

    public HttpClient AdminClient { get; private set; } = null!;

    public ApiFactory() => ClientOptions.BaseAddress = new Uri("https://localhost");

    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:17")
        .Build();

    public FakeTimeProvider Clock { get; } = new(
        new DateTimeOffset(2026, 11, 13, 12, 0, 0, TimeSpan.Zero));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Default", _database.GetConnectionString());
        builder.UseSetting("Workers:SeriesGeneration", "false");
        builder.UseSetting("Encryption:ActiveKeyId", "teste");
        builder.UseSetting("Encryption:Keys:teste", TestEncryptionKey);

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Clock);
        });
    }

    public async Task InitializeAsync()
    {
        await _database.StartAsync();

        using (var scope = Services.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();

            var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            foreach (var role in Roles.All)
            {
                var created = await roles.CreateAsync(new IdentityRole<Guid>(role));
                if (!created.Succeeded)
                    throw new InvalidOperationException(string.Join("; ", created.Errors.Select(e => e.Description)));
            }
        }

        AdminClient = await CreateStaffClientAsync(AdminEmail, AdminPassword, Roles.Admin);
    }

    public async Task CreateUserAsync(string email, string password, string? role = null, Guid? professionalId = null)
    {
        using var scope = Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser
        {
            UserName = email, Email = email, EmailConfirmed = true, ProfessionalId = professionalId,
        };
        var created = await users.CreateAsync(user, password);
        if (!created.Succeeded)
            throw new InvalidOperationException(string.Join("; ", created.Errors.Select(e => e.Description)));

        if (role is not null)
        {
            var assigned = await users.AddToRoleAsync(user, role);
            if (!assigned.Succeeded)
                throw new InvalidOperationException(string.Join("; ", assigned.Errors.Select(e => e.Description)));
        }
    }

    public async Task<HttpClient> CreateLoggedInClientAsync(string email, string password)
    {
        var client = CreateClient();
        await LoginAsync(client, email, password);
        return client;
    }

    public async Task LoginAsync(HttpClient client, string email, string password)
    {
        using var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        if (!body.GetProperty("requiresTwoFactor").GetBoolean())
            return;

        using var scope = Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await users.FindByEmailAsync(email) ?? throw new InvalidOperationException("Usuário de teste não encontrado.");
        var key = await users.GetAuthenticatorKeyAsync(user) ?? throw new InvalidOperationException("MFA de teste não configurado.");
        var code = new Totp(Base32Encoding.ToBytes(key)).ComputeTotp();
        using var secondFactor = await client.PostAsJsonAsync("/api/v1/auth/login/2fa", new { code });
        secondFactor.EnsureSuccessStatusCode();
    }

    // Cria a conta de equipe e configura/comprova o MFA pela API.
    public async Task<HttpClient> CreateStaffClientAsync(
        string email, string password, string role, Guid? professionalId = null)
    {
        await CreateUserAsync(email, password, role, professionalId);
        var client = await CreateLoggedInClientAsync(email, password);
        using var setup = await client.PostAsync("/api/v1/auth/2fa/setup", null);
        setup.EnsureSuccessStatusCode();
        var body = await setup.Content.ReadFromJsonAsync<JsonElement>();
        var key = body.GetProperty("manualKey").GetString()!.Replace(" ", "").ToUpperInvariant();
        var code = new Totp(Base32Encoding.ToBytes(key)).ComputeTotp();
        using var enable = await client.PostAsJsonAsync("/api/v1/auth/2fa/enable", new { code });
        enable.EnsureSuccessStatusCode();
        return client;
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _database.DisposeAsync();
    }
}
