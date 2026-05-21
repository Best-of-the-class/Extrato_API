using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Extrato_API.Data;
using Extrato_API.Models;
using Extrato_API.DTOs;
using Extrato_API.Extensions;
using Extrato_API.Services.Implementations;

namespace Extrato_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LicaoController : ControllerBase
    {
        private const string InvalidAnswersMessage = "Uma ou mais respostas não pertencem à lição informada.";

        private readonly AppDbContext _context;
        private readonly ConquistaService _conquistaService;
        private readonly XpService _xpService;

        public LicaoController(AppDbContext context, ConquistaService conquistaService, XpService xpService)
        {
            _context = context;
            _conquistaService = conquistaService;
            _xpService = xpService;
        }

        //protegido com [Authorize(Roles = "admin")]
        [HttpPost("criar")]
        [Authorize(Roles = "admin")]
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
                    novaAtividade.Alternativas.Add(new Alternativa
                    {
                        Texto = questaoDto.Alternativas[i],
                        Correta = (i == questaoDto.IndiceCorreta),
                        Ordem = i + 1
                    });
                }

                novaLicao.Atividades.Add(novaAtividade);
                ordemQuestao++;
            }

            _context.Licoes.Add(novaLicao);
            _context.SaveChanges();

            return Ok(new { Sucesso = true, Mensagem = "Aula criada com sucesso no Poupas!", LicaoId = novaLicao.Id });
        }

        [HttpPut("editar/{tituloAntigo}")]
        [Authorize(Roles = "admin")]
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
                licao.ModuloId = modulo.Id;

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
                    novaAtividade.Alternativas.Add(new Alternativa
                    {
                        Texto = questaoDto.Alternativas[i],
                        Correta = (i == questaoDto.IndiceCorreta),
                        Ordem = i + 1
                    });
                }

                novasAtividades.Add(novaAtividade);
                ordemQuestao++;
            }

            licao.Atividades = novasAtividades;
            _context.SaveChanges();

            return Ok(new { Sucesso = true, Mensagem = "Aula editada com sucesso!" });
        }

        [HttpDelete("deletar/{titulo}")]
        [Authorize(Roles = "admin")]
        public IActionResult DeletarAula(string titulo)
        {
            var licao = _context.Licoes
                .Include(l => l.Atividades)
                    .ThenInclude(a => a.Alternativas)
                .FirstOrDefault(l => l.Titulo == titulo);

            if (licao == null)
                return NotFound(new { Mensagem = "Lição não encontrada no banco de dados." });

            _context.Licoes.Remove(licao);
            _context.SaveChanges();

            return Ok(new { Sucesso = true, Mensagem = "Aula deletada com sucesso!" });
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

        //requer autenticação e NÃO retorna gabarito
        [HttpGet("detalhes/{titulo}")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public IActionResult BuscarDetalhesAula(string titulo)
        {
            var licao = _context.Licoes
                .Include(l => l.Modulo)
                .Include(l => l.Atividades)
                    .ThenInclude(a => a.Alternativas)
                .FirstOrDefault(l => l.Titulo == titulo);

            if (licao == null)
                return NotFound(new { Mensagem = "Lição não encontrada." });

            var questoesFormatadas = licao.Atividades.OrderBy(a => a.Ordem).Select(atividade =>
            {
                var (alternativas, alternativasIds) = ObterAlternativasOrdenadas(atividade);
                return new
                {
                    atividadeId = atividade.Id,
                    enunciado = atividade.Enunciado,
                    // gabarito removido — não retorna indiceCorreta
                    alternativas,
                    alternativasIds
                };
            }).ToList<object>();

            return Ok(new
            {
                licaoId = licao.Id,
                dificuldade = licao.Modulo?.Nivel ?? 0,
                tituloLicao = licao.Titulo,
                textoConceito = licao.TextoConceito ?? string.Empty,
                questoes = questoesFormatadas
            });
        }

        //usa usuarioId do JWT, remove EstudanteId do DTO
        [HttpPost("concluir")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public IActionResult ConcluirLicao([FromBody] ConcluirLicaoDto dto)
        {
            if (!this.TryGetAuthenticatedUserId(out var usuarioId))
                return this.UserNotAuthenticated();

            var licao = _context.Licoes
                .Include(l => l.Atividades)
                    .ThenInclude(a => a.Alternativas)
                .FirstOrDefault(l => l.Id == dto.LicaoId);

            if (licao == null)
                return NotFound(new { Mensagem = "Lição não encontrada." });

            var estudante = ObterEstudante(usuarioId);
            if (estudante == null)
                return this.StudentProfileNotFound();

            if (estudante.QuantVidas <= 0)
                return BadRequest(new { Sucesso = false, Mensagem = "Sem vidas restantes. Aguarde a recarga." });

            if (TemRespostaInvalida(licao, dto.Respostas))
                return BadRequest(new { Sucesso = false, Mensagem = InvalidAnswersMessage });

            var resultadoXp = _xpService.ProcessarRespostas(estudante, licao, dto.Respostas);

            if (resultadoXp.Erros > 0)
                estudante.QuantVidas = Math.Max(0, estudante.QuantVidas - 1);

            bool jaConcluiu = _context.LicaoConcluidas.Any(lc => lc.UsuarioId == usuarioId && lc.LicaoId == licao.Id);
            bool ganhouSequencia = false;

            if (!jaConcluiu)
            {
                _context.LicaoConcluidas.Add(new LicaoConcluida
                {
                    UsuarioId = usuarioId,
                    LicaoId = licao.Id,
                    ConcluidoEm = DateTime.UtcNow
                });

                estudante.LicoesConcluidas += 1;
                ganhouSequencia = AtualizarSequenciaAoConcluir(estudante);
            }

            _context.SaveChanges();

            var resultadoConquistas = _conquistaService.AvaliarConquistas(usuarioId, estudante);
            if (resultadoConquistas.NovasConquistas.Any())
                _context.SaveChanges();

            return Ok(new
            {
                acertos = resultadoXp.Acertos,
                erros = resultadoXp.Erros,
                xp = resultadoXp.XpGanhoTotal,
                xpTotalUsuario = resultadoXp.XpTotalUsuario,
                xpPorAtividade = resultadoXp.Atividades,
                ganhouSequencia,
                vidasRestantes = estudante.QuantVidas,
                sequenciaAtual = estudante.SequenciaDias,
                novasConquistas = resultadoConquistas.NovasConquistas
            });
        }

        //endpoint de recarga de vidas com cooldown de 4 horas
        [HttpPost("recarregar-vidas")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public IActionResult RecarregarVidas()
        {
            if (!this.TryGetAuthenticatedUserId(out var usuarioId))
                return this.UserNotAuthenticated();

            var estudante = ObterEstudante(usuarioId);
            if (estudante == null)
                return this.StudentProfileNotFound();

            if (estudante.QuantVidas >= 5)
                return Ok(new { Sucesso = true, Mensagem = "Você já está com as vidas completas.", QuantVidas = estudante.QuantVidas });

            // Cooldown de 4 horas desde a última atividade
            if (estudante.DataUltimaAtividade.HasValue)
            {
                var horasDesdeUltimaAtividade = (DateTime.UtcNow - estudante.DataUltimaAtividade.Value).TotalHours;
                if (horasDesdeUltimaAtividade < 4)
                {
                    var horasRestantes = Math.Ceiling(4 - horasDesdeUltimaAtividade);
                    return BadRequest(new
                    {
                        Sucesso = false,
                        Mensagem = $"Aguarde mais {horasRestantes}h para recarregar as vidas.",
                        HorasRestantes = horasRestantes
                    });
                }
            }

            estudante.QuantVidas = 5;
            _context.SaveChanges();

            return Ok(new { Sucesso = true, Mensagem = "Vidas recarregadas!", QuantVidas = estudante.QuantVidas });
        }

        [HttpGet("provao")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public IActionResult ObterProvao([FromQuery] int? quantidade)
        {
            var query = _context.Atividades
                .Include(a => a.Alternativas)
                .Where(a => a.ProvaFinal == true);

            if (quantidade.HasValue && quantidade.Value > 0)
                query = query.Take(quantidade.Value);

            var atividades = query.OrderBy(a => a.Id).ToList();

            if (!atividades.Any())
                return NotFound(new { Mensagem = "Nenhuma questão de provão cadastrada ainda." });

            var resultado = atividades.Select(a =>
            {
                var (alternativas, alternativasIds) = ObterAlternativasOrdenadas(a);
                return new
                {
                    atividadeId = a.Id,
                    enunciado = a.Enunciado,
                    dificuldade = a.Dificuldade,
                    alternativas,
                    alternativasIds
                };
            }).ToList();

            return Ok(new { Sucesso = true, TotalQuestoes = resultado.Count, Questoes = resultado });
        }

        [HttpGet("pratica")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public IActionResult ObterQuestoesPratica(
            [FromQuery] int? moduloId,
            [FromQuery] int? dificuldade,
            [FromQuery] int quantidade = 10)
        {
            var query = _context.Atividades
                .Include(a => a.Alternativas)
                .Include(a => a.Licao)
                    .ThenInclude(l => l!.Modulo)
                .Where(a => a.ProvaFinal != true)
                .AsQueryable();

            if (moduloId.HasValue)
                query = query.Where(a => a.Licao != null && a.Licao.ModuloId == moduloId.Value);

            if (dificuldade.HasValue)
                query = query.Where(a => a.Dificuldade == dificuldade.Value);

            var atividades = query
                .OrderBy(a => Guid.NewGuid())
                .Take(Math.Clamp(quantidade, 1, 50))
                .ToList();

            if (!atividades.Any())
                return NotFound(new { Mensagem = "Nenhuma questão encontrada com os filtros informados." });

            var resultado = atividades.Select(a =>
            {
                var (alternativas, alternativasIds) = ObterAlternativasOrdenadas(a);
                return new
                {
                    atividadeId = a.Id,
                    enunciado = a.Enunciado,
                    dificuldade = a.Dificuldade,
                    modulo = a.Licao?.Modulo?.Titulo,
                    licaoTitulo = a.Licao?.Titulo,
                    alternativas,
                    alternativasIds
                };
            }).ToList();

            return Ok(new { Sucesso = true, TotalQuestoes = resultado.Count, Questoes = resultado });
        }

        [HttpGet("vidas")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public IActionResult ObterVidas()
        {
            if (!this.TryGetAuthenticatedUserId(out var usuarioId))
                return this.UserNotAuthenticated();

            var estudante = ObterEstudante(usuarioId);
            if (estudante == null)
                return this.StudentProfileNotFound();

            return Ok(new
            {
                Sucesso = true,
                QuantVidas = estudante.QuantVidas,
                VidasMaximas = 5,
                PodeJogar = estudante.QuantVidas > 0
            });
        }

        [HttpGet("ofensiva")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public IActionResult ObterOfensiva()
        {
            if (!this.TryGetAuthenticatedUserId(out var usuarioId))
                return this.UserNotAuthenticated();

            var estudante = ObterEstudante(usuarioId);
            if (estudante == null)
                return this.StudentProfileNotFound();

            bool sequenciaAtiva = false;
            if (estudante.DataUltimaAtividade.HasValue)
            {
                var dias = (DateTime.UtcNow.Date - estudante.DataUltimaAtividade.Value.Date).TotalDays;
                sequenciaAtiva = dias <= 1;

                if (dias > 1)
                {
                    estudante.SequenciaDias = 0;
                    _context.SaveChanges();
                }
            }

            return Ok(new
            {
                Sucesso = true,
                SequenciaDias = estudante.SequenciaDias,
                SequenciaAtiva = sequenciaAtiva,
                UltimaAtividade = estudante.DataUltimaAtividade
            });
        }

        private Estudante? ObterEstudante(Guid usuarioId)
        {
            return _context.Estudante.FirstOrDefault(e => e.UsuarioId == usuarioId);
        }

        private static bool AtualizarSequenciaAoConcluir(Estudante estudante)
        {
            var agora = DateTime.UtcNow;
            var ganhouSequencia = false;
            var ontem = agora.Date.AddDays(-1);

            if (estudante.DataUltimaAtividade.HasValue)
            {
                var ultimaData = estudante.DataUltimaAtividade.Value.Date;
                if (ultimaData == ontem)
                {
                    estudante.SequenciaDias += 1;
                    ganhouSequencia = true;
                }
                else if (ultimaData < ontem)
                {
                    estudante.SequenciaDias = 1;
                }
            }
            else
            {
                estudante.SequenciaDias = 1;
                ganhouSequencia = true;
            }

            estudante.DataUltimaAtividade = agora;
            return ganhouSequencia;
        }

        private static bool TemRespostaInvalida(Licao licao, IReadOnlyCollection<RespostaDto> respostas)
        {
            var alternativasPorAtividade = (licao.Atividades ?? new List<Atividade>())
                .ToDictionary(
                    atividade => atividade.Id,
                    atividade => atividade.Alternativas.Select(alternativa => alternativa.Id).ToHashSet());

            return respostas.Any(resposta =>
                !alternativasPorAtividade.TryGetValue(resposta.AtividadeId, out var alternativasValidas) ||
                (resposta.AlternativaEscolhidaId.HasValue && !alternativasValidas.Contains(resposta.AlternativaEscolhidaId.Value)));
        }

        private static (List<string> Alternativas, List<int> AlternativasIds) ObterAlternativasOrdenadas(Atividade atividade)
        {
            var alternativasOrdenadas = atividade.Alternativas
                .OrderBy(alternativa => alternativa.Ordem)
                .ToList();

            return (
                alternativasOrdenadas.Select(alternativa => alternativa.Texto).ToList(),
                alternativasOrdenadas.Select(alternativa => alternativa.Id).ToList());
        }
    }
}