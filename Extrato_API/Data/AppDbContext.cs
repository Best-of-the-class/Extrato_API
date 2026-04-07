using Microsoft.EntityFrameworkCore;
using Extrato_API.Models; // Importa a sua pasta Models

namespace Extrato_API.Data // Se criar na raiz, pode ser só namespace Extrato_API
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Esta linha é a mágica: ela diz que a sua classe Usuario vai virar uma tabela chamada "Usuarios"
        public DbSet<Usuario> Usuarios { get; set; }

        // Tabela de estatísticas do perfil do usuário
        public DbSet<EstatisticasUsuario> EstatisticasUsuarios { get; set; }
    }
}
