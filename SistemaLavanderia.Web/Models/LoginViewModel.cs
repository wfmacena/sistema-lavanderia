using System.ComponentModel.DataAnnotations;

namespace SistemaLavanderia.Web.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "E-mail é obrigatório")]
    [EmailAddress(ErrorMessage = "E-mail inválido")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha é obrigatória")]
    [DataType(DataType.Password)]
    [Display(Name = "Senha")]
    public string Senha { get; set; } = string.Empty;

    [Display(Name = "Lembrar-me")]
    public bool LembrarMe { get; set; }
}