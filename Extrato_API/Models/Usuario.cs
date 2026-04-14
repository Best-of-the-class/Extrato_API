using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Extrato_API.Models
{
    [Table("tb_usuario")]
    public class Usuario
    {
        // Id único para cada usuário
        [Key]
        [Column("id_usuario")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("nome")]
        public string NomeUsuario { get; set; } = string.Empty;

        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Column("senha")]
        public string SenhaHash { get; set; } = string.Empty;

        [Column("tipo_usuario")]
        public string TipoUsuario { get; set; } = string.Empty;

        [Column("criado_em")]
        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

        //Recuperação de senha
        public string? CodigoResetSenha { get; set; }
        public DateTime? CodigoResetExpiracao { get; set; }

        //Avatar
        //public int? AvatarId { get; set; }
    }
}