using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using SistemaLavanderia.Web.Models;
using SistemaLavanderia.Core.Entities;

namespace SistemaLavanderia.Web.Controllers;

public class LoginController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl = "http://localhost:5154"; // USE A PORTA DA SUA API

    public LoginController()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(_apiBaseUrl);
    }

    // GET: Login
    public IActionResult Index()
    {
        // Se já estiver autenticado, redireciona para home
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    // POST: Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                // Buscar usuário na API
                var response = await _httpClient.GetAsync("api/Usuarios");

                if (response.IsSuccessStatusCode)
                {
                    var usuarios = await response.Content.ReadAsAsync<List<Usuario>>();
                    var usuario = usuarios.FirstOrDefault(u =>
                        u.Email == model.Email && u.SenhaHash == model.Senha);

                    if (usuario != null)
                    {
                        // Criar claims do usuário
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, usuario.Nome),
                            new Claim(ClaimTypes.Email, usuario.Email),
                            new Claim(ClaimTypes.Role, usuario.Perfil), // ROLE é importante!
                            new Claim("UsuarioId", usuario.Id.ToString()),
                            new Claim("Perfil", usuario.Perfil) // Guardar o perfil
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = model.LembrarMe,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                        };

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties);

                        TempData["Sucesso"] = $"Bem-vindo, {usuario.Nome}!";
                        return RedirectToAction("Index", "Home");
                    }
                }

                ModelState.AddModelError(string.Empty, "E-mail ou senha inválidos.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Erro ao conectar com o servidor: " + ex.Message);
            }
        }

        return View(model);
    }

    // GET: Logout
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["Sucesso"] = "Você saiu do sistema.";
        return RedirectToAction(nameof(Index));
    }

    // GET: AcessoNegado
    public IActionResult AcessoNegado()
    {
        return View();
    }
}