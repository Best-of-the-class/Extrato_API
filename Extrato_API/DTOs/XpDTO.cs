namespace Extrato_API.DTOs
{
    public class XpAtividadeDto
    {
        public int AtividadeId { get; set; }
        public bool Correta { get; set; }
        public int XpGanho { get; set; }
        public bool JaPontuadaAnteriormente { get; set; }
    }

    public class ResultadoAtribuicaoXpDto
    {
        public int Acertos { get; set; }
        public int Erros { get; set; }
        public int XpGanhoTotal { get; set; }
        public int XpTotalUsuario { get; set; }
        public int ExerciciosProcessados { get; set; }
        public List<XpAtividadeDto> Atividades { get; set; } = new();
    }
}