namespace Extrato_API.DTOs
{
    public class ConquistaDTO
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string Icone { get; set; } = string.Empty;
        public string TipoDesbloqueio { get; set; } = string.Empty;
        public string BackgroundCor { get; set; } = string.Empty;
        public bool Desbloqueada { get; set; }
        public DateTime? ConquistadoEm { get; set; }
    }

    public class ConquistasUsuarioResultadoDTO
    {
        public List<ConquistaDTO> Conquistas { get; set; } = new();
        public List<ConquistaDTO> NovasConquistas { get; set; } = new();
    }
}