using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Extrato_API.Models
{
    [Table("tb_tentativa")]
    public class Tentativa
    {
        [Key]
        [Column("id_tentativa")]
        public int Id { get; set; }

        [Required]
        [Column("estudante_id")]
        public Guid EstudanteId { get; set; }

        [Required]
        [Column("atividade_id")]
        public int AtividadeId { get; set; }

        [Column("alternativa_escolhida_id")]
        public int? AlternativaEscolhidaId { get; set; }

        [Required]
        [Column("correta")]
        public bool Correta { get; set; } = false;

        [Column("xp_ganho")]
        public int? XpGanho { get; set; } = 0;

        [Column("tentado_em")]
        public DateTime? TentadoEm { get; set; } = DateTime.UtcNow;

        // Relacionamentos
        [ForeignKey("AtividadeId")]
        public Atividade? Atividade { get; set; }

        [ForeignKey("AlternativaEscolhidaId")]
        public Alternativa? AlternativaEscolhida { get; set; }
    }
}