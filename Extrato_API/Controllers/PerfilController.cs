using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Extrato_API.DTOs;
using Extrato_API.Data;

namespace Extrato_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PerfilController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PerfilController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult ObterPerfil()
        {
            var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                      ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var usuarioId))
                return Unauthorized(new { Sucesso = false, Mensagem = "Usuário não autenticado." });

            var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);

            if (usuario == null)
                return NotFound(new { Sucesso = false, Mensagem = "Usuário não encontrado." });

            var estudante = _context.Estudante.FirstOrDefault(e => e.UsuarioId == usuarioId);

            return Ok(new
            {
                Sucesso = true,
                NomeUsuario = usuario.NomeUsuario,
                Email = usuario.Email,
                AvatarId = estudante?.AvatarId,
                LicoesConcluidas = estudante?.LicoesConcluidas ?? 0,
                ExerciciosResolvidos = estudante?.ExerciciosResolvidos ?? 0,
                Pontuacao = estudante?.XpTotal ?? 0,
                SequenciaDias = estudante?.SequenciaDias ?? 0,
                QuantVidas = estudante?.QuantVidas ?? 5
            });
        }

        [HttpPut("editar")]
        public IActionResult EditarPerfil([FromBody] EditarPerfilDTO dto)
        {
            var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                      ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var usuarioId))
                return Unauthorized(new { Sucesso = false, Mensagem = "Usuário não autenticado." });

            var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);

            if (usuario == null)
                return NotFound(new { Sucesso = false, Mensagem = "Usuário não encontrado." });

            usuario.NomeUsuario = dto.NovoNome;

            if (dto.AvatarId.HasValue)
            {
                var estudante = _context.Estudante.FirstOrDefault(e => e.UsuarioId == usuarioId);
                if (estudante != null)
                    estudante.AvatarId = dto.AvatarId.Value;
            }

            _context.SaveChanges();

            return Ok(new
            {
                Sucesso = true,
                Mensagem = "Perfil atualizado com sucesso.",
                NomeUsuario = usuario.NomeUsuario
            });
        }
    }
}