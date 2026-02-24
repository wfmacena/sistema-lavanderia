using System;
using System.Text.Json.Serialization;

namespace SistemaLavanderia.Core.Entities;

public class ItemPedido
{
    public int PedidoId { get; set; }
    public int ServicoId { get; set; }
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }

    [JsonIgnore]
    public decimal Subtotal => Quantidade * PrecoUnitario;

    // Propriedades de navegação - com JsonIgnore para evitar validação e referências circulares
    [JsonIgnore]
    public Pedido? Pedido { get; set; }

    [JsonIgnore]
    public Servico? Servico { get; set; }
}