using System.Text;
using ClinicaAcolher.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace ClinicaAcolher.Api.Setup;

public static class UserSetup
{
    // Uso: criar-usuario <email> <Admin|Professional> [professionalId]
    public static async Task CreateUserAsync(IServiceProvider services, string[] args)
    {
        if (args.Length < 3 || args[2] is not Roles.Admin and not Roles.Professional)
        {
            Console.WriteLine("Uso: criar-usuario <email> <Admin|Professional> [professionalId]");
            return;
        }

        var email = args[1].Trim().ToLowerInvariant();
        var role = args[2];
        Guid? professionalId = args.Length > 3 ? Guid.Parse(args[3]) : null;

        Console.Write("Senha (mínimo 10 caracteres): ");
        var password = ReadPassword();

        using var scope = services.CreateScope();

        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        foreach (var name in Roles.All)
            if (!await roles.RoleExistsAsync(name))
            {
                var roleResult = await roles.CreateAsync(new IdentityRole<Guid>(name));
                if (!roleResult.Succeeded)
                {
                    foreach (var error in roleResult.Errors)
                        Console.WriteLine($"- {error.Description}");
                    return;
                }
            }

        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, ProfessionalId = professionalId };

        var result = await users.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                Console.WriteLine($"- {error.Description}");
            return;
        }

        var roleAssignment = await users.AddToRoleAsync(user, role);
        if (!roleAssignment.Succeeded)
        {
            Console.WriteLine($"Usuário criado, mas não foi possível atribuir o papel {role}:");
            foreach (var error in roleAssignment.Errors)
                Console.WriteLine($"- {error.Description}");
            return;
        }

        Console.WriteLine($"Usuário criado: {email} ({role})");
    }

    private static string ReadPassword()
    {
        // No Git Bash a entrada não é um console "de verdade"; aí a senha aparece ao digitar
        if (Console.IsInputRedirected)
            return Console.ReadLine() ?? string.Empty;

        var password = new StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
                break;
            if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                password.Length--;
            else if (!char.IsControl(key.KeyChar))
                password.Append(key.KeyChar);
        }
        Console.WriteLine();
        return password.ToString();
    }
}