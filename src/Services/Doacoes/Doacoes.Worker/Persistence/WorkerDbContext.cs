using Microsoft.EntityFrameworkCore;

namespace Doacoes.Worker.Persistence;

/// <summary>
/// Visão mínima da campanha: só o que o Worker precisa para incrementar o
/// total (SOLID/ISP aplicado a dados — nada de título, descrição, datas...).
/// </summary>
public class CampanhaResumo
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal ValorTotalArrecadado { get; set; }
}

/// <summary>
/// DbContext próprio do Worker sobre o banco de Campanhas. Mapeia apenas a
/// tabela "campanhas" (3 colunas) e é dono da tabela de controle de
/// idempotência "doacoes_processadas". Sem retry automático do Npgsql:
/// as transações explícitas exigem isso, e o retry fica no consumer.
/// </summary>
public class WorkerDbContext(DbContextOptions<WorkerDbContext> options) : DbContext(options)
{
    public DbSet<CampanhaResumo> Campanhas => Set<CampanhaResumo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CampanhaResumo>(e =>
        {
            e.ToTable("campanhas");
            e.HasKey(c => c.Id);
            e.Property(c => c.Status).HasMaxLength(20);
            e.Property(c => c.ValorTotalArrecadado).HasPrecision(18, 2);
        });
    }
}
