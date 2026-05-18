using Microsoft.EntityFrameworkCore;
using Extrato_API.Models;

namespace Extrato_API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<EstatisticasUsuario> EstatisticasUsuarios { get; set; }

        public DbSet<Estudante> Estudante { get; set; }
        public DbSet<Modulo> Modulos { get; set; }
        public DbSet<Licao> Licoes { get; set; }
        public DbSet<Atividade> Atividades { get; set; }
        public DbSet<Alternativa> Alternativas { get; set; }
        public DbSet<Tentativa> Tentativas { get; set; }
        public DbSet<LicaoConcluida> LicaoConcluidas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("tb_usuario");
                entity.Property(e => e.Id).HasColumnName("id_usuario");
                entity.Property(e => e.NomeUsuario).HasColumnName("nome");
                entity.Property(e => e.Email).HasColumnName("email");
                entity.Property(e => e.SenhaHash).HasColumnName("senha");
                entity.Property(e => e.DataCadastro).HasColumnName("criado_em");
                entity.Ignore(e => e.CodigoResetSenha);
                entity.Ignore(e => e.CodigoResetExpiracao);
                //entity.Ignore(e => e.AvatarId);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}