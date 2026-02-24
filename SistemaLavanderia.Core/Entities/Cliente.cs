using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SistemaLavanderia.Core.Entities;

public class Cliente
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres")]
    [Display(Name = "Nome Completo")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O telefone é obrigatório")]
    [Phone(ErrorMessage = "Telefone inválido")]
    [StringLength(20, ErrorMessage = "Telefone muito longo")]
    [Display(Name = "Telefone")]
    public string? Telefone { get; set; }

    [EmailAddress(ErrorMessage = "E-mail inválido")]
    [StringLength(100, ErrorMessage = "E-mail muito longo")]
    [Display(Name = "E-mail")]
    public string? Email { get; set; }

    [StringLength(200, ErrorMessage = "Endereço muito longo")]
    [Display(Name = "Endereço")]
    public string? Endereco { get; set; }

    [Display(Name = "Data de Cadastro")]
    [DataType(DataType.Date)]
    public DateTime DataCadastro { get; set; } = DateTime.Now;

    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}