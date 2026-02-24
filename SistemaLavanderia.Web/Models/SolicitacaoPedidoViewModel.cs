using System.ComponentModel.DataAnnotations;
using SistemaLavanderia.Core.Enums;

namespace SistemaLavanderia.Web.Models;

public class SolicitacaoPedidoViewModel
{
    public int ClienteId { get; set; }
    public string? ClienteNome { get; set; }

    [Required(ErrorMessage = "Selecione o tipo de serviço")]
    [Display(Name = "Tipo de Serviço")]
    public TipoServico TipoServico { get; set; }

    [Required(ErrorMessage = "Adicione pelo menos uma peça")]
    public List<ItemSolicitacaoViewModel> Itens { get; set; } = new();

    [Display(Name = "Data desejada para entrega")]
    [DataType(DataType.Date)]
    public DateTime? DataPrevistaEntrega { get; set; } = DateTime.Now.AddDays(3);

    [Display(Name = "Observações")]
    [StringLength(500, ErrorMessage = "Máximo 500 caracteres")]
    public string? Observacoes { get; set; }

    public decimal ValorTotal { get; set; }
}