using Extrato_API.Data;
using Extrato_API.DTOs;
using Extrato_API.Models;

namespace Extrato_API.Services.Implementations
{
    public class XpService
    {
        private readonly AppDbContext _context;

        public XpService(AppDbContext context)
        {
            _context = context;
        }

        public ResultadoAtribuicaoXpDto ProcessarRespostas(Estudante estudante, Licao licao, IReadOnlyCollection<RespostaDto> respostas)
        {
            var resultado = new ResultadoAtribuicaoXpDto();
            var atividadesDaLicao = (licao.Atividades ?? new List<Atividade>())
                .ToDictionary(atividade => atividade.Id);
            var xpPlanejadoPorAtividade = CalcularXpPlanejadoPorAtividade(licao, atividadesDaLicao.Values.ToList());

            var atividadesRespondidas = respostas
                .Select(resposta => resposta.AtividadeId)
                .Distinct()
                .ToList();

            var atividadesJaPontuadas = _context.Tentativas
                .Where(tentativa =>
                    (tentativa.EstudanteId == estudante.Id || tentativa.EstudanteId == estudante.UsuarioId) &&
                    atividadesRespondidas.Contains(tentativa.AtividadeId) &&
                    tentativa.Correta &&
                    (tentativa.XpGanho ?? 0) > 0)
                .Select(tentativa => tentativa.AtividadeId)
                .Distinct()
                .ToHashSet();

            foreach (var resposta in respostas)
            {
                atividadesDaLicao.TryGetValue(resposta.AtividadeId, out var atividade);

                var correta = false;

                if (atividade != null)
                {
                    var alternativaEscolhida = atividade.Alternativas
                        .FirstOrDefault(alternativa => alternativa.Id == resposta.AlternativaEscolhidaId);

                    correta = alternativaEscolhida?.Correta == true;
                }

                if (correta)
                    resultado.Acertos++;
                else
                    resultado.Erros++;

                // Verifica se a atividade já foi pontuada anteriormente para evitar duplicidade de XP
                bool jaPontuadaAnteriormente = atividade != null && atividadesJaPontuadas.Contains(atividade.Id);
                // Só pontua se a resposta estiver correta, a atividade existir e não tiver sido pontuada antes
                bool devePontuar = correta && atividade != null && !jaPontuadaAnteriormente;
                int xpGanho = 0;

                // SonarCloud: esta condição depende de três variáveis dinâmicas, não é sempre verdadeira
                if (devePontuar)
                {
                    if (atividade != null && xpPlanejadoPorAtividade.ContainsKey(atividade.Id))
                    {
                        xpGanho = xpPlanejadoPorAtividade[atividade.Id];
                        atividadesJaPontuadas.Add(atividade.Id);
                    }
                }

                _context.Tentativas.Add(new Tentativa
                {
                    EstudanteId = estudante.Id,
                    AtividadeId = resposta.AtividadeId,
                    AlternativaEscolhidaId = resposta.AlternativaEscolhidaId,
                    Correta = correta,
                    XpGanho = xpGanho,
                    TentadoEm = DateTime.UtcNow
                });

                resultado.XpGanhoTotal += xpGanho;
                resultado.Atividades.Add(new XpAtividadeDto
                {
                    AtividadeId = resposta.AtividadeId,
                    Correta = correta,
                    XpGanho = xpGanho,
                    JaPontuadaAnteriormente = jaPontuadaAnteriormente
                });
            }

            resultado.ExerciciosProcessados = resultado.Acertos + resultado.Erros;
            estudante.XpTotal += resultado.XpGanhoTotal;
            estudante.ExerciciosResolvidos += resultado.ExerciciosProcessados;
            resultado.XpTotalUsuario = estudante.XpTotal;

            return resultado;
        }

        private static Dictionary<int, int> CalcularXpPlanejadoPorAtividade(Licao licao, IReadOnlyCollection<Atividade> atividades)
        {
            var distribuicao = new Dictionary<int, int>();

            if (atividades.Count == 0)
                return distribuicao;

            var xpTotalDaLicao = ObterXpTotalDaLicao(licao, atividades.Count);
            var pesos = atividades
                .Select(atividade => new
                {
                    atividade.Id,
                    Peso = ObterPesoDaAtividade(atividade)
                })
                .ToList();

            var somaPesos = pesos.Sum(item => item.Peso);

            if (somaPesos <= 0)
            {
                var xpUniforme = xpTotalDaLicao / atividades.Count;
                var resto = xpTotalDaLicao % atividades.Count;

                foreach (var item in pesos.OrderBy(item => item.Id))
                {
                    distribuicao[item.Id] = xpUniforme + (resto > 0 ? 1 : 0);
                    if (resto > 0)
                        resto--;
                }

                return distribuicao;
            }

            var parcelas = pesos
                .Select(item =>
                {
                    var bruto = xpTotalDaLicao * (item.Peso / somaPesos);
                    var inteiro = (int)Math.Floor(bruto);

                    return new ParcelaXp
                    {
                        AtividadeId = item.Id,
                        Xp = inteiro,
                        Fracao = bruto - inteiro
                    };
                })
                .ToList();

            var restante = xpTotalDaLicao - parcelas.Sum(parcela => parcela.Xp);

            foreach (var parcela in parcelas
                         .OrderByDescending(parcela => parcela.Fracao)
                         .ThenBy(parcela => parcela.AtividadeId)
                         .Take(restante))
            {
                parcela.Xp += 1;
            }

            foreach (var parcela in parcelas)
                distribuicao[parcela.AtividadeId] = parcela.Xp;

            return distribuicao;
        }

        private static int ObterXpTotalDaLicao(Licao licao, int quantidadeAtividades)
        {
            if (licao.RecompensaXp > 0)
                return licao.RecompensaXp;

            return Math.Max(10, quantidadeAtividades * 10);
        }

        private static decimal ObterPesoDaAtividade(Atividade atividade)
        {
            var peso = atividade.Dificuldade switch
            {
                <= 1 => 1.00m,
                2 => 1.25m,
                _ => 1.50m
            };

            if (atividade.ProvaFinal == true)
                peso += 0.25m;

            return peso;
        }

        private sealed class ParcelaXp
        {
            public int AtividadeId { get; set; }
            public int Xp { get; set; }
            public decimal Fracao { get; set; }
        }
    }
}