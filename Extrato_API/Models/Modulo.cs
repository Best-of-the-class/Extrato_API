using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Extrato_API.Models
{
    [Table("tb_modulo")]
    public class Modulo
    {
        [Key]
        [Column("id_modulo")]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Column("titulo")]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        [Column("nivel")]
        public int Nivel { get; set; } // 1, 2 ou 3

        // Relacionamento (Um Módulo tem Várias Lições)
        public List<Licao> Licoes { get; set; } = new List<Licao>();
    }
}
