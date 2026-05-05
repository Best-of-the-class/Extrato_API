using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Extrato_API.Models
{
    [Table("tb_atividade")]
    public class Atividade
    {
        [Key]
        [Column("id_atividade")]
        public int Id { get; set; }

        [Required]
        [Column("licao_id")]
        public int LicaoId { get; set; }

        [Required]
        [Column("enunciado")]
        public string Enunciado { get; set; } = string.Empty;

        [Required]
        [Column("dificuldade")]
        public int Dificuldade { get; set; } // 1, 2 ou 3

        [Required]
        [Column("ordem")]
        public int Ordem { get; set; }

        [Column("prova_final")]
        public bool? ProvaFinal { get; set; } = false;

        // Relacionamentos
        [ForeignKey("LicaoId")]
        public Licao? Licao { get; set; }

        public List<Alternativa> Alternativas { get; set; } = new List<Alternativa>();
    }
}