using Campanhas.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Campanhas.Infrastructure;

public class CampanhasDbContext(DbContextOptions<CampanhasDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Campanha> Campanhas => Set<Campanha>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("usuarios");
            e.HasKey(u => u.Id);
            e.Property(u => u.NomeCompleto).HasMaxLength(200).IsRequired();
            e.Property(u => u.Email).HasMaxLength(200).IsRequired();
            e.HasIndex(u => u.Email).IsUnique(); // e-mail único no banco (requisito do edital)
            e.Property(u => u.Cpf).HasMaxLength(11).IsRequired();
            e.Property(u => u.SenhaHash).IsRequired();
            e.Property(u => u.Perfil).HasConversion<string>().HasMaxLength(20).IsRequired();
        });

        modelBuilder.Entity<Campanha>(e =>
        {
            e.ToTable("campanhas");
            e.HasKey(c => c.Id);
            e.Property(c => c.Titulo).HasMaxLength(150).IsRequired();
            e.Property(c => c.Descricao).IsRequired();
            e.Property(c => c.MetaFinanceira).HasPrecision(18, 2);
            e.Property(c => c.ValorTotalArrecadado).HasPrecision(18, 2);
            e.Property(c => c.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            e.HasIndex(c => c.Status); // painel público filtra por status
        });
    }
}
