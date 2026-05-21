using System.ComponentModel.DataAnnotations;

namespace Extrato_API.DTOs
{ //remove EstudanteId que vem do JWT
    public class ConcluirLicaoDTO
    {
        [Required]
        public int LicaoId { get; set; }

        [Required]
        public List<RespostaDTO> Respostas { get; set; } = new List<RespostaDTO>();
    }

    public class RespostaDTO
    {
        [Required]
        public int AtividadeId { get; set; }

        public int? AlternativaEscolhidaId { get; set; }
    }
}
