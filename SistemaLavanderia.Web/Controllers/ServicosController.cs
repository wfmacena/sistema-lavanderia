using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using SistemaLavanderia.Core.Entities;

namespace SistemaLavanderia.Web.Controllers;

public class ServicosController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl = "http://localhost:5154";

    public ServicosController()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(_apiBaseUrl);
    }

    // GET: Servicos - TODOS podem ver (Clientes e Admin)
    [AllowAnonymous] // Permite acesso mesmo sem login
    public async Task<IActionResult> Index()
    {
        var response = await _httpClient.GetAsync("api/Servicos");
        if (response.IsSuccessStatusCode)
        {
            var servicos = await response.Content.ReadAsAsync<List<Servico>>();
            return View(servicos);
        }
        return View(new List<Servico>());
    }

    // GET: Servicos/Create - Só ADMIN
    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View();
    }

    // POST: Servicos/Create - Só ADMIN
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Servico servico)
    {
        if (ModelState.IsValid)
        {
            var json = JsonSerializer.Serialize(servico);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/Servicos", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Sucesso"] = "Serviço cadastrado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["Erro"] = "Erro ao cadastrar serviço.";
            }
        }
        return View(servico);
    }

    // GET: Servicos/Edit/5 - Só ADMIN
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var response = await _httpClient.GetAsync($"api/Servicos/{id}");
        if (response.IsSuccessStatusCode)
        {
            var servico = await response.Content.ReadAsAsync<Servico>();
            return View(servico);
        }
        return NotFound();
    }

    // POST: Servicos/Edit/5 - Só ADMIN
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Servico servico)
    {
        if (id != servico.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var json = JsonSerializer.Serialize(servico);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"api/Servicos/{id}", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Sucesso"] = "Serviço atualizado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["Erro"] = "Erro ao atualizar serviço.";
            }
        }
        return View(servico);
    }

    // GET: Servicos/Delete/5 - Só ADMIN
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var response = await _httpClient.GetAsync($"api/Servicos/{id}");
        if (response.IsSuccessStatusCode)
        {
            var servico = await response.Content.ReadAsAsync<Servico>();
            return View(servico);
        }
        return NotFound();
    }

    // POST: Servicos/Delete/5 - Só ADMIN
    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/Servicos/{id}");

        if (response.IsSuccessStatusCode)
        {
            TempData["Sucesso"] = "Serviço excluído com sucesso!";
        }
        else
        {
            TempData["Erro"] = "Erro ao excluir serviço.";
        }

        return RedirectToAction(nameof(Index));
    }
}