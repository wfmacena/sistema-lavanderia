using System.Collections.Generic;

namespace SistemaLavanderia.Core.Entities;

public class Servico
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal PrecoBase { get; set; }
    public string UnidadeMedida { get; set; } = "Peça";

    // Propriedade de navegação
    public ICollection<ItemPedido> ItensPedido { get; set; } = new List<ItemPedido>();
}