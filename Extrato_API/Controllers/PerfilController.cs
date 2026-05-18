using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
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
            var sub = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
                      ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var usuarioId))
                return Unauthorized(new { Sucesso = false, Mensagem = "Usuário não autenticado." });

            var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);

            if (usuario == null)
                return NotFound(new { Sucesso = false, Mensagem = "Usuário não encontrado." });

            var stats = _context.Estudante.FirstOrDefault(e => e.UsuarioId == usuario.Id);

            return Ok(new
            {
                Sucesso = true,
                NomeUsuario = usuario.NomeUsuario,
                Email = usuario.Email,
                //AvatarId = usuario.AvatarId,
                LicoesConcluidas = stats?.LicoesConcluidas ?? 0,
                ExerciciosResolvidos = stats?.ExerciciosResolvidos ?? 0,
                Pontuacao = stats?.XpTotal ?? 0,
                SequenciaDias = stats?.SequenciaDias ?? 0
            });
        }

        [HttpPut("editar")]
        public IActionResult EditarPerfil([FromBody] EditarPerfilDTO dto)
        {
            var emailAtualNormalizado = dto.Email.Trim().ToLower();
            var novoEmailNormalizado = dto.NovoEmail.Trim().ToLower();
            var novoNomeNormalizado = dto.NovoNome.Trim();

            if (string.IsNullOrWhiteSpace(novoNomeNormalizado))
            {
                return BadRequest(new
                {
                    Sucesso = false,
                    Mensagem = "O novo nome não pode estar vazio."
                });
            }

            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email.ToLower() == emailAtualNormalizado);

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

            usuario.NomeUsuario = novoNomeNormalizado;
            usuario.Email = novoEmailNormalizado;

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