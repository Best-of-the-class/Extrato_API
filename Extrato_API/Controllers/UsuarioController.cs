using Microsoft.AspNetCore.Mvc;
using Extrato_API.DTOs;
using Extrato_API.Data;

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

        //Logout
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new { Sucesso = true, Mensagem = "Logout realizado com sucesso." });
        }

        //Deletar conta
        [HttpDelete("deletar")]
        public IActionResult DeletarConta([FromBody] DeletarContaDTO dto)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == dto.Email);

            if (usuario == null)
                return NotFound(new { Sucesso = false, Mensagem = "Usuário não encontrado." });

            if (usuario.SenhaHash != dto.Senha)
                return Unauthorized(new { Sucesso = false, Mensagem = "Senha incorreta." });

            _context.Usuarios.Remove(usuario);
            _context.SaveChanges();

            return Ok(new { Sucesso = true, Mensagem = "Conta excluída com sucesso." });
        }

        //Alterar avatar
        [HttpPut("avatar")]
        public IActionResult AlterarAvatar([FromBody] AlterarAvatarDTO dto)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == dto.Email);

            if (usuario == null)
                return NotFound(new { Sucesso = false, Mensagem = "Usuário não encontrado." });

            usuario.AvatarId = dto.AvatarId;
            _context.SaveChanges();

            return Ok(new { Sucesso = true, Mensagem = "Avatar atualizado com sucesso.", AvatarId = usuario.AvatarId });
        }
    }
}