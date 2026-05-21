using System.ComponentModel.DataAnnotations;

namespace Extrato_API.DTOs
{
    // ID 18 — Deletar conta
    public class DeletarContaDTO
    {
        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória.")]
        public string Senha { get; set; } = string.Empty;
    }

    // ID 21 — Alterar avatar
    public class AlterarAvatarDTO
    { //retirei o e-mail que antes era obrigatório
        [Required(ErrorMessage = "O avatarId é obrigatório.")]
        public int AvatarId { get; set; }
    }
}