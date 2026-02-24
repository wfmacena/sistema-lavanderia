using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SistemaLavanderia.Core.Entities;

public class Pedido
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public int UsuarioId { get; set; }
    public DateTime DataPedido { get; set; }
    public DateTime? DataPrevistaEntrega { get; set; }
    public string Status { get; set; } = "Aguardando";
    public decimal ValorTotal { get; set; }
    public string? Observacoes { get; set; }

    // Propriedades de navegação com JsonIgnore
    [JsonIgnore]
    public Cliente? Cliente { get; set; }

    [JsonIgnore]
    public Usuario? Usuario { get; set; }

    public ICollection<ItemPedido> ItensPedido { get; set; } = new List<ItemPedido>();
}