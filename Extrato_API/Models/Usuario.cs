namespace Extrato_API.Models
{
    public class Usuario
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string NomeUsuario { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string SenhaHash { get; set; } = string.Empty;

        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

        //Recuperação de senha
        public string? CodigoResetSenha { get; set; }
        public DateTime? CodigoResetExpiracao { get; set; }

        //Avatar
        //public int? AvatarId { get; set; }
    }
}
