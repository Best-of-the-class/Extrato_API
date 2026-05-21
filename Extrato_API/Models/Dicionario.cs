using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Extrato_API.Models
{
    [Table("tb_dicionario")]
    public class Dicionario
    {
        [Key]
        [Column("id_dicionario")]
        public int Id { get; set; }

        [Required]
        [Column("termo")]
        public string Termo { get; set; } = string.Empty;

        [Required]
        [Column("definicao")]
        public string Definicao { get; set; } = string.Empty;

        [Column("criado_em")]
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    }
}