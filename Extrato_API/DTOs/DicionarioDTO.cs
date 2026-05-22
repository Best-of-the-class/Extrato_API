using System.ComponentModel.DataAnnotations;

namespace Extrato_API.DTOs
{
    public class CriarDicionarioDto
    {
        [Required(ErrorMessage = "O termo é obrigatório.")]
        public string Termo { get; set; } = string.Empty;

        [Required(ErrorMessage = "A definição é obrigatória.")]
        public string Definicao { get; set; } = string.Empty;
    }

    public class AtualizarDicionarioDto
    {
        [Required(ErrorMessage = "O termo é obrigatório.")]
        public string Termo { get; set; } = string.Empty;

        [Required(ErrorMessage = "A definição é obrigatória.")]
        public string Definicao { get; set; } = string.Empty;
    }
}