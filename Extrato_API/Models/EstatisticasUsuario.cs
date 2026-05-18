namespace Extrato_API.Models
{
    public class EstatisticasUsuario
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UsuarioId { get; set; }

        public int LicoesConcluidas { get; set; } = 0;

        public int ExerciciosResolvidos { get; set; } = 0;

        public int Pontuacao { get; set; } = 0;

        public int SequenciaDias { get; set; } = 0;
    }
}