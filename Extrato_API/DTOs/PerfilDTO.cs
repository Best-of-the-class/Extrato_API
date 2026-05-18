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

    // PUT /api/perfil/editar — salvar alterações de nome e e-mail
    public class EditarPerfilDTO
    {
        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "O novo nome é obrigatório.")]
        public string NovoNome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O novo e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Novo e-mail inválido.")]
        public string NovoEmail { get; set; } = string.Empty;
    }
}
