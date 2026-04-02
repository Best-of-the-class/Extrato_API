using Microsoft.AspNetCore.Mvc;
using Extrato_API.DTOs;
using Extrato_API.Data;

namespace Extrato_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PerfilController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PerfilController(AppDbContext context)
        {
            _context = context;
        }

        // Buscar perfil completo: header (nome, email, avatar) + stats
        [HttpGet]
        public IActionResult ObterPerfil([FromQuery] string email)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == email);

            if (usuario == null)
                return NotFound(new { Sucesso = false, Mensagem = "Usuário não encontrado." });

            var stats = _context.EstatisticasUsuarios.FirstOrDefault(e => e.UsuarioId == usuario.Id);

            return Ok(new
            {
                Sucesso = true,
                NomeUsuario = usuario.NomeUsuario,
                Email = usuario.Email,
                AvatarId = usuario.AvatarId,
                LicoesConcluidas = stats?.LicoesConcluidas ?? 0,
                ExerciciosResolvidos = stats?.ExerciciosResolvidos ?? 0,
                Pontuacao = stats?.Pontuacao ?? 0,
                SequenciaDias = stats?.SequenciaDias ?? 0
            });
        }

        // Salvar todas as alterações do perfil (ação "Editar Perfil")
        [HttpPut("editar")]
        public IActionResult EditarPerfil([FromBody] EditarPerfilDTO dto)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == dto.Email);

            if (usuario == null)
                return NotFound(new { Sucesso = false, Mensagem = "Usuário não encontrado." });

            usuario.NomeUsuario = dto.NovoNome;

            if (dto.AvatarId.HasValue)
                usuario.AvatarId = dto.AvatarId;

            _context.SaveChanges();

            return Ok(new
            {
                Sucesso = true,
                Mensagem = "Perfil atualizado com sucesso.",
                NomeUsuario = usuario.NomeUsuario,
                AvatarId = usuario.AvatarId
            });
        }
    }
}
