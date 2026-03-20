using Microsoft.AspNetCore.Mvc;
using Extrato_API.DTOs;

namespace Extrato_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AutenticacaoController : ControllerBase
    {
        [HttpPost("cadastro")]
        public IActionResult Cadastrar([FromBody] CadastroUsuarioDTO dto)
        {

            //Mockup de validação de dados enquanto não tem banco
            return Ok(new
            {
                Sucesso = true,
                Mensagem = "Eba! Usuário cadastrado com sucesso!",
                Nome = dto.NomeUsuario,
                Email = dto.Email
            });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginUsuarioDTO dto)
        {

            if (dto.Email == "teste@admin.com" && dto.Senha == "senha12345")
            {
                return Ok(new
                {
                    Sucesso = true,
                    Mensagem = "Bem-vindo de volta!",
                    Token = "token_falso_para_teste_123456789"
                });
            }

            return Unauthorized(new
            {
                Sucesso = false,
                Mensagem = "Email ou senha incorretos. Tente novamente!"
            });
        }
    }
}
