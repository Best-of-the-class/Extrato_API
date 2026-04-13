using Microsoft.EntityFrameworkCore;
using Extrato_API.Models;

namespace Extrato_API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<EstatisticasUsuario> EstatisticasUsuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("tb_usuário");
                entity.Property(e => e.Id).HasColumnName("id_usuario");
                entity.Property(e => e.NomeUsuario).HasColumnName("nome");
                entity.Property(e => e.Email).HasColumnName("email");
                entity.Property(e => e.SenhaHash).HasColumnName("senha");
                entity.Property(e => e.DataCadastro).HasColumnName("criado_em");
                entity.Ignore(e => e.CodigoResetSenha);
                entity.Ignore(e => e.CodigoResetExpiracao);
                entity.Ignore(e => e.AvatarId);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
