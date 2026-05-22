using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Extrato_API.Data;
using Extrato_API.Models;
using Extrato_API.DTOs;

namespace Extrato_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DicionarioController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DicionarioController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public IActionResult ListarTermos()
        {
            var termos = _context.Dicionarios
                .OrderBy(d => d.Termo)
                .Select(d => new
                {
                    d.Id,
                    d.Termo,
                    d.Definicao,
                    d.CriadoEm
                })
                .ToList();

            return Ok(new { Sucesso = true, Dados = termos });
        }

        [HttpGet("{id}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public IActionResult ObterTermo(int id)
        {
            var termo = _context.Dicionarios.FirstOrDefault(d => d.Id == id);

            if (termo == null)
                return NotFound(new { Sucesso = false, Mensagem = "Termo não encontrado." });

            return Ok(new { Sucesso = true, Dados = termo });
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    public IActionResult CriarTermo([FromBody] CriarDicionarioDto dto)
        {
            var novoTermo = new Dicionario
            {
                Termo = dto.Termo.Trim(),
                Definicao = dto.Definicao.Trim(),
                CriadoEm = DateTime.UtcNow
            };

            _context.Dicionarios.Add(novoTermo);
            _context.SaveChanges();

            return CreatedAtAction(nameof(ObterTermo), new { id = novoTermo.Id }, new { Sucesso = true, Mensagem = "Termo criado com sucesso!", Dados = novoTermo });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult AtualizarTermo(int id, [FromBody] AtualizarDicionarioDto dto)
        {
            var termoExistente = _context.Dicionarios.FirstOrDefault(d => d.Id == id);

            if (termoExistente == null)
                return NotFound(new { Sucesso = false, Mensagem = "Termo não encontrado." });

            termoExistente.Termo = dto.Termo.Trim();
            termoExistente.Definicao = dto.Definicao.Trim();

            _context.SaveChanges();

            return Ok(new { Sucesso = true, Mensagem = "Termo atualizado com sucesso!", Dados = termoExistente });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public IActionResult DeletarTermo(int id)
        {
            var termo = _context.Dicionarios.FirstOrDefault(d => d.Id == id);

            if (termo == null)
                return NotFound(new { Sucesso = false, Mensagem = "Termo não encontrado." });

            _context.Dicionarios.Remove(termo);
            _context.SaveChanges();

            return Ok(new { Sucesso = true, Mensagem = "Termo excluído com sucesso." });
        }
    }
}