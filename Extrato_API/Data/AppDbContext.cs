using Microsoft.EntityFrameworkCore;
using Extrato_API.Models;

namespace Extrato_API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Conquista> Conquistas { get; set; }
        public DbSet<ConquistaEstudante> ConquistasEstudante { get; set; }

        public DbSet<Estudante> Estudante { get; set; }
        public DbSet<Avatar> Avatares { get; set; }
        public DbSet<Modulo> Modulos { get; set; }
        public DbSet<Licao> Licoes { get; set; }
        public DbSet<Atividade> Atividades { get; set; }
        public DbSet<Alternativa> Alternativas { get; set; }
        public DbSet<Tentativa> Tentativas { get; set; }
        public DbSet<LicaoConcluida> LicaoConcluidas { get; set; }
        public DbSet<Dicionario> Dicionarios { get; set; }
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

            modelBuilder.Entity<Conquista>(entity =>
            {
                entity.ToTable("tb_conquista");
                entity.Property(e => e.Id).HasColumnName("id_conquista");
                entity.Property(e => e.Titulo).HasColumnName("titulo");
                entity.Property(e => e.Descricao).HasColumnName("descricao");
                entity.Property(e => e.Icone).HasColumnName("icone");
                entity.Property(e => e.TipoDesbloqueio).HasColumnName("tipo_desbloqueio");
                entity.Property(e => e.BackgroundCor).HasColumnName("background_cor");
            });

            modelBuilder.Entity<ConquistaEstudante>(entity =>
            {
                entity.ToTable("tb_conquista_estudante");
                entity.Property(e => e.Id).HasColumnName("id_conquista_estudante");
                entity.Property(e => e.EstudanteId).HasColumnName("estudante_id");
                entity.Property(e => e.ConquistaId).HasColumnName("id_conquista");
                entity.Property(e => e.ConquistadoEm).HasColumnName("conquistado_em");

                entity.HasIndex(e => new { e.EstudanteId, e.ConquistaId }).IsUnique();

                entity.HasOne<Estudante>()
                    .WithMany()
                    .HasForeignKey(e => e.EstudanteId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<Conquista>()
                    .WithMany()
                    .HasForeignKey(e => e.ConquistaId)
                    .OnDelete(DeleteBehavior.Cascade);
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

            modelBuilder.Entity<Dicionario>(entity =>
            {
                entity.ToTable("tb_dicionario");
                entity.Property(e => e.Id).HasColumnName("id_dicionario");
                entity.Property(e => e.Termo).HasColumnName("termo");
                entity.Property(e => e.Definicao).HasColumnName("definicao");
                entity.Property(e => e.CriadoEm).HasColumnName("criado_em");
            });
        }
    }
}