namespace Extrato_API.DTOs
{
    public class XpAtividadeDTO
    {
        public int AtividadeId { get; set; }
        public bool Correta { get; set; }
        public int XpGanho { get; set; }
        public bool JaPontuadaAnteriormente { get; set; }
    }

    public class ResultadoAtribuicaoXpDTO
    {
        public int Acertos { get; set; }
        public int Erros { get; set; }
        public int XpGanhoTotal { get; set; }
        public int XpTotalUsuario { get; set; }
        public int ExerciciosProcessados { get; set; }
        public List<XpAtividadeDTO> Atividades { get; set; } = new();
    }
}