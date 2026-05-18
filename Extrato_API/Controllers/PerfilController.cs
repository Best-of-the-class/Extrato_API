using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Extrato_API.DTOs;
using Extrato_API.Data;
using Extrato_API.Models;

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

            bool emailJaExiste = _context.Usuarios.Any(u =>
                u.Id != usuario.Id && u.Email.ToLower() == novoEmailNormalizado);

            if (emailJaExiste)
            {
                return Conflict(new
                {
                    Sucesso = false,
                    Mensagem = "Este e-mail já está em uso por outro usuário."
                });
            }

            if (dto.AvatarId.HasValue)
            {
                var estudante = _context.Estudante.FirstOrDefault(e => e.UsuarioId == usuarioId);
                if (estudante != null)
                    estudante.AvatarId = dto.AvatarId.Value;
            }

            _context.SaveChanges();

            var stats = _context.EstatisticasUsuarios.FirstOrDefault(e => e.UsuarioId == usuario.Id);

            return Ok(CriarRespostaPerfil(
                usuario,
                stats,
                "Perfil atualizado com sucesso."));
        }

        private static object CriarRespostaPerfil(Usuario usuario, EstatisticasUsuario? stats, string? mensagem = null)
        {
            return new
            {
                Sucesso = true,
                Mensagem = mensagem,
                NomeUsuario = usuario.NomeUsuario,
                Email = usuario.Email,
                LicoesConcluidas = stats?.LicoesConcluidas ?? 0,
                ExerciciosResolvidos = stats?.ExerciciosResolvidos ?? 0,
                Pontuacao = stats?.Pontuacao ?? 0,
                SequenciaDias = stats?.SequenciaDias ?? 0
            };
        }
    }
}