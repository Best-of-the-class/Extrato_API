using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Extrato_API.Data;
using Extrato_API.Models;
using Extrato_API.DTOs;
using System;
using System.Linq;

namespace Extrato_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize(Roles = "admin")] -> feature futura de segurança
    public class LicaoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LicaoController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("criar")]
        public IActionResult CriarAulaCompleta([FromBody] CriarAulaDTO dto)
        {
            var modulo = _context.Modulos.FirstOrDefault(m => m.Nivel == dto.Dificuldade);

            if (modulo == null)
            {
                modulo = new Modulo { Titulo = $"Módulo Nível {dto.Dificuldade}", Nivel = dto.Dificuldade };
                _context.Modulos.Add(modulo);
                _context.SaveChanges();
            }

            int novaOrdem = _context.Licoes.Count(l => l.ModuloId == modulo.Id) + 1;

            var novaLicao = new Licao
            {
                ModuloId = modulo.Id,
                Titulo = dto.TituloLicao,
                Ordem = novaOrdem,
                RecompensaXp = 50,
                TituloConceito = "Teoria: " + dto.TituloLicao,
                TextoConceito = dto.TextoConceito
            };

            int ordemQuestao = 1;
            foreach (var questaoDto in dto.Questoes)
            {
                var novaAtividade = new Atividade
                {
                    Enunciado = questaoDto.Enunciado,
                    Dificuldade = dto.Dificuldade,
                    Ordem = ordemQuestao,
                    ProvaFinal = false
                };

                for (int i = 0; i < questaoDto.Alternativas.Count; i++)
                {
                    var novaAlternativa = new Alternativa
                    {
                        Texto = questaoDto.Alternativas[i],
                        Correta = (i == questaoDto.IndiceCorreta),
                        Ordem = i + 1
                    };
                    novaAtividade.Alternativas.Add(novaAlternativa);
                }

                novaLicao.Atividades.Add(novaAtividade);
                ordemQuestao++;
            }

            _context.Licoes.Add(novaLicao);
            _context.SaveChanges();

            return Ok(new
            {
                Sucesso = true,
                Mensagem = "Aula criada com sucesso no Poupas!",
                LicaoId = novaLicao.Id
            });
        }

        [HttpGet("listar")]
        public IActionResult ListarAulas()
        {
            var trilha = _context.Modulos
                .Include(m => m.Licoes)
                .Select(m => new
                {
                    Dificuldade = m.Nivel,
                    Licoes = m.Licoes.OrderBy(l => l.Ordem).Select(l => l.Titulo).ToList()
                })
                .ToList();

            return Ok(trilha);
        }

        [HttpDelete("deletar/{titulo}")]
        public IActionResult DeletarAula(string titulo)
        {
            var licao = _context.Licoes.FirstOrDefault(l => l.Titulo == titulo);

            if (licao == null)
            {
                return NotFound(new { Mensagem = "Lição não encontrada no banco de dados." });
            }

            _context.Licoes.Remove(licao);
            _context.SaveChanges();

            return Ok(new { Sucesso = true, Mensagem = "Aula deletada com sucesso!" });
        }

        [HttpGet("detalhes/{titulo}")]
        public IActionResult BuscarDetalhesAula(string titulo)
        {
            var licao = _context.Licoes
                .Include(l => l.Modulo)
                .Include(l => l.Atividades)
                    .ThenInclude(a => a.Alternativas)
                .FirstOrDefault(l => l.Titulo == titulo);

            if (licao == null)
                return NotFound(new { Mensagem = "Lição não encontrada." });

            var questoesFormatadas = new List<object>();

            foreach (var atividade in licao.Atividades.OrderBy(a => a.Ordem))
            {
                var alternativasOrdenadas = atividade.Alternativas.OrderBy(alt => alt.Ordem).ToList();

                int indCorreta = alternativasOrdenadas.FindIndex(alt => alt.Correta == true);

                questoesFormatadas.Add(new
                {
                    atividadeId = atividade.Id,
                    enunciado = atividade.Enunciado,
                    indiceCorreta = indCorreta,
                    alternativas = alternativasOrdenadas.Select(alt => alt.Texto).ToList(),
                    alternativasIds = alternativasOrdenadas.Select(alt => alt.Id).ToList()
                });
            }

            var result = new
            {
                licaoId = licao.Id,
                dificuldade = licao.Modulo?.Nivel ?? 0,
                tituloLicao = licao.Titulo,
                textoConceito = licao.TextoConceito ?? string.Empty,
                questoes = questoesFormatadas
            };

            return Ok(result);
        }

        [HttpPost("concluir")]
        public IActionResult ConcluirLicao([FromBody] ConcluirLicaoDTO dto)
        {
            var licao = _context.Licoes
                .Include(l => l.Atividades)
                    .ThenInclude(a => a.Alternativas)
                .FirstOrDefault(l => l.Id == dto.LicaoId);

            if (licao == null)
                return NotFound(new { Mensagem = "Lição não encontrada." });

            int acertos = 0;
            int erros = 0;
            int xpTotal = 0;

            int totalAtividades = licao.Atividades?.Count ?? 0;
            int xpPorAcerto = 100;
            if (licao.RecompensaXp > 0 && totalAtividades > 0)
            {
                xpPorAcerto = Math.Max(1, licao.RecompensaXp / totalAtividades);
            }

            foreach (var resposta in dto.Respostas)
            {
                var atividade = licao.Atividades.FirstOrDefault(a => a.Id == resposta.AtividadeId);
                if (atividade == null)
                {
                    erros++;
                    var tentativaErrada = new Tentativa
                    {
                        EstudanteId = dto.EstudanteId,
                        AtividadeId = resposta.AtividadeId,
                        AlternativaEscolhidaId = resposta.AlternativaEscolhidaId,
                        Correta = false,
                        XpGanho = 0,
                        TentadoEm = DateTime.UtcNow
                    };
                    _context.Tentativas.Add(tentativaErrada);
                    continue;
                }

                var alternativaEscolhida = atividade.Alternativas.FirstOrDefault(a => a.Id == resposta.AlternativaEscolhidaId);
                bool correta = alternativaEscolhida != null && alternativaEscolhida.Correta == true;

                if (correta) acertos++; else erros++;

                int xpGanho = correta ? xpPorAcerto : 0;
                xpTotal += xpGanho;

                var tentativa = new Tentativa
                {
                    EstudanteId = dto.EstudanteId,
                    AtividadeId = atividade.Id,
                    AlternativaEscolhidaId = resposta.AlternativaEscolhidaId,
                    Correta = correta,
                    XpGanho = xpGanho,
                    TentadoEm = DateTime.UtcNow
                };

                _context.Tentativas.Add(tentativa);
            }

            var stats = _context.EstatisticasUsuarios.FirstOrDefault(s => s.UsuarioId == dto.EstudanteId);
            if (stats == null)
            {
                stats = new EstatisticasUsuario { UsuarioId = dto.EstudanteId };
                _context.EstatisticasUsuarios.Add(stats);
            }

            stats.ExerciciosResolvidos += (acertos + erros);
            stats.Pontuacao += xpTotal;

            bool jaConcluiu = _context.LicaoConcluidas.Any(lc => lc.UsuarioId == dto.EstudanteId && lc.LicaoId == licao.Id);
            bool ganhouSequencia = false;
            if (!jaConcluiu)
            {
                var registro = new LicaoConcluida
                {
                    UsuarioId = dto.EstudanteId,
                    LicaoId = licao.Id,
                    ConcluidoEm = DateTime.UtcNow
                };
                _context.LicaoConcluidas.Add(registro);

                stats.LicoesConcluidas += 1;

                if (erros == 0 && (acertos + erros) > 0)
                {
                    stats.SequenciaDias += 1;
                    ganhouSequencia = true;
                }
            }

            _context.SaveChanges();

            return Ok(new
            {
                acertos,
                erros,
                xp = xpTotal,
                ganhouSequencia
            });
        }

        [HttpPut("editar/{tituloAntigo}")]
        public IActionResult EditarAula(string tituloAntigo, [FromBody] CriarAulaDTO dto)
        {
            var licao = _context.Licoes
                .Include(l => l.Atividades)
                    .ThenInclude(a => a.Alternativas)
                .FirstOrDefault(l => l.Titulo == tituloAntigo);

            if (licao == null)
                return NotFound(new { Mensagem = "Lição original não encontrada para edição." });

            licao.Titulo = dto.TituloLicao;
            licao.TextoConceito = dto.TextoConceito;

            var modulo = _context.Modulos.FirstOrDefault(m => m.Nivel == dto.Dificuldade);
            if (modulo != null)
            {
                licao.ModuloId = modulo.Id;
            }

            _context.Atividades.RemoveRange(licao.Atividades);

            int ordemQuestao = 1;
            var novasAtividades = new List<Atividade>();
            foreach (var questaoDto in dto.Questoes)
            {
                var novaAtividade = new Atividade
                {
                    Enunciado = questaoDto.Enunciado,
                    Dificuldade = dto.Dificuldade,
                    Ordem = ordemQuestao,
                    ProvaFinal = false
                };

                for (int i = 0; i < questaoDto.Alternativas.Count; i++)
                {
                    var novaAlternativa = new Alternativa
                    {
                        Texto = questaoDto.Alternativas[i],
                        Correta = (i == questaoDto.IndiceCorreta),
                        Ordem = i + 1
                    };
                    novaAtividade.Alternativas.Add(novaAlternativa);
                }

                novasAtividades.Add(novaAtividade);
                ordemQuestao++;
            }

            licao.Atividades = novasAtividades;

            _context.SaveChanges();

            return Ok(new { Sucesso = true, Mensagem = "Aula editada com sucesso!" });
        }
    }
}