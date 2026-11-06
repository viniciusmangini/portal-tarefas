using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using PortalTarefas.Web.Contratos;

namespace PortalTarefas.Web.Endpoints;

public static class ContaEndpoints
{
    public static void MapContaEndpoints(this IEndpointRouteBuilder app)
    {
        var conta = app.MapGroup("/api/conta").WithTags("Conta");

        // Endpoints prontos do Identity: /login, /register, /refresh, /manage/info...
        //   POST /api/conta/login?useCookies=true  → cookie (usado pela SPA)
        //   POST /api/conta/login                  → token bearer (usado no .http)
        conta.MapIdentityApi<IdentityUser>();

        // O Identity não traz logout pronto: apagamos o cookie de sessão
        conta.MapPost("/logout", async (SignInManager<IdentityUser> signIn) =>
        {
            await signIn.SignOutAsync();
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("Sair")
        .Produces(StatusCodes.Status204NoContent);

        // Quem está logado — a SPA usa para restaurar a sessão
        conta.MapGet("/eu", (ClaimsPrincipal usuario) => Results.Ok(new UsuarioDto(
            usuario.Identity!.Name!,
            usuario.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList(),
            usuario.FindFirstValue("equipe"),
            usuario.HasClaim("permissao", "editar-outras-equipes"))))
        .RequireAuthorization()
        .WithName("ObterUsuarioAtual")
        .Produces<UsuarioDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}