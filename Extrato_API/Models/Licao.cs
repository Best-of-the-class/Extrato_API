using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Extrato_API.Models
{
    [Table("tb_licao")]
    public class Licao
    {
        [Key]
        [Column("id_licao")]
        public int Id { get; set; }

        [Required]
        [Column("modulo_id")]
        public int ModuloId { get; set; }

        [Required]
        [StringLength(200)]
        [Column("titulo")]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        [Column("ordem")]
        public int Ordem { get; set; }

        [Required]
        [Column("recompensa_xp")]
        public int RecompensaXp { get; set; }

        [StringLength(200)]
        [Column("titulo_conceito")]
        public string? TituloConceito { get; set; } // Opcional (null)

        [Column("texto_conceito")]
        public string? TextoConceito { get; set; } // Opcional (null)

        // Relacionamentos
        [ForeignKey("ModuloId")]
        public Modulo? Modulo { get; set; }

        public List<Atividade> Atividades { get; set; } = new List<Atividade>();
    }
}
