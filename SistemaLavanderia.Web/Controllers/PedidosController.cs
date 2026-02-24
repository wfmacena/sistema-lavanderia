using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using SistemaLavanderia.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SistemaLavanderia.Web.Controllers;

[Authorize]
public class PedidosController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl = "http://localhost:5154"; // USE A PORTA DA SUA API

    public PedidosController()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(_apiBaseUrl);
    }

    // GET: Pedidos
    public async Task<IActionResult> Index()
    {
        var perfil = User.FindFirst("Perfil")?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;

        var response = await _httpClient.GetAsync("api/Pedidos");
        if (response.IsSuccessStatusCode)
        {
            var pedidos = await response.Content.ReadAsAsync<List<Pedido>>();

            // Log para debug
            foreach (var p in pedidos)
            {
                Console.WriteLine($"Pedido {p.Id}: ClienteId={p.ClienteId}, NomeCliente={(p.Cliente != null ? p.Cliente.Nome : "null")}");
            }

            // Se for cliente, mostrar só os pedidos dele
            if (perfil == "Cliente")
            {
                var responseClientes = await _httpClient.GetAsync("api/Clientes");
                if (responseClientes.IsSuccessStatusCode)
                {
                    var clientes = await responseClientes.Content.ReadAsAsync<List<Cliente>>();
                    var cliente = clientes.FirstOrDefault(c => c.Email == email);

                    if (cliente != null)
                    {
                        pedidos = pedidos.Where(p => p.ClienteId == cliente.Id).ToList();
                    }
                }
            }

            return View(pedidos);
        }
        return View(new List<Pedido>());
    }

    // GET: Pedidos/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var perfil = User.FindFirst("Perfil")?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;

        var response = await _httpClient.GetAsync($"api/Pedidos/{id}");
        if (response.IsSuccessStatusCode)
        {
            var pedido = await response.Content.ReadAsAsync<Pedido>();

            // Se for cliente, verificar se o pedido pertence a ele
            if (perfil == "Cliente")
            {
                var responseClientes = await _httpClient.GetAsync("api/Clientes");
                if (responseClientes.IsSuccessStatusCode)
                {
                    var clientes = await responseClientes.Content.ReadAsAsync<List<Cliente>>();
                    var cliente = clientes.FirstOrDefault(c => c.Email == email);

                    if (cliente == null || pedido.ClienteId != cliente.Id)
                    {
                        return RedirectToAction("AcessoNegado", "Login");
                    }
                }
            }

            return View(pedido);
        }
        return NotFound();
    }

    // GET: Pedidos/Create
    public async Task<IActionResult> Create()
    {
        var perfil = User.FindFirst("Perfil")?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;

        // Carregar listas para os combos
        ViewBag.Servicos = await CarregarServicos();

        var pedido = new Pedido
        {
            DataPedido = DateTime.Now,
            DataPrevistaEntrega = DateTime.Now.AddDays(2),
            Status = "Aguardando",
            ItensPedido = new List<ItemPedido>()
        };

        // Se for cliente, buscar automaticamente o cliente
        if (perfil == "Cliente")
        {
            var responseClientes = await _httpClient.GetAsync("api/Clientes");
            if (responseClientes.IsSuccessStatusCode)
            {
                var clientes = await responseClientes.Content.ReadAsAsync<List<Cliente>>();
                var cliente = clientes.FirstOrDefault(c => c.Email == email);

                if (cliente != null)
                {
                    pedido.ClienteId = cliente.Id;
                    ViewBag.ClienteNome = cliente.Nome;
                }
            }
        }
        else
        {
            // Se for admin, mostrar lista de clientes
            ViewBag.Clientes = await CarregarClientes();
        }

        return View(pedido);
    }

    // POST: Pedidos/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Pedido pedido, int[] servicoIds, int[] quantidades)
    {
        var perfil = User.FindFirst("Perfil")?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;

        // Montar a lista de itens do pedido
        pedido.ItensPedido = new List<ItemPedido>();

        for (int i = 0; i < servicoIds.Length; i++)
        {
            if (quantidades[i] > 0)
            {
                pedido.ItensPedido.Add(new ItemPedido
                {
                    ServicoId = servicoIds[i],
                    Quantidade = quantidades[i]
                });
            }
        }

        // Se for cliente, garantir que o ClienteId seja o dele
        if (perfil == "Cliente")
        {
            var responseClientes = await _httpClient.GetAsync("api/Clientes");
            if (responseClientes.IsSuccessStatusCode)
            {
                var clientes = await responseClientes.Content.ReadAsAsync<List<Cliente>>();
                var cliente = clientes.FirstOrDefault(c => c.Email == email);

                if (cliente != null)
                {
                    pedido.ClienteId = cliente.Id;
                }
            }
        }

        // Garantir que o usuário logado seja o responsável
        var responseUsuarios = await _httpClient.GetAsync("api/Usuarios");
        if (responseUsuarios.IsSuccessStatusCode)
        {
            var usuarios = await responseUsuarios.Content.ReadAsAsync<List<Usuario>>();
            var usuario = usuarios.FirstOrDefault(u => u.Email == email);

            if (usuario != null)
            {
                pedido.UsuarioId = usuario.Id;
            }
        }

        if (ModelState.IsValid && pedido.ItensPedido.Any())
        {
            var json = JsonSerializer.Serialize(pedido);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/Pedidos", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Sucesso"] = "Pedido cadastrado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["Erro"] = $"Erro ao cadastrar pedido: {error}";
            }
        }

        // Se deu erro, recarrega as listas
        ViewBag.Servicos = await CarregarServicos();

        if (perfil != "Cliente")
        {
            ViewBag.Clientes = await CarregarClientes();
        }

        if (!pedido.ItensPedido.Any())
        {
            ModelState.AddModelError("", "Adicione pelo menos um item ao pedido.");
        }

        return View(pedido);
    }

    // POST: Pedidos/UpdateStatus/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var perfil = User.FindFirst("Perfil")?.Value;

        // Apenas admin pode atualizar status
        if (perfil != "Admin")
        {
            return RedirectToAction("AcessoNegado", "Login");
        }

        var content = new StringContent($"\"{status}\"", Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"api/Pedidos/{id}/status?status={status}", content);

        if (response.IsSuccessStatusCode)
        {
            TempData["Sucesso"] = "Status atualizado com sucesso!";
        }
        else
        {
            TempData["Erro"] = "Erro ao atualizar status.";
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Pedidos/Delete/5 - CORRIGIDO PARA PERMITIR QUE CLIENTES CANCELEM
    public async Task<IActionResult> Delete(int id)
    {
        var perfil = User.FindFirst("Perfil")?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;

        var response = await _httpClient.GetAsync($"api/Pedidos/{id}");
        if (response.IsSuccessStatusCode)
        {
            var pedido = await response.Content.ReadAsAsync<Pedido>();

            // Se for cliente, verificar se o pedido é dele e se está "Aguardando"
            if (perfil == "Cliente")
            {
                var responseClientes = await _httpClient.GetAsync("api/Clientes");
                if (responseClientes.IsSuccessStatusCode)
                {
                    var clientes = await responseClientes.Content.ReadAsAsync<List<Cliente>>();
                    var cliente = clientes.FirstOrDefault(c => c.Email == email);

                    if (cliente == null || pedido.ClienteId != cliente.Id)
                    {
                        TempData["Erro"] = "Você não tem permissão para cancelar este pedido.";
                        return RedirectToAction(nameof(Index));
                    }

                    if (pedido.Status != "Aguardando")
                    {
                        TempData["Erro"] = "Só é possível cancelar pedidos com status 'Aguardando'.";
                        return RedirectToAction(nameof(Index));
                    }
                }
            }

            return View(pedido);
        }
        return NotFound();
    }

    // POST: Pedidos/Delete/5 - CORRIGIDO PARA PERMITIR QUE CLIENTES CANCELEM
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var perfil = User.FindFirst("Perfil")?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;

        // Para clientes, verificar novamente antes de excluir
        if (perfil == "Cliente")
        {
            var responsePedido = await _httpClient.GetAsync($"api/Pedidos/{id}");
            if (responsePedido.IsSuccessStatusCode)
            {
                var pedido = await responsePedido.Content.ReadAsAsync<Pedido>();

                var responseClientes = await _httpClient.GetAsync("api/Clientes");
                if (responseClientes.IsSuccessStatusCode)
                {
                    var clientes = await responseClientes.Content.ReadAsAsync<List<Cliente>>();
                    var cliente = clientes.FirstOrDefault(c => c.Email == email);

                    if (cliente == null || pedido.ClienteId != cliente.Id)
                    {
                        TempData["Erro"] = "Você não tem permissão para cancelar este pedido.";
                        return RedirectToAction(nameof(Index));
                    }

                    if (pedido.Status != "Aguardando")
                    {
                        TempData["Erro"] = "Só é possível cancelar pedidos com status 'Aguardando'.";
                        return RedirectToAction(nameof(Index));
                    }
                }
            }
        }

        var response = await _httpClient.DeleteAsync($"api/Pedidos/{id}");

        if (response.IsSuccessStatusCode)
        {
            TempData["Sucesso"] = "Pedido cancelado com sucesso!";
        }
        else
        {
            TempData["Erro"] = "Erro ao cancelar pedido.";
        }

        return RedirectToAction(nameof(Index));
    }

    // Métodos auxiliares
    private async Task<List<Cliente>> CarregarClientes()
    {
        var response = await _httpClient.GetAsync("api/Clientes");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsAsync<List<Cliente>>();
        }
        return new List<Cliente>();
    }

    private async Task<List<Servico>> CarregarServicos()
    {
        var response = await _httpClient.GetAsync("api/Servicos");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsAsync<List<Servico>>();
        }
        return new List<Servico>();
    }
}