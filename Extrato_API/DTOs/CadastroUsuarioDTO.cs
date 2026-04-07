using System.ComponentModel.DataAnnotations;

namespace Extrato_API.DTOs
{
    public class CadastroUsuarioDTO
    {
        [Required(ErrorMessage = "O nome de usuário é obrigatório.")]
        public string NomeUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Email inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória.")]
        [MinLength(8, ErrorMessage = "Senha deve conter no mínimo 8 caracteres")]
        public string Senha { get; set; } = string.Empty;

        [Required(ErrorMessage = "A confirmação de senha é obrigatória.")]
        [Compare("Senha", ErrorMessage = "As senhas não coincidem.")]
        public string ConfirmarSenha { get; set; } = string.Empty;
    }
}