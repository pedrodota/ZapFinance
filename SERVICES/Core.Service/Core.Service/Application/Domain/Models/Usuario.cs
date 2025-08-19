using System.ComponentModel.DataAnnotations;

namespace Core.Service.Application.Domain.Models;

public class Usuario
{
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string? Telefone { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string Documento { get; set; } = string.Empty;
    
    public TipoDocumento TipoDocumento { get; set; }
    
    public bool Ativo { get; set; } = true;
    
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    
    public DateTime? DataAtualizacao { get; set; }
    
    [MaxLength(255)]
    public string? Senha { get; set; }
    
    // Método para atualizar dados
    public void Atualizar(string nome, string email, string? telefone)
    {
        Nome = nome;
        Email = email;
        Telefone = telefone;
        DataAtualizacao = DateTime.UtcNow;
    }
    
    // Método para desativar usuário
    public void Desativar()
    {
        Ativo = false;
        DataAtualizacao = DateTime.UtcNow;
    }
    
    // Método para ativar usuário
    public void Ativar()
    {
        Ativo = true;
        DataAtualizacao = DateTime.UtcNow;
    }
}

public enum TipoDocumento
{
    CPF = 0,
    CNPJ = 1,
    Passaporte = 2
}
