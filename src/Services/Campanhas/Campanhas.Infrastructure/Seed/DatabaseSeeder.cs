using Campanhas.Domain.Entities;
using Campanhas.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Campanhas.Infrastructure.Seed;

/// <summary>
/// Aplica migrations pendentes e garante o usuário GestorONG inicial.
/// Idempotente: pode rodar em todo startup (replicas: 1 no Kubernetes).
/// </summary>
public static class DatabaseSeeder
{
    public static async Task MigrarESemearAsync(
        CampanhasDbContext db, string gestorEmail, string gestorSenha, ILogger logger)
    {
        // Retry manual: no cluster o Postgres pode subir depois da API.
        const int tentativas = 10;
        for (var i = 1; ; i++)
        {
            try
            {
                await db.Database.MigrateAsync();
                break;
            }
            catch (Exception ex) when (i < tentativas)
            {
                logger.LogWarning(
                    "Banco indisponível (tentativa {Tentativa}/{Total}): {Erro}. Aguardando 3s...",
                    i, tentativas, ex.Message);
                await Task.Delay(TimeSpan.FromSeconds(3));
            }
        }

        var existeGestor = await db.Usuarios.AnyAsync(u => u.Perfil == PerfilUsuario.GestorONG);
        if (!existeGestor)
        {
            var senhaHash = BCrypt.Net.BCrypt.HashPassword(gestorSenha);
            var gestor = Usuario.CriarGestor(
                "Gestor Esperança Solidária", gestorEmail, senhaHash, DateTime.UtcNow);
            db.Usuarios.Add(gestor);
            await db.SaveChangesAsync();
            logger.LogInformation("Usuário GestorONG semeado: {Email}", gestorEmail);
        }
    }
}
