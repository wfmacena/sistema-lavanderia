using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using SistemaLavanderia.Core.Entities;
using SistemaLavanderia.Core.Enums;
using SistemaLavanderia.Web.Models;
using SistemaLavanderia.Web.Services;

namespace SistemaLavanderia.Web.Controllers;

[Authorize]
public class SolicitacaoController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl = "http://localhost:5154";

    public SolicitacaoController()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(_apiBaseUrl);
    }

    // GET: Solicitacao/NovoPedido
    // GET: Solicitacao/NovoPedido
    public async Task<IActionResult> NovoPedido()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var perfil = User.FindFirst("Perfil")?.Value;

        Console.WriteLine($"=== NOVO PEDIDO ===");
        Console.WriteLine($"Email: {email}");
        Console.WriteLine($"Perfil: {perfil}");

        var model = new SolicitacaoPedidoViewModel();

        // Se for cliente, buscar dados do cliente
        if (perfil == "Cliente")
        {
            try
            {
                var responseClientes = await _httpClient.GetAsync("api/Clientes");
                if (responseClientes.IsSuccessStatusCode)
                {
                    var clientes = await responseClientes.Content.ReadAsAsync<List<Cliente>>();
                    Console.WriteLine($"Total de clientes: {clientes.Count}");

                    var cliente = clientes.FirstOrDefault(c => c.Email == email);

                    if (cliente != null)
                    {
                        model.ClienteId = cliente.Id;
                        model.ClienteNome = cliente.Nome;

                        Console.WriteLine($"Cliente ENCONTRADO: ID={cliente.Id}, Nome={cliente.Nome}");
                    }
                    else
                    {
                        Console.WriteLine($"Cliente NÃO encontrado para email: {email}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exceção ao buscar cliente: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("Usuário é ADMIN - pode acessar a página");
            // Para admin, podemos deixar ClienteId como 0 ou buscar um cliente padrão
            model.ClienteId = 0;
            model.ClienteNome = "Administrador";
        }

        // Carregar listas para os selects
        ViewBag.TiposServico = Enum.GetValues(typeof(TipoServico))
            .Cast<TipoServico>()
            .ToDictionary(t => (int)t, t => CalculoPrecoService.ObterDescricaoServico(t));

        return View(model);
    }

    // POST: Solicitacao/CalcularPrecos
    [HttpPost]
    public IActionResult CalcularPrecos([FromBody] SolicitacaoPedidoViewModel model)
    {
        foreach (var item in model.Itens)
        {
            item.PrecoUnitario = CalculoPrecoService.CalcularPreco(model.TipoServico, item.TipoRoupa);
        }

        model.ValorTotal = model.Itens.Sum(i => i.Subtotal);

        return Json(new
        {
            success = true,
            itens = model.Itens,
            valorTotal = model.ValorTotal.ToString("C")
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FinalizarPedido(SolicitacaoPedidoViewModel model)
    {
        try
        {
            if (model.Itens == null || !model.Itens.Any())
            {
                TempData["Erro"] = "Adicione pelo menos um item ao pedido.";
                return RedirectToAction("NovoPedido");
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            // Buscar cliente completo
            var responseClientes = await _httpClient.GetAsync("api/Clientes");
            var clientes = await responseClientes.Content.ReadAsAsync<List<Cliente>>();
            var cliente = clientes.FirstOrDefault(c => c.Email == email);
            if (cliente == null)
            {
                TempData["Erro"] = "Cliente não encontrado.";
                return RedirectToAction("NovoPedido");
            }

            // Buscar usuário completo
            var responseUsuarios = await _httpClient.GetAsync("api/Usuarios");
            var usuarios = await responseUsuarios.Content.ReadAsAsync<List<Usuario>>();
            var usuario = usuarios.FirstOrDefault(u => u.Email == email);
            if (usuario == null)
            {
                TempData["Erro"] = "Usuário não encontrado.";
                return RedirectToAction("NovoPedido");
            }

            // Buscar serviço completo
            var responseServicos = await _httpClient.GetAsync("api/Servicos");
            var servicos = await responseServicos.Content.ReadAsAsync<List<Servico>>();
            var servico = servicos.FirstOrDefault();
            if (servico == null)
            {
                TempData["Erro"] = "Serviço não encontrado.";
                return RedirectToAction("NovoPedido");
            }

            // Construir os itens do pedido com o objeto Servico completo
            var itensPedido = new List<object>();
            decimal total = 0;

            foreach (var item in model.Itens)
            {
                var preco = CalculoPrecoService.CalcularPreco(model.TipoServico, item.TipoRoupa);
                total += preco * item.Quantidade;

                itensPedido.Add(new
                {
                    servico = new
                    {
                        id = servico.Id,
                        nome = servico.Nome,
                        descricao = servico.Descricao,
                        precoBase = servico.PrecoBase,
                        unidadeMedida = servico.UnidadeMedida
                    },
                    quantidade = item.Quantidade,
                    precoUnitario = preco
                });
            }

            // Montar o pedido completo com os objetos aninhados
            var pedidoParaEnviar = new
            {
                cliente = new
                {
                    id = cliente.Id,
                    nome = cliente.Nome,
                    telefone = cliente.Telefone,
                    email = cliente.Email,
                    endereco = cliente.Endereco,
                    dataCadastro = cliente.DataCadastro
                },
                usuario = new
                {
                    id = usuario.Id,
                    nome = usuario.Nome,
                    email = usuario.Email,
                    senhaHash = usuario.SenhaHash,
                    perfil = usuario.Perfil,
                    dataCadastro = usuario.DataCadastro
                },
                dataPrevistaEntrega = model.DataPrevistaEntrega?.ToString("yyyy-MM-ddTHH:mm:ss"),
                status = "Aguardando",
                observacoes = model.Observacoes ?? "",
                itensPedido = itensPedido
            };

            var json = JsonSerializer.Serialize(pedidoParaEnviar);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/Pedidos", content);

            if (response.IsSuccessStatusCode)
            {
                var pedidoCriado = await response.Content.ReadAsAsync<Pedido>();
                TempData["Sucesso"] = $"Pedido #{pedidoCriado.Id} realizado com sucesso!";
                return RedirectToAction("MeusPedidos", "Perfil");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["Erro"] = $"Erro ao realizar pedido: {error}";
                return RedirectToAction("NovoPedido");
            }
        }
        catch (Exception ex)
        {
            TempData["Erro"] = $"Erro inesperado: {ex.Message}";
            return RedirectToAction("NovoPedido");
        }
    }

    // GET: Solicitacao/ObterRoupasPorServico
    // GET: Solicitacao/ObterRoupasPorServico
    public IActionResult ObterRoupasPorServico(TipoServico servico)
    {
        try
        {
            var roupas = CalculoPrecoService.ObterRoupasPorServico(servico);

            var resultado = roupas.Select(roupa => new
            {
                id = (int)roupa, // Usar o valor do enum como ID temporário
                nome = roupa.ToString(),
                precoBase = CalculoPrecoService.CalcularPreco(servico, roupa)
            });

            return Json(resultado);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro: {ex.Message}");
            return Json(new List<object>());
        }
    }
}