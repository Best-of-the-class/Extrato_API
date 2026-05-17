using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Extrato_API.Models
{
    [Table("tb_avatar")]
    public class Avatar
    {
        [Key]
        [Column("id_avatar")]
        public int Id { get; set; }

        [Column("url_imagem")]
        public string UrlImagem { get; set; } = string.Empty;
    }
}