using Microsoft.AspNetCore.Mvc;
using Extrato_API.DTOs;
using Extrato_API.Models;
using Extrato_API.Data;
using System.Linq;

namespace Extrato_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AutenticacaoController : ControllerBase
    {
        // Variável que guarda o nosso "Gerente do Banco"
        private readonly AppDbContext _context;

        // O Construtor: O Visual Studio injeta o banco aqui automaticamente
        public AutenticacaoController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("cadastro")]
        public IActionResult Cadastrar([FromBody] CadastroUsuarioDTO dto)
        {
            // 1. Verificar se o e-mail já está cadastrado no banco
            bool emailJaExiste = _context.Usuarios.Any(u => u.Email == dto.Email);

            if (emailJaExiste)
            {
                return BadRequest(new
                {
                    Sucesso = false,
                    Mensagem = "Este e-mail já está em uso. Tente fazer login!"
                });
            }

            // 2. Criar o objeto Usuario para salvar
            var novoUsuario = new Usuario
            {
                NomeUsuario = dto.NomeUsuario,
                Email = dto.Email,

                // ATENÇÃO: Salvando direto para o teste de hoje.
                // Amanhã nós colocamos a criptografia (Hash) aqui!
                SenhaHash = dto.Senha
            };

            // 3. Adicionar e Salvar no banco de dados da Neon!
            _context.Usuarios.Add(novoUsuario);
            _context.SaveChanges();

            return Ok(new
            {
                Sucesso = true,
                Mensagem = "Eba! Usuário salvo no banco de dados com sucesso!",
                Nome = novoUsuario.NomeUsuario,
                Email = novoUsuario.Email
            });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginUsuarioDTO dto)
        {
            // O Login continua como mockup, faremos a leitura do banco na próxima etapa!
            if (dto.Email == "teste@admin.com" && dto.Senha == "senha12345")
            {
                return Ok(new
                {
                    Sucesso = true,
                    Mensagem = "Bem-vindo de volta!",
                    Token = "token_falso_para_teste_123456789"
                });
            }

            return Unauthorized(new { Sucesso = false, Mensagem = "Email ou senha incorretos." });
        }
    }
}