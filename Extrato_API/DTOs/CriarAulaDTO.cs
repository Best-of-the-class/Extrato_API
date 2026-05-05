using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Extrato_API.DTOs
{
    public class CriarAulaDTO
    {
        [Required]
        [JsonPropertyName("dificuldade")]
        public int Dificuldade { get; set; } //Módulo

        [Required]
        public string TituloLicao { get; set; } = string.Empty;

        public string? TextoConceito { get; set; } //Teoria

        [Required(ErrorMessage = "A lista de questões é obrigatória.")]
        [MinLength(4, ErrorMessage = "Operação negada: A aula precisa ter exatamente 4 questões.")]
        [MaxLength(4, ErrorMessage = "Operação negada: A aula precisa ter exatamente 4 questões.")]
        public List<CriarQuestaoDTO> Questoes { get; set; } = new List<CriarQuestaoDTO>();
    }

    //Estrutura individual de cada uma das 4 questões
    public class CriarQuestaoDTO
    {
        [Required]
        public string Enunciado { get; set; } = string.Empty;

        [Required]
        [MinLength(3, ErrorMessage = "Cada questão precisa ter exatamente 3 alternativas.")]
        [MaxLength(3, ErrorMessage = "Cada questão precisa ter exatamente 3 alternativas.")]
        public List<string> Alternativas { get; set; } = new List<string>();

        [Required]
        public int IndiceCorreta { get; set; }
    }
}