using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace PortalTarefas.Web.Data;

// SEED CONTROLADO de papéis e usuários de teste.
// Só roda quando pedido: dotnet run --launch-profile http -- --seed
// A senha vem dos User Secrets (Seed:SenhaInicial), nunca do código. É idempotente.
public static class SeedIdentidade
{
    public static async Task ExecutarAsync(WebApplication app)
    {
        var senha = app.Configuration["Seed:SenhaInicial"];
        if (string.IsNullOrWhiteSpace(senha))
            throw new InvalidOperationException(
                "Defina a senha dos usuários de teste: dotnet user-secrets set \"Seed:SenhaInicial\" \"<senha>\"");

        using var escopo = app.Services.CreateScope();
        var papeis = escopo.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var usuarios = escopo.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        foreach (var papel in new[] { "Usuario", "Administrador" })
            if (!await papeis.RoleExistsAsync(papel))
                await papeis.CreateAsync(new IdentityRole(papel));

        await CriarAsync(usuarios, "ana@portal.local", senha, "Usuario", "Alfa", podeEditarOutrasEquipes: false);
        await CriarAsync(usuarios, "admin@portal.local", senha, "Administrador", "Alfa", podeEditarOutrasEquipes: true);

        app.Logger.LogInformation("Seed de identidade concluído.");
    }

    private static async Task CriarAsync(UserManager<IdentityUser> usuarios, string email, string senha,
        string papel, string equipe, bool podeEditarOutrasEquipes)
    {
        if (await usuarios.FindByEmailAsync(email) is not null) return; // já existe: nada a fazer

        var usuario = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
        var resultado = await usuarios.CreateAsync(usuario, senha);

        if (!resultado.Succeeded)
            throw new InvalidOperationException(
                $"Não foi possível criar {email}: " + string.Join(" ", resultado.Errors.Select(e => e.Description)));

        await usuarios.AddToRoleAsync(usuario, papel);
        await usuarios.AddClaimAsync(usuario, new Claim("equipe", equipe));
        if (podeEditarOutrasEquipes)
            await usuarios.AddClaimAsync(usuario, new Claim("permissao", "editar-outras-equipes"));
    }
}