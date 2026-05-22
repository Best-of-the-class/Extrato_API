using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Extrato_API.Data;
using Extrato_API.Extensions;
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
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public IActionResult ObterConquistasUsuario()
        {
            if (!this.TryGetAuthenticatedUserId(out var usuarioId))
                return this.UserNotAuthenticated();

            var estudante = _context.Estudante.FirstOrDefault(e => e.UsuarioId == usuarioId);

            if (estudante == null)
                return this.StudentProfileNotFound(includeSuccessFlag: true);

            var conquistas = _conquistaService.ObterConquistas(estudante);

            //removido NovasConquistas que sempre vinha vazio e induzia o front ao erro.
            //Novas conquistas são retornadas apenas no endpoint /concluir após cada lição.
            return Ok(new
            {
                Sucesso = true,
                Total = conquistas.Count,
                Desbloqueadas = conquistas.Count(c => c.Desbloqueada),
                Conquistas = conquistas
            });
        }
    }
}