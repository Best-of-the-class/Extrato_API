using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Extrato_API.Models
{
    [Table("tb_alternativa")]
    public class Alternativa
    {
        [Key]
        [Column("id_alternativa")]
        public int Id { get; set; }

        [Required]
        [Column("atividade_id")]
        public int AtividadeId { get; set; }

        [Required]
        [Column("texto")]
        public string Texto { get; set; } = string.Empty;

        [Column("correta")]
        public bool? Correta { get; set; } = false;

        [Required]
        [Column("ordem")]
        public int Ordem { get; set; }

        // Relacionamentos
        [ForeignKey("AtividadeId")]
        public Atividade? Atividade { get; set; }
    }
}