using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Extrato_API.Models
{
    [Table("tb_estudante")]
    public class Estudante
    {
        [Key]
        [Column("id_estudante")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("usuario_id")]
        public Guid UsuarioId { get; set; }

        [Column("avatar_id")]
        public int? AvatarId { get; set; }

        [Column("xp_total")]
        public int XpTotal { get; set; } = 0;

        [Column("nivel")]
        public int Nivel { get; set; } = 1;

        [Column("licoes_concluidas")]
        public int LicoesConcluidas { get; set; } = 0;

        [Column("exercicios_resolvidos")]
        public int ExerciciosResolvidos { get; set; } = 0;

        [Column("sequencia_dias")]
        public int SequenciaDias { get; set; } = 0;

        [Column("data_ultima_atividade")]
        public DateTime? DataUltimaAtividade { get; set; }

        [Column("quant_vidas")]
        public int QuantVidas { get; set; } = 5;
    }
}