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

            var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            foreach (var role in Roles.All)
            {
                var created = await roles.CreateAsync(new IdentityRole<Guid>(role));
                if (!created.Succeeded)
                    throw new InvalidOperationException(string.Join("; ", created.Errors.Select(e => e.Description)));
            }
        }

        await CreateUserAsync(AdminEmail, AdminPassword);
        AdminClient = await CreateLoggedInClientAsync(AdminEmail, AdminPassword);
    }

    public async Task CreateUserAsync(string email, string password, string? role = null)
    {
        using var scope = Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
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
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();
        return client;
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _database.DisposeAsync();
    }
}
