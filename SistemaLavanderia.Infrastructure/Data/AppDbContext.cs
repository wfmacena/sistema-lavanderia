using Microsoft.EntityFrameworkCore;
using SistemaLavanderia.Core.Entities;

namespace SistemaLavanderia.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Servico> Servicos { get; set; }
    public DbSet<Usuario> Usuarios { get; set; } // SEM ACENTO!
    public DbSet<Pedido> Pedidos { get; set; }
    public DbSet<ItemPedido> ItensPedido { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurar chave composta para ItemPedido
        modelBuilder.Entity<ItemPedido>()
            .HasKey(ip => new { ip.PedidoId, ip.ServicoId });

        // Configurar relacionamentos do ItemPedido
        modelBuilder.Entity<ItemPedido>()
            .HasOne(ip => ip.Pedido)
            .WithMany(p => p.ItensPedido)
            .HasForeignKey(ip => ip.PedidoId);

        modelBuilder.Entity<ItemPedido>()
            .HasOne(ip => ip.Servico)
            .WithMany(s => s.ItensPedido)
            .HasForeignKey(ip => ip.ServicoId);

        // Configurar relacionamentos do Pedido
        modelBuilder.Entity<Pedido>()
            .HasOne(p => p.Cliente)
            .WithMany(c => c.Pedidos)
            .HasForeignKey(p => p.ClienteId);

        modelBuilder.Entity<Pedido>()
            .HasOne(p => p.Usuario)
            .WithMany(u => u.Pedidos)
            .HasForeignKey(p => p.UsuarioId);

        // Seed data - Serviços iniciais
        modelBuilder.Entity<Servico>().HasData(
            new Servico { Id = 1, Nome = "Lavagem Simples", Descricao = "Lavagem padrão", PrecoBase = 10.00m, UnidadeMedida = "Peça" },
            new Servico { Id = 2, Nome = "Lavagem a Seco", Descricao = "Lavagem especial", PrecoBase = 25.00m, UnidadeMedida = "Peça" },
            new Servico { Id = 3, Nome = "Passadoria", Descricao = "Passar roupa", PrecoBase = 8.00m, UnidadeMedida = "Peça" },
            new Servico { Id = 4, Nome = "Lavagem por Quilo", Descricao = "Preço por quilo", PrecoBase = 35.00m, UnidadeMedida = "Kg" }
        );

        // Seed data - Usuário administrador
        modelBuilder.Entity<Usuario>().HasData(
            new Usuario { Id = 1, Nome = "Administrador", Email = "admin@lavanderia.com", SenhaHash = "123456", Perfil = "Admin" }
        );
    }
}