using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Extrato_API.Models
{
    [Table("tb_reset_senha")]
    public class ResetSenha
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Column("codigo")]
        public string Codigo { get; set; } = string.Empty;

        [Column("expiracao")]
        public DateTime Expiracao { get; set; }
    }
}