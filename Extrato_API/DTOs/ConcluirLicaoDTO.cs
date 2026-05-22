using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Extrato_API.DTOs
{ //remove EstudanteId que vem do JWT
    public class ConcluirLicaoDto
    {
        [JsonRequired]
        [Range(1, int.MaxValue)]
        public int LicaoId { get; set; }

        [Required]
        public List<RespostaDto> Respostas { get; set; } = new();
    }

    public class RespostaDto
    {
        [JsonRequired]
        [Range(1, int.MaxValue)]
        public int AtividadeId { get; set; }

        public int? AlternativaEscolhidaId { get; set; }
    }
}
