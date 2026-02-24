using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using SistemaLavanderia.Web.Models;
using SistemaLavanderia.Core.Entities;

namespace SistemaLavanderia.Web.Controllers;

public class RegistroController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl = "http://localhost:5154";

    public RegistroController()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(_apiBaseUrl);
    }

    // GET: Registro
    public IActionResult Index()
    {
        return View();
    }

    // POST: Registro
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(RegistroViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                // 1. Verificar se já existe usuário com este email
                var response = await _httpClient.GetAsync($"api/Usuarios");
                if (response.IsSuccessStatusCode)
                {
                    var usuarios = await response.Content.ReadAsAsync<List<Usuario>>();
                    var existe = usuarios.Any(u => u.Email == model.Email);

                    if (existe)
                    {
                        ModelState.AddModelError("Email", "Este e-mail já está cadastrado.");
                        return View(model);
                    }
                }

                // 2. Criar o usuário
                var novoUsuario = new
                {
                    nome = model.Nome,
                    email = model.Email,
                    senhaHash = model.Senha, // Em produção, hash seria feito no backend
                    perfil = "Cliente"
                };

                var json = JsonSerializer.Serialize(novoUsuario);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var responsePost = await _httpClient.PostAsync("api/Usuarios", content);

                if (responsePost.IsSuccessStatusCode)
                {
                    // 3. Criar também como cliente na tabela de Clientes
                    var novoCliente = new
                    {
                        nome = model.Nome,
                        telefone = model.Telefone,
                        email = model.Email,
                        endereco = model.Endereco ?? ""
                    };

                    var jsonCliente = JsonSerializer.Serialize(novoCliente);
                    var contentCliente = new StringContent(jsonCliente, Encoding.UTF8, "application/json");
                    await _httpClient.PostAsync("api/Clientes", contentCliente);

                    TempData["Sucesso"] = "Cadastro realizado com sucesso! Faça login para continuar.";
                    return RedirectToAction("Index", "Login");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Erro ao cadastrar usuário.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Erro ao conectar com o servidor: " + ex.Message);
            }
        }

        return View(model);
    }
}