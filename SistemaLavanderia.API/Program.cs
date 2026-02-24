using Microsoft.EntityFrameworkCore;
using SistemaLavanderia.Infrastructure.Data;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Configurar para escutar na porta correta (Render usa porta dinâmica)
builder.WebHost.ConfigureKestrel(options =>
{
    // Se a variável de ambiente PORT estiver definida (Render), usa ela, senão usa 8080
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    options.ListenAnyIP(int.Parse(port));
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurar CORS para permitir requisições do frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// Configurar DbContext com PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Em produção, não precisamos do Swagger (opcional)
    // app.UseSwagger();
    // app.UseSwaggerUI();
}

app.UseCors("AllowAll");
// app.UseHttpsRedirection(); // Comentado pois o Render gerencia SSL
app.UseAuthorization();
app.MapControllers();

// Aplicar migrações automaticamente e criar banco se necessário
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Tenta conectar ao banco e aplicar migrações
        Console.WriteLine("Tentando conectar ao banco de dados...");
        dbContext.Database.Migrate();
        Console.WriteLine("Banco de dados configurado com sucesso!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro ao configurar banco de dados: {ex.Message}");
        Console.WriteLine($"Detalhes: {ex.InnerException?.Message ?? "Sem detalhes adicionais"}");
        // Não derrubamos a aplicação aqui, pois pode ser erro de conexão temporário
    }
}

// Log de inicialização
Console.WriteLine("Aplicação iniciada com sucesso!");
Console.WriteLine($"Ambiente: {app.Environment.EnvironmentName}");
Console.WriteLine($"URLs: http://localhost:{Environment.GetEnvironmentVariable("PORT") ?? "8080"}");

app.Run();