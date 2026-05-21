using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Extrato_API.Constants;
using Extrato_API.DTOs;
using Extrato_API.Data;
using Extrato_API.Extensions;

namespace Extrato_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsuarioController(AppDbContext context)
        {
            _context = context;
        }

        // Logout 
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public IActionResult Logout()
        {
            return Ok(new { Sucesso = true, Mensagem = "Logout realizado com sucesso." });
        }

        // Deletar conta 
        [HttpDelete("deletar")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public IActionResult DeletarConta([FromBody] DeletarContaDTO dto)
        {
            if (!this.TryGetAuthenticatedUserId(out var usuarioId))
                return this.UserNotAuthenticated();

            var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);

            if (usuario == null)
                return NotFound(new { Sucesso = false, Mensagem = "Usuário não encontrado." });

            if (!string.Equals(usuario.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { Sucesso = false, Mensagem = "E-mail não corresponde à conta autenticada." });

            var senhaValida = BCrypt.Net.BCrypt.Verify(dto.Senha + SecurityConstants.HashPepper, usuario.SenhaHash);
            if (!senhaValida)
                return Unauthorized(new { Sucesso = false, Mensagem = "Senha incorreta." });

            // Remove dados vinculados antes de remover o usuário
            var estudante = _context.Estudante.FirstOrDefault(e => e.UsuarioId == usuarioId);
            if (estudante != null)
                _context.Estudante.Remove(estudante);

            var licoesConcluidas = _context.LicaoConcluidas.Where(lc => lc.UsuarioId == usuarioId).ToList();
            if (licoesConcluidas.Any())
                _context.LicaoConcluidas.RemoveRange(licoesConcluidas);

            _context.Usuarios.Remove(usuario);
            _context.SaveChanges();

            return Ok(new { Sucesso = true, Mensagem = "Conta excluída com sucesso." });
        }

        // Alterar avatar 
        [HttpPut("avatar")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public IActionResult AlterarAvatar([FromBody] AlterarAvatarDTO dto)
        {
            if (!this.TryGetAuthenticatedUserId(out var usuarioId))
                return this.UserNotAuthenticated();

            var estudante = _context.Estudante.FirstOrDefault(e => e.UsuarioId == usuarioId);

            if (estudante == null)
                return this.StudentProfileNotFound(includeSuccessFlag: true);

            bool avatarExiste = _context.Avatares.Any(a => a.Id == dto.AvatarId);
            if (!avatarExiste)
                return BadRequest(new { Sucesso = false, Mensagem = "Avatar inválido. Escolha um avatar disponível." });

            estudante.AvatarId = dto.AvatarId;
            _context.SaveChanges();

            return Ok(new
            {
                Sucesso = true,
                Mensagem = "Avatar atualizado com sucesso.",
                AvatarId = estudante.AvatarId
            });
        }

        // GET avatares disponíveis 
        [HttpGet("avatares")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public IActionResult ListarAvatares()
        {
            var avatares = _context.Avatares
                .Select(a => new { a.Id, a.UrlImagem })
                .ToList();

            return Ok(new { Sucesso = true, Avatares = avatares });
        }
    }
}