using Extrato_API.Data;
using Extrato_API.DTOs;
using Extrato_API.Models;

namespace Extrato_API.Services.Implementations
{
    public class ConquistaService
    {
        private readonly AppDbContext _context;

        public ConquistaService(AppDbContext context)
        {
            _context = context;
        }

        public ConquistasUsuarioResultadoDTO AvaliarConquistas(Guid usuarioId, Estudante estudante)
        {
            var catalogo = _context.Conquistas
                .OrderBy(c => c.Id)
                .ToList();

            var conquistasExistentes = _context.ConquistasEstudante
                .Where(ce => ce.EstudanteId == estudante.Id)
                .ToList();

            var mapaConquistas = conquistasExistentes
                .GroupBy(ce => ce.ConquistaId)
                .ToDictionary(grupo => grupo.Key, grupo => grupo.First());

            var respostasCorretas = _context.Tentativas.Count(t =>
                (t.EstudanteId == estudante.Id || t.EstudanteId == estudante.UsuarioId) &&
                t.Correta);

            var datasLicoesConcluidas = _context.LicaoConcluidas
                .Where(lc => lc.UsuarioId == usuarioId)
                .Select(lc => lc.ConcluidoEm)
                .ToList();

            var temMaratona = datasLicoesConcluidas
                .GroupBy(data => data.Date)
                .Any(grupo => grupo.Count() >= 5);

            var agora = DateTime.UtcNow;
            var novasConquistas = new List<ConquistaEstudante>();

            foreach (var conquista in catalogo)
            {
                if (mapaConquistas.ContainsKey(conquista.Id) ||
                    !EstaElegivel(conquista, estudante, respostasCorretas, temMaratona))
                {
                    continue;
                }

                var conquistaEstudante = new ConquistaEstudante
                {
                    EstudanteId = estudante.Id,
                    ConquistaId = conquista.Id,
                    ConquistadoEm = agora
                };

                _context.ConquistasEstudante.Add(conquistaEstudante);
                conquistasExistentes.Add(conquistaEstudante);
                mapaConquistas[conquista.Id] = conquistaEstudante;
                novasConquistas.Add(conquistaEstudante);
            }

            var todas = catalogo
                .Select(conquista => CriarDto(conquista, mapaConquistas))
                .ToList();

            var novas = catalogo
                .Where(conquista => novasConquistas.Any(nova => nova.ConquistaId == conquista.Id))
                .Select(conquista => CriarDto(conquista, mapaConquistas))
                .ToList();

            return new ConquistasUsuarioResultadoDTO
            {
                Conquistas = todas,
                NovasConquistas = novas
            };
        }

        public IReadOnlyCollection<ConquistaDTO> ObterConquistas(Estudante estudante)
        {
            var catalogo = _context.Conquistas
                .OrderBy(c => c.Id)
                .ToList();

            var mapaConquistas = _context.ConquistasEstudante
                .Where(ce => ce.EstudanteId == estudante.Id)
                .ToList()
                .GroupBy(ce => ce.ConquistaId)
                .ToDictionary(grupo => grupo.Key, grupo => grupo.First());

            return catalogo
                .Select(conquista => CriarDto(conquista, mapaConquistas))
                .ToList();
        }

        private static bool EstaElegivel(
            Conquista conquista,
            Estudante estudante,
            int respostasCorretas,
            bool temMaratona)
        {
            var titulo = Normalizar(conquista.Titulo);

            if (titulo == "primeiros passos")
                return estudante.LicoesConcluidas >= 1;

            if (titulo == "poup aprendiz")
                return respostasCorretas >= 10;

            if (titulo == "megamente")
                return respostasCorretas >= 50;

            if (titulo == "maratonista")
                return temMaratona;

            if (titulo == "poupador")
                return estudante.XpTotal >= 1500;

            return false;
        }

        private static ConquistaDTO CriarDto(Conquista conquista, IReadOnlyDictionary<int, ConquistaEstudante> mapaConquistas)
        {
            mapaConquistas.TryGetValue(conquista.Id, out var conquistaEstudante);

            return new ConquistaDTO
            {
                Id = conquista.Id,
                Titulo = conquista.Titulo,
                Descricao = conquista.Descricao,
                Icone = conquista.Icone,
                TipoDesbloqueio = conquista.TipoDesbloqueio,
                BackgroundCor = conquista.BackgroundCor,
                Desbloqueada = conquistaEstudante != null,
                ConquistadoEm = conquistaEstudante?.ConquistadoEm
            };
        }

        private static string Normalizar(string valor)
        {
            return valor.Trim().ToLowerInvariant();
        }
    }
}