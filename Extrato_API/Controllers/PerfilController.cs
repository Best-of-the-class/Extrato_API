using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
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
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == dto.Email);

            if (usuario == null)
                return NotFound(new { Sucesso = false, Mensagem = "Usuário não encontrado." });

            usuario.NomeUsuario = dto.NovoNome;

            /*if (dto.AvatarId.HasValue)
                usuario.AvatarId = dto.AvatarId;*/

            _context.SaveChanges();

            return Ok(new
            {
                Sucesso = true,
                Mensagem = "Perfil atualizado com sucesso.",
                NomeUsuario = usuario.NomeUsuario,
                //AvatarId = usuario.AvatarId
            });
        }
    }
}