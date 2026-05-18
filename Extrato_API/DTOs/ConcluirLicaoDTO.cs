using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Extrato_API.DTOs
{
    public class ConcluirLicaoDTO
    {
        [Required]
        public Guid EstudanteId { get; set; }

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
