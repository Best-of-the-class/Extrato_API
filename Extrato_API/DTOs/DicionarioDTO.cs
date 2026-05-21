using System.ComponentModel.DataAnnotations;

namespace Extrato_API.DTOs
{
    public class CriarDicionarioDTO
    {
        [Required(ErrorMessage = "O termo é obrigatório.")]
        public string Termo { get; set; } = string.Empty;

        [Required(ErrorMessage = "A definição é obrigatória.")]
        public string Definicao { get; set; } = string.Empty;
    }

    public class AtualizarDicionarioDTO
    {
        [Required(ErrorMessage = "O termo é obrigatório.")]
        public string Termo { get; set; } = string.Empty;

        [Required(ErrorMessage = "A definição é obrigatória.")]
        public string Definicao { get; set; } = string.Empty;
    }
}