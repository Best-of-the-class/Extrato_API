using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Extrato_API.Models
{
    [Table("tb_conquista")]
    public class Conquista
    {
        [Key]
        [Column("id_conquista")]
        public int Id { get; set; }

        [Required]
        [Column("titulo")]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        [Column("descricao")]
        public string Descricao { get; set; } = string.Empty;

        [Required]
        [Column("icone")]
        public string Icone { get; set; } = string.Empty;

        [Required]
        [Column("tipo_desbloqueio")]
        public string TipoDesbloqueio { get; set; } = string.Empty;

        [Required]
        [Column("background_cor")]
        public string BackgroundCor { get; set; } = string.Empty;
    }
}