using System.ComponentModel.DataAnnotations;

namespace Core.Service.Application.Domain.Models;

public class Receipt
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid UsuarioId { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string NomeArquivo { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(500)]
    public string CaminhoArquivo { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string TipoMime { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Descricao { get; set; }
    
    public decimal? Valor { get; set; }
    
    [MaxLength(100)]
    public string? Categoria { get; set; }
    
    public bool Ativo { get; set; } = true;
    
    public DateTime DataUpload { get; set; } = DateTime.UtcNow;
    
    public DateTime? DataAtualizacao { get; set; }
    
    // Navegação
    public virtual Usuario Usuario { get; set; } = null!;
    
    // Métodos de domínio
    public void Atualizar(string? descricao, decimal? valor, string? categoria)
    {
        Descricao = descricao;
        Valor = valor;
        Categoria = categoria;
        DataAtualizacao = DateTime.UtcNow;
    }
    
    public void Desativar()
    {
        Ativo = false;
        DataAtualizacao = DateTime.UtcNow;
    }
    
    public void Ativar()
    {
        Ativo = true;
        DataAtualizacao = DateTime.UtcNow;
    }
    
    public bool IsImagemValida()
    {
        var tiposPermitidos = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
        return tiposPermitidos.Contains(TipoMime.ToLowerInvariant());
    }
}
