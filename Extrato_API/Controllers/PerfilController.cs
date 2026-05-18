using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Extrato_API.DTOs;
using Extrato_API.Data;
using Extrato_API.Models;
using Extrato_API.Services.Implementations;

namespace Extrato_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PerfilController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ConquistaService _conquistaService;

        public PerfilController(AppDbContext context, ConquistaService conquistaService)
        {
            _context = context;
            _conquistaService = conquistaService;
        }

        [HttpGet]
        public IActionResult ObterPerfil()
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

            var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);

            if (usuario == null)
            {
                return NotFound(new
                {
                    Sucesso = false,
                    Mensagem = "Usuário não encontrado."
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

            return Ok(CriarRespostaPerfil(usuario, estudante, conquistas));
        }

        [HttpPut("editar")]
        public IActionResult EditarPerfil([FromBody] EditarPerfilDTO dto)
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

            var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);

            if (usuario == null)
            {
                return NotFound(new
                {
                    Sucesso = false,
                    Mensagem = "Usuário não encontrado."
                });
            }

            if (!string.Equals(usuario.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    Sucesso = false,
                    Mensagem = "E-mail não corresponde à conta autenticada."
                });
            }

            if (string.IsNullOrWhiteSpace(dto.NovoNome))
            {
                return BadRequest(new
                {
                    Sucesso = false,
                    Mensagem = "O novo nome é obrigatório."
                });
            }

            var estudante = _context.Estudante
                .FirstOrDefault(e => e.UsuarioId == usuarioId);

            if (estudante == null)
            {
                return NotFound(new
                {
                    Sucesso = false,
                    Mensagem = "Perfil do estudante não encontrado."
                });
            }

            var novoEmailNormalizado = dto.NovoEmail.Trim().ToLower();
            var novoNomeNormalizado = dto.NovoNome.Trim();

            bool emailJaExiste = _context.Usuarios.Any(u =>
                u.Id != usuario.Id &&
                u.Email.ToLower() == novoEmailNormalizado);

            if (emailJaExiste)
            {
                return Conflict(new
                {
                    Sucesso = false,
                    Mensagem = "Este e-mail já está em uso por outro usuário."
                });
            }

            usuario.Email = novoEmailNormalizado;
            usuario.NomeUsuario = novoNomeNormalizado;

            if (dto.AvatarId.HasValue)
            {
                bool avatarExiste = _context.Avatares.Any(a => a.Id == dto.AvatarId.Value);

                if (!avatarExiste)
                {
                    return BadRequest(new
                    {
                        Sucesso = false,
                        Mensagem = "Avatar inválido. Escolha um avatar disponível."
                    });
                }

                estudante.AvatarId = dto.AvatarId.Value;
            }

            _context.SaveChanges();

            var conquistas = _conquistaService.ObterConquistas(estudante);

            return Ok(CriarRespostaPerfil(
                usuario,
                estudante,
                conquistas,
                "Perfil atualizado com sucesso."
            ));
        }

        private static object CriarRespostaPerfil(
            Usuario usuario,
            Estudante? estudante,
            IReadOnlyCollection<ConquistaDTO>? conquistas = null,
            string? mensagem = null)
        {
            return new
            {
                Sucesso = true,
                Mensagem = mensagem,
                NomeUsuario = usuario.NomeUsuario,
                Email = usuario.Email,
                AvatarId = estudante?.AvatarId,
                LicoesConcluidas = estudante?.LicoesConcluidas ?? 0,
                ExerciciosResolvidos = estudante?.ExerciciosResolvidos ?? 0,
                Pontuacao = estudante?.XpTotal ?? 0,
                SequenciaDias = estudante?.SequenciaDias ?? 0,
                QuantVidas = estudante?.QuantVidas ?? 5,
                Conquistas = conquistas ?? Array.Empty<ConquistaDTO>()
            };
        }
    }
}