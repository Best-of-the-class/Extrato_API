using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Extrato_API.Models
{
    [Table("tb_conquista_estudante")]
    public class ConquistaEstudante
    {
        [Key]
        [Column("id_conquista_estudante")]
        public int Id { get; set; }

        [Required]
        [Column("estudante_id")]
        public Guid EstudanteId { get; set; }

        [Required]
        [Column("id_conquista")]
        public int ConquistaId { get; set; }

        [Required]
        [Column("conquistado_em")]
        public DateTime ConquistadoEm { get; set; } = DateTime.UtcNow;
    }
}