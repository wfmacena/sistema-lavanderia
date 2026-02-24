using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using SistemaLavanderia.Core.Entities;
using Microsoft.AspNetCore.Authorization;

namespace SistemaLavanderia.Web.Controllers;

[Authorize(Roles = "Admin")]
public class ClientesController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl = "http://localhost:5154"; // USE A PORTA QUE APARECE NO SEU TERMINAL

    public ClientesController()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(_apiBaseUrl);
    }

    // GET: Clientes
    public async Task<IActionResult> Index()
    {
        var response = await _httpClient.GetAsync("api/Clientes");
        if (response.IsSuccessStatusCode)
        {
            var clientes = await response.Content.ReadAsAsync<List<Cliente>>();
            return View(clientes);
        }
        return View(new List<Cliente>());
    }

    // GET: Clientes/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var response = await _httpClient.GetAsync($"api/Clientes/{id}");
        if (response.IsSuccessStatusCode)
        {
            var cliente = await response.Content.ReadAsAsync<Cliente>();
            return View(cliente);
        }
        return NotFound();
    }

    // GET: Clientes/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Clientes/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Cliente cliente)
    {
        if (ModelState.IsValid)
        {
            var json = JsonSerializer.Serialize(cliente);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/Clientes", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Sucesso"] = "Cliente cadastrado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
        }
        return View(cliente);
    }

    // GET: Clientes/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var response = await _httpClient.GetAsync($"api/Clientes/{id}");
        if (response.IsSuccessStatusCode)
        {
            var cliente = await response.Content.ReadAsAsync<Cliente>();
            return View(cliente);
        }
        return NotFound();
    }

    // POST: Clientes/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Cliente cliente)
    {
        if (id != cliente.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var json = JsonSerializer.Serialize(cliente);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"api/Clientes/{id}", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Sucesso"] = "Cliente atualizado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
        }
        return View(cliente);
    }

    // GET: Clientes/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var response = await _httpClient.GetAsync($"api/Clientes/{id}");
        if (response.IsSuccessStatusCode)
        {
            var cliente = await response.Content.ReadAsAsync<Cliente>();
            return View(cliente);
        }
        return NotFound();
    }

    // POST: Clientes/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/Clientes/{id}");

        if (response.IsSuccessStatusCode)
        {
            TempData["Sucesso"] = "Cliente excluído com sucesso!";
        }

        return RedirectToAction(nameof(Index));
    }
}