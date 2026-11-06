using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalTarefas.Web.Models;
using PortalTarefas.Web.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace PortalTarefas.Web.Controllers;

// Controller MVC: herda de Controller (com "r" no fim), que sabe
// devolver VIEWS (páginas HTML), e não só JSON.
[Authorize(Policy = "SomenteAdministrador")]
public class TarefasController : Controller
{
    private readonly ITarefaService _servico;

    public TarefasController(ITarefaService servico) => _servico = servico;

    // GET /  ou  /Tarefas
    public async Task<IActionResult> Index() =>
        View(await _servico.ListarAsync());

    // GET /Tarefas/Details/1
    public async Task<IActionResult> Details(int id)
    {
        var tarefa = await _servico.ObterAsync(id);
        return tarefa is null ? NotFound() : View(tarefa);
    }

    // GET /Tarefas/Create — mostra o formulário vazio
    public IActionResult Create() => View("Form", new TarefaInputModel());

    // POST /Tarefas/Create — recebe o formulário preenchido
    [HttpPost]
    public async Task<IActionResult> Create(TarefaInputModel input)
    {
        // Regra de ouro: só executa se a validação passou.
        // Se não passou, devolve a MESMA view com os dados digitados e as mensagens.
        if (!ModelState.IsValid) return View("Form", input);

        await _servico.CriarAsync(new Tarefa
        {
            Situacao = input.Situacao,
            Titulo = input.Titulo,
            Descricao = input.Descricao,
            Prioridade = input.Prioridade,
            Prazo = input.Prazo,
            Equipe = User.FindFirstValue("equipe") ?? "",
        });

        TempData["Mensagem"] = "Tarefa criada.";
        return RedirectToAction(nameof(Index));
    }

    // GET /Tarefas/Edit/1 — formulário preenchido com a tarefa atual
    public async Task<IActionResult> Edit(int id)
    {
        var tarefa = await _servico.ObterAsync(id);
        if (tarefa is null) return NotFound();

        return View("Form", new TarefaInputModel
        {
            Titulo = tarefa.Titulo,
            Descricao = tarefa.Descricao,
            Prioridade = tarefa.Prioridade,
            Prazo = tarefa.Prazo,
            RowVersion = tarefa.RowVersion          // Roteiro 3: guarda a versão no formulário
        });
    }

    // POST /Tarefas/Edit/1
    [HttpPost]
    public async Task<IActionResult> Edit(int id, TarefaInputModel input)
    {
        if (!ModelState.IsValid) return View("Form", input);

        try
        {
            var atualizou = await _servico.AtualizarAsync(new Tarefa
            {
                Situacao = input.Situacao,
                Id = id,
                Titulo = input.Titulo,
                Descricao = input.Descricao,
                Prioridade = input.Prioridade,
                Prazo = input.Prazo,
                RowVersion = input.RowVersion ?? ""   // Roteiro 3: a versão que o usuário tinha
            });
            if (!atualizou) return NotFound();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Roteiro 3 — ESTRATÉGIA: "o primeiro a salvar vence".
            // Nada foi gravado; avisamos o usuário e mantemos o que ele digitou.
            ModelState.AddModelError(string.Empty,
                "Esta tarefa foi alterada por outra pessoa enquanto você editava. " +
                "Abra a tarefa novamente para ver a versão atual.");
            return View("Form", input);
        }

        TempData["Mensagem"] = "Tarefa atualizada.";
        return RedirectToAction(nameof(Index));
    }
}
