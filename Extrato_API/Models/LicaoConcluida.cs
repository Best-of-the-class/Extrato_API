using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Extrato_API.Models
{
    [Table("tb_licao_concluida")]
    public class LicaoConcluida
    {
        [Key]
        [Column("id_licao_concluida")]
        public int Id { get; set; }

        [Column("id_licao")]
        public int LicaoId { get; set; }

        [Column("id_usuario")]

        public Guid UsuarioId { get; set; }

        [Column("concluido_em")]
        public DateTime ConcluidoEm { get; set; } = DateTime.UtcNow;
    }
}
