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
        public DbSet<Avatar> Avatares { get; set; }
        public DbSet<Modulo> Modulos { get; set; }
        public DbSet<Licao> Licoes { get; set; }
        public DbSet<Atividade> Atividades { get; set; }
        public DbSet<Alternativa> Alternativas { get; set; }
        public DbSet<Tentativa> Tentativas { get; set; }
        public DbSet<LicaoConcluida> LicaoConcluidas { get; set; }
        public DbSet<ResetSenha> ResetSenhas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("tb_usuario");
                entity.Property(e => e.Id).HasColumnName("id_usuario");
                entity.Property(e => e.NomeUsuario).HasColumnName("nome");
                entity.Property(e => e.Email).HasColumnName("email");
                entity.Property(e => e.SenhaHash).HasColumnName("senha");
                entity.Property(e => e.TipoUsuario).HasColumnName("tipo_usuario");
                entity.Property(e => e.DataCadastro).HasColumnName("criado_em");
                entity.Ignore(e => e.CodigoResetSenha);
                entity.Ignore(e => e.CodigoResetExpiracao);
            });

            modelBuilder.Entity<Estudante>(entity =>
            {
                entity.ToTable("tb_estudante");
                entity.Property(e => e.Id).HasColumnName("id_estudante");
                entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");
                entity.Property(e => e.AvatarId).HasColumnName("avatar_id");
                entity.Property(e => e.XpTotal).HasColumnName("xp_total");
                entity.Property(e => e.Nivel).HasColumnName("nivel");
                entity.Property(e => e.LicoesConcluidas).HasColumnName("licoes_concluidas");
                entity.Property(e => e.ExerciciosResolvidos).HasColumnName("exercicios_resolvidos");
                entity.Property(e => e.SequenciaDias).HasColumnName("sequencia_dias");
                entity.Property(e => e.DataUltimaAtividade).HasColumnName("data_ultima_atividade");
                entity.Property(e => e.QuantVidas).HasColumnName("quant_vidas");
            });

            modelBuilder.Entity<Avatar>(entity =>
            {
                entity.ToTable("tb_avatar");
                entity.Property(e => e.Id).HasColumnName("id_avatar");
                entity.Property(e => e.UrlImagem).HasColumnName("url_imagem");
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}