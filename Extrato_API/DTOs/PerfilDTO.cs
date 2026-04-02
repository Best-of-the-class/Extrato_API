using System.ComponentModel.DataAnnotations;

namespace Extrato_API.DTOs
{
    // GET /api/perfil — buscar perfil completo
    public class ObterPerfilDTO
    {
        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        public string Email { get; set; } = string.Empty;
    }

    // PUT /api/perfil/editar — salvar todas as alterações do perfil (nome + avatar)
    public class EditarPerfilDTO
    {
        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "O novo nome é obrigatório.")]
        public string NovoNome { get; set; } = string.Empty;

        // AvatarId é opcional — null significa manter o atual
        public int? AvatarId { get; set; }
    }
}
