using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using SistemaLavanderia.Core.Entities;

namespace SistemaLavanderia.Web.Controllers;

[Authorize] // Precisa estar logado
public class PerfilController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl = "http://localhost:5154";

    public PerfilController()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(_apiBaseUrl);
    }

    // GET: Perfil
    public async Task<IActionResult> Index()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(email))
            return RedirectToAction("Index", "Login");

        // Buscar o usuário
        var response = await _httpClient.GetAsync("api/Usuarios");
        if (response.IsSuccessStatusCode)
        {
            var usuarios = await response.Content.ReadAsAsync<List<Usuario>>();
            var usuario = usuarios.FirstOrDefault(u => u.Email == email);

            if (usuario != null)
            {
                // Buscar o cliente associado
                var responseClientes = await _httpClient.GetAsync("api/Clientes");
                if (responseClientes.IsSuccessStatusCode)
                {
                    var clientes = await responseClientes.Content.ReadAsAsync<List<Cliente>>();
                    var cliente = clientes.FirstOrDefault(c => c.Email == email);

                    ViewBag.Cliente = cliente;
                    return View(usuario);
                }
            }
        }

        return View(new Usuario());
    }

    // POST: Perfil/AtualizarCliente
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AtualizarCliente(Cliente cliente)
    {
        if (ModelState.IsValid)
        {
            var json = JsonSerializer.Serialize(cliente);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"api/Clientes/{cliente.Id}", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Sucesso"] = "Perfil atualizado com sucesso!";
            }
            else
            {
                TempData["Erro"] = "Erro ao atualizar perfil.";
            }
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: Perfil/AlterarSenha
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlterarSenha(string senhaAtual, string novaSenha, string confirmarSenha)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;

        if (novaSenha != confirmarSenha)
        {
            TempData["Erro"] = "As senhas não conferem.";
            return RedirectToAction(nameof(Index));
        }

        // Buscar o usuário
        var response = await _httpClient.GetAsync("api/Usuarios");
        if (response.IsSuccessStatusCode)
        {
            var usuarios = await response.Content.ReadAsAsync<List<Usuario>>();
            var usuario = usuarios.FirstOrDefault(u => u.Email == email);

            if (usuario != null && usuario.SenhaHash == senhaAtual)
            {
                usuario.SenhaHash = novaSenha;

                var json = JsonSerializer.Serialize(usuario);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var responsePut = await _httpClient.PutAsync($"api/Usuarios/{usuario.Id}", content);

                if (responsePut.IsSuccessStatusCode)
                {
                    TempData["Sucesso"] = "Senha alterada com sucesso!";
                }
                else
                {
                    TempData["Erro"] = "Erro ao alterar senha.";
                }
            }
            else
            {
                TempData["Erro"] = "Senha atual incorreta.";
            }
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Perfil/MeusPedidos
    public async Task<IActionResult> MeusPedidos()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var perfil = User.FindFirst("Perfil")?.Value;

        // Buscar todos os pedidos
        var responsePedidos = await _httpClient.GetAsync("api/Pedidos");
        if (!responsePedidos.IsSuccessStatusCode)
        {
            return View(new List<Pedido>());
        }

        var todosPedidos = await responsePedidos.Content.ReadAsAsync<List<Pedido>>();

        // Se for admin, mostra todos
        if (perfil == "Admin")
        {
            return View(todosPedidos);
        }

        // Se for cliente, filtrar pelos pedidos do cliente
        var responseClientes = await _httpClient.GetAsync("api/Clientes");
        if (responseClientes.IsSuccessStatusCode)
        {
            var clientes = await responseClientes.Content.ReadAsAsync<List<Cliente>>();
            var cliente = clientes.FirstOrDefault(c => c.Email == email);

            if (cliente != null)
            {
                var meusPedidos = todosPedidos.Where(p => p.ClienteId == cliente.Id).ToList();
                return View(meusPedidos);
            }
        }

        return View(new List<Pedido>());
    }
}