using System.ComponentModel.DataAnnotations;

namespace SistemaLavanderia.Web.Models;

public class RegistroViewModel
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Nome deve ter entre 3 e 100 caracteres")]
    [Display(Name = "Nome Completo")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-mail é obrigatório")]
    [EmailAddress(ErrorMessage = "E-mail inválido")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Telefone é obrigatório")]
    [Phone(ErrorMessage = "Telefone inválido")]
    [Display(Name = "Telefone")]
    public string Telefone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha é obrigatória")]
    [DataType(DataType.Password)]
    [StringLength(20, MinimumLength = 6, ErrorMessage = "Senha deve ter entre 6 e 20 caracteres")]
    [Display(Name = "Senha")]
    public string Senha { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirme a senha")]
    [DataType(DataType.Password)]
    [Compare("Senha", ErrorMessage = "As senhas não conferem")]
    [Display(Name = "Confirmar Senha")]
    public string ConfirmarSenha { get; set; } = string.Empty;

    [Display(Name = "Endereço")]
    public string? Endereco { get; set; }
}