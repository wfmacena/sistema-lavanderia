using System.ComponentModel.DataAnnotations;
using SistemaLavanderia.Core.Enums;

namespace SistemaLavanderia.Web.Models;

public class ItemSolicitacaoViewModel
{
    [Required(ErrorMessage = "Selecione o tipo de roupa")]
    [Display(Name = "Tipo de Roupa")]
    public TipoRoupa TipoRoupa { get; set; }

    [Required(ErrorMessage = "Informe a quantidade")]
    [Range(1, 100, ErrorMessage = "Quantidade deve ser entre 1 e 100")]
    [Display(Name = "Quantidade")]
    public int Quantidade { get; set; }

    [Display(Name = "Observações da peça")]
    public string? Observacoes { get; set; }

    // Preço calculado automaticamente
    public decimal PrecoUnitario { get; set; }
    public decimal Subtotal => Quantidade * PrecoUnitario;
}