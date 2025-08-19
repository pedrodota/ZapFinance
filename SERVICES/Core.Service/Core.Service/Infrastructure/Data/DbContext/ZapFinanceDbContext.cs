using Core.Service.Application.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Core.Service.Infrastructure.Data.DbContext;

public class ZapFinanceDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public ZapFinanceDbContext(DbContextOptions<ZapFinanceDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios { get; set; } = null!;
    public DbSet<Receipt> Receipts { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração da entidade Usuario
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Nome)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(150);

            entity.HasIndex(e => e.Email)
                .IsUnique();

            entity.Property(e => e.Telefone)
                .HasMaxLength(20);

            entity.Property(e => e.Documento)
                .IsRequired()
                .HasMaxLength(20);

            entity.HasIndex(e => e.Documento)
                .IsUnique();

            entity.Property(e => e.TipoDocumento)
                .HasConversion<int>();

            entity.Property(e => e.DataCriacao)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.DataAtualizacao)
                .IsRequired(false);
        });

        // Configuração da entidade Receipt
        modelBuilder.Entity<Receipt>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.NomeArquivo)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.CaminhoArquivo)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.TipoMime)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Descricao)
                .HasMaxLength(500);

            entity.Property(e => e.Valor)
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.Categoria)
                .HasMaxLength(100);

            entity.Property(e => e.DataUpload)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.DataAtualizacao)
                .IsRequired(false);

            // Relacionamento com Usuario
            entity.HasOne(e => e.Usuario)
                .WithMany()
                .HasForeignKey(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices
            entity.HasIndex(e => e.UsuarioId);
            entity.HasIndex(e => e.Categoria);
            entity.HasIndex(e => e.DataUpload);
        });
    }
}
