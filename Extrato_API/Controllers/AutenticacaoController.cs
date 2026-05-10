using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc;
using Extrato_API.DTOs;
using Extrato_API.Models;
using Extrato_API.Data;
using System.Net;
using System.Net.Mail;

namespace Extrato_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AutenticacaoController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AutenticacaoController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("cadastro")]
        public IActionResult Cadastrar([FromBody] CadastroUsuarioDTO dto)
        {
            bool emailJaExiste = _context.Usuarios.Any(u => u.Email == dto.Email);

            if (emailJaExiste)
            {
                return BadRequest(new
                {
                    Sucesso = false,
                    Mensagem = "Este e-mail já está em uso. Tente fazer login!"
                });
            }

            var novoUsuario = new Usuario
            {
                NomeUsuario = dto.NomeUsuario,
                Email = dto.Email,

                TipoUsuario = "estudante",

                SenhaHash = dto.Senha
            };

            _context.Usuarios.Add(novoUsuario);

            var estatisticas = new Models.EstatisticasUsuario
            {
                UsuarioId = novoUsuario.Id
            };
            _context.EstatisticasUsuarios.Add(estatisticas);

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
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == dto.Email);

            if (usuario == null)
                return Unauthorized(new { Sucesso = false, Mensagem = "Email ou senha incorretos." });

            var senhaValida = BCrypt.Net.BCrypt.Verify(dto.Senha + "poupas_pepper_secret", usuario.SenhaHash);

            if (!senhaValida)
                return Unauthorized(new { Sucesso = false, Mensagem = "Email ou senha incorretos." });

            if (usuario.TipoUsuario.ToLower() == "admin")
                return Unauthorized(new { Sucesso = false, Mensagem = "Administradores devem acessar via painel web." });

            return Ok(new
            {
                Sucesso = true,
                Mensagem = "Bem-vindo de volta!",
                Nome = usuario.NomeUsuario,
                Tipo = usuario.TipoUsuario,
                Token = GerarTokenJwt(usuario)
            });
        }

        [HttpPost("login-admin")]
        public IActionResult LoginAdmin([FromBody] LoginUsuarioDTO dto)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == dto.Email);

            if (usuario == null || usuario.SenhaHash != dto.Senha)
            {
                return Unauthorized(new { Sucesso = false, Mensagem = "Credenciais inválidas." });
            }

            if (usuario.TipoUsuario.ToLower() != "admin")
            {
                return StatusCode(403, new { Sucesso = false, Mensagem = "Acesso negado. Área exclusiva para administradores." });
            }

            return Ok(new
            {
                Sucesso = true,
                Mensagem = "Bem-vindo ao Painel Administrativo!",
                Nome = usuario.NomeUsuario,
                Tipo = usuario.TipoUsuario,
                Token = GerarTokenJwt(usuario)
            });
        }

        //Recuperação de senha
        //recebe o e-mail e envia o código
        [HttpPost("recuperar-senha")]
        public IActionResult SolicitarReset([FromBody] SolicitarResetSenhaDTO dto)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == dto.Email);

            if (usuario == null)
            {
                return Ok(new { Sucesso = true, Mensagem = "Se o e-mail existir, você receberá o código." });
            }

            var random = new Random();
            var codigo = random.Next(10000, 99999).ToString();

            usuario.CodigoResetSenha = codigo;
            usuario.CodigoResetExpiracao = DateTime.UtcNow.AddMinutes(15);
            _context.SaveChanges();

            try
            {
                EnviarEmailCodigo(usuario.Email, usuario.NomeUsuario, codigo);
            }
            catch (Exception ex)
            {
                usuario.CodigoResetSenha = null;
                usuario.CodigoResetExpiracao = null;
                _context.SaveChanges();

                return StatusCode(500, new { Sucesso = false, Mensagem = "Erro ao enviar e-mail.", Detalhe = ex.Message });
            }

            return Ok(new { Sucesso = true, Mensagem = "Se o e-mail existir, você receberá o código." });
        }

        //valida o código
        [HttpPost("verificar-codigo")]
        public IActionResult VerificarCodigo([FromBody] VerificarCodigoDTO dto)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == dto.Email);

            if (usuario == null || usuario.CodigoResetSenha == null || usuario.CodigoResetExpiracao == null)
                return BadRequest(new { Sucesso = false, Mensagem = "Código inválido ou expirado." });

            if (usuario.CodigoResetExpiracao < DateTime.UtcNow)
                return BadRequest(new { Sucesso = false, Mensagem = "O código expirou. Solicite um novo." });

            if (usuario.CodigoResetSenha != dto.Codigo)
                return BadRequest(new { Sucesso = false, Mensagem = "Código incorreto." });

            return Ok(new { Sucesso = true, Mensagem = "Código válido." });
        }

        //redefine a senha
        [HttpPost("redefinir-senha")]
        public IActionResult RedefinirSenha([FromBody] RedefinirSenhaDTO dto)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == dto.Email);

            if (usuario == null || usuario.CodigoResetSenha == null || usuario.CodigoResetExpiracao == null)
                return BadRequest(new { Sucesso = false, Mensagem = "Código inválido ou expirado." });

            if (usuario.CodigoResetExpiracao < DateTime.UtcNow)
                return BadRequest(new { Sucesso = false, Mensagem = "O código expirou. Solicite um novo." });

            if (usuario.CodigoResetSenha != dto.Codigo)
                return BadRequest(new { Sucesso = false, Mensagem = "Código incorreto." });

            usuario.SenhaHash = dto.NovaSenha;
            usuario.CodigoResetSenha = null;
            usuario.CodigoResetExpiracao = null;
            _context.SaveChanges();

            return Ok(new { Sucesso = true, Mensagem = "Senha redefinida com sucesso!" });
        }

        //Método auxiliar — envia o e-mail com o código
        private void EnviarEmailCodigo(string destinatario, string nomeUsuario, string codigo)
        {
            var emailConfig = _configuration.GetSection("Email");

            var smtpClient = new SmtpClient(emailConfig["Smtp"])
            {
                Port = int.Parse(emailConfig["Porta"]!),
                Credentials = new NetworkCredential(emailConfig["Remetente"], emailConfig["Senha"]),
                EnableSsl = true,
            };

            var mensagem = new MailMessage
            {
                From = new MailAddress(emailConfig["Remetente"]!, "Poupas App"),
                Subject = "Seu código de verificação - Poupas",
                Body = $@"
                    <h2>Olá, {nomeUsuario}!</h2>
                    <p>Recebemos uma solicitação para redefinir a sua senha.</p>
                    <p>Use o código abaixo para continuar:</p>
                    <h1 style='letter-spacing: 8px; color: #E32626;'>{codigo}</h1>
                    <p>Este código expira em <strong>15 minutos</strong>.</p>
                    <p>Se você não solicitou isso, ignore este e-mail.</p>
                ",
                IsBodyHtml = true,
            };

            mensagem.To.Add(destinatario);
            smtpClient.Send(mensagem);
        }

        private string GerarTokenJwt(Usuario usuario)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);

            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
        new Claim("nome", usuario.NomeUsuario),
        new Claim(ClaimTypes.Role, usuario.TipoUsuario)
    };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

    }


}