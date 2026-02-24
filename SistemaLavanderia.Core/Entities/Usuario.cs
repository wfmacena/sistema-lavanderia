using System;
using System.Collections.Generic;

namespace SistemaLavanderia.Core.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public string Perfil { get; set; } = "Atendente";
    public DateTime DataCadastro { get; set; } = DateTime.Now;

    // Propriedade de navegação
    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}