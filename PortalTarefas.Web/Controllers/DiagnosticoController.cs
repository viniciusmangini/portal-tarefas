using Microsoft.AspNetCore.Mvc;

namespace PortalTarefas.Web.Controllers;

// A MESMA resposta do endpoint /api/diagnostico (Minimal API),
// agora escrita no estilo Controller, para comparar as duas formas.
[ApiController]                       // liga comportamentos de API (ex.: validação automática)
[Route("api/diagnostico-mvc")]        // endereço deste controller
[ApiExplorerSettings(IgnoreApi = true)]
public class DiagnosticoController : ControllerBase
{
    private readonly IWebHostEnvironment _ambiente;

    // Dependências chegam pelo construtor (injeção de dependência)
    public DiagnosticoController(IWebHostEnvironment ambiente)
    {
        _ambiente = ambiente;
    }

    [HttpGet]                         // responde a GET /api/diagnostico-mvc
    public IActionResult Obter()
    {
        return Ok(new
        {
            abordagem = "Controller (MVC)",
            ambiente = _ambiente.EnvironmentName,
            horario = DateTime.Now
        });
    }
}
