using System.Net.Http.Json;
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

namespace ClinicaAcolher.IntegrationTests;

public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string AdminEmail = "admin@exemplo.org";
    public const string AdminPassword = "senha-de-teste-123";

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

            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var created = await users.CreateAsync(
                new ApplicationUser { UserName = AdminEmail, Email = AdminEmail, EmailConfirmed = true }, AdminPassword);
            if (!created.Succeeded)
                throw new InvalidOperationException(string.Join("; ", created.Errors.Select(e => e.Description)));
        }

        AdminClient = await CreateLoggedInClientAsync(AdminEmail, AdminPassword);
    }

    public async Task<HttpClient> CreateLoggedInClientAsync(string email, string password)
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();
        return client;
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _database.DisposeAsync();
    }
}
