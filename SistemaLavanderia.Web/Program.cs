using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// *** CONFIGURAÇÃO CRÍTICA PARA O RENDER ***
// Permite que a aplicação escute na porta fornecida pela variável de ambiente PORT
builder.WebHost.ConfigureKestrel(options =>
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "5090"; // 5090 é a porta local padrão
    options.ListenAnyIP(int.Parse(port));
});

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configurar autenticação por cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index";
        options.LogoutPath = "/Login/Logout";
        options.AccessDeniedPath = "/Login/AcessoNegado";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpClient();

// *** CONFIGURAÇÃO DO HttpClient PARA CONSUMIR A API ***
// Ele vai pegar a URL base da API do arquivo de configuração
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5154")
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// app.UseHttpsRedirection(); // Comentado, pois o Render gerencia o SSL no nível do proxy.
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// *** ROTA PADRÃO - DEVE APONTAR PARA O CONTROLLER DE LOGIN ***
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}"); // A página inicial é a de Login

// Log de inicialização
Console.WriteLine("=========================================");
Console.WriteLine("Aplicação Web iniciada com sucesso!");
Console.WriteLine($"Ambiente: {app.Environment.EnvironmentName}");
Console.WriteLine($"URL: https://localhost:{Environment.GetEnvironmentVariable("PORT") ?? "5090"}");
Console.WriteLine("=========================================");

app.Run();