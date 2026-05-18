using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Extrato_API.Data;
using Extrato_API.DTOs;
using Extrato_API.Services.Implementations;

namespace Extrato_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ConquistasController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ConquistaService _conquistaService;

        public ConquistasController(AppDbContext context, ConquistaService conquistaService)
        {
            _context = context;
            _conquistaService = conquistaService;
        }

        [HttpGet]
        public IActionResult ObterConquistasUsuario()
        {
            var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                   ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var usuarioId))
            {
                return Unauthorized(new
                {
                    Sucesso = false,
                    Mensagem = "Usuário não autenticado."
                });
            }

            var estudante = _context.Estudante.FirstOrDefault(e => e.UsuarioId == usuarioId);

            if (estudante == null)
            {
                return NotFound(new
                {
                    Sucesso = false,
                    Mensagem = "Perfil do estudante não encontrado."
                });
            }

            var conquistas = _conquistaService.ObterConquistas(estudante);

            return Ok(new
            {
                Sucesso = true,
                Total = conquistas.Count,
                Desbloqueadas = conquistas.Count(c => c.Desbloqueada),
                NovasConquistas = Array.Empty<ConquistaDTO>(),
                Conquistas = conquistas
            });
        }
    }
}