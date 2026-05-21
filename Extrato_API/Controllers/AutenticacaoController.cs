using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Extrato_API.Data;
using Extrato_API.DTOs;
using Extrato_API.Models;

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
            var emailLimpo = dto.Email.Trim().ToLower();
            var senhaLimpa = dto.Senha.Trim();

            bool emailJaExiste = _context.Usuarios.Any(u => u.Email.ToLower() == emailLimpo);
            if (emailJaExiste)
                return BadRequest(new { Sucesso = false, Mensagem = "Este e-mail já está em uso. Tente fazer login!" });

            string senhaHasheada = BCrypt.Net.BCrypt.HashPassword(senhaLimpa + "poupas_pepper_secret");

            var novoUsuario = new Usuario
            {
                NomeUsuario = dto.NomeUsuario.Trim(),
                Email = emailLimpo,
                TipoUsuario = "estudante",
                SenhaHash = senhaHasheada
            };

            _context.Usuarios.Add(novoUsuario);
            _context.SaveChanges();

            _context.Estudante.Add(new Estudante { UsuarioId = novoUsuario.Id, AvatarId = 1 });
            _context.SaveChanges();

            return Ok(new { Sucesso = true, Mensagem = "Usuário cadastrado com sucesso!", Nome = novoUsuario.NomeUsuario, Email = novoUsuario.Email });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginUsuarioDTO dto)
        {
            var emailLimpo = dto.Email.Trim().ToLower();
            var senhaLimpa = dto.Senha.Trim();

            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email.ToLower() == emailLimpo);

            if (usuario == null)
            {
                return Unauthorized(new { Sucesso = false, Mensagem = "Email ou senha incorretos." });
            }

            bool senhaValida = BCrypt.Net.BCrypt.Verify(senhaLimpa + "poupas_pepper_secret", usuario.SenhaHash);

            if (!senhaValida)
            {
                return Unauthorized(new { Sucesso = false, Mensagem = "Email ou senha incorretos." });
            }

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

        //usando BCrypt igual ao login normal
        [HttpPost("login-admin")]
        public IActionResult LoginAdmin([FromBody] LoginUsuarioDTO dto)
        {
            var emailLimpo = dto.Email.Trim().ToLower();
            var senhaLimpa = dto.Senha.Trim();

            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email.ToLower() == emailLimpo);

            if (usuario == null)
                return Unauthorized(new { Sucesso = false, Mensagem = "Credenciais inválidas." });

            if (!BCrypt.Net.BCrypt.Verify(senhaLimpa + "poupas_pepper_secret", usuario.SenhaHash))
                return Unauthorized(new { Sucesso = false, Mensagem = "Credenciais inválidas." });

            if (usuario.TipoUsuario.ToLower() != "admin")
                return StatusCode(403, new { Sucesso = false, Mensagem = "Acesso negado." });

            return Ok(new
            {
                Sucesso = true,
                Mensagem = "Bem-vindo ao Painel Administrativo!",
                Nome = usuario.NomeUsuario,
                Tipo = usuario.TipoUsuario,
                Token = GerarTokenJwt(usuario)
            });
        }

        //normaliza e-mail igual ao login + código seguro com RandomNumberGenerator
        [HttpPost("recuperar-senha")]
        public IActionResult SolicitarReset([FromBody] SolicitarResetSenhaDTO dto)
        {
            var emailLimpo = dto.Email.Trim().ToLower();
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == emailLimpo);

            if (usuario == null)
                return Ok(new { Sucesso = true, Mensagem = "Se o e-mail existir, você receberá o código." });

            var resetAntigo = _context.ResetSenhas.FirstOrDefault(r => r.Email == emailLimpo);
            if (resetAntigo != null)
                _context.ResetSenhas.Remove(resetAntigo);

            // Corrigido: RandomNumberGenerator é criptograficamente seguro (6 dígitos)
            var codigoNumerico = System.Security.Cryptography.RandomNumberGenerator.GetInt32(100000, 999999);
            var codigo = codigoNumerico.ToString();

            _context.ResetSenhas.Add(new ResetSenha
            {
                Email = emailLimpo,
                Codigo = codigo,
                Expiracao = DateTime.UtcNow.AddMinutes(15)
            });
            _context.SaveChanges();

            try
            {
                EnviarEmailCodigo(usuario.Email, usuario.NomeUsuario, codigo);
            }
            catch
            {
                // Corrigido: não expõe detalhe interno do erro
                var resetCriado = _context.ResetSenhas.FirstOrDefault(r => r.Email == emailLimpo);
                if (resetCriado != null) _context.ResetSenhas.Remove(resetCriado);
                _context.SaveChanges();
                return StatusCode(500, new { Sucesso = false, Mensagem = "Não foi possível enviar o e-mail. Tente novamente mais tarde." });
            }

            return Ok(new { Sucesso = true, Mensagem = "Se o e-mail existir, você receberá o código." });
        }

        //normaliza e-mail
        [HttpPost("verificar-codigo")]
        public IActionResult VerificarCodigo([FromBody] VerificarCodigoDTO dto)
        {
            var emailLimpo = dto.Email.Trim().ToLower();
            var reset = _context.ResetSenhas.FirstOrDefault(r => r.Email == emailLimpo);

            if (reset == null)
                return BadRequest(new { Sucesso = false, Mensagem = "Código inválido ou expirado." });

            if (reset.Expiracao < DateTime.UtcNow)
            {
                _context.ResetSenhas.Remove(reset);
                _context.SaveChanges();
                return BadRequest(new { Sucesso = false, Mensagem = "O código expirou. Solicite um novo." });
            }

            if (reset.Codigo != dto.Codigo)
                return BadRequest(new { Sucesso = false, Mensagem = "Código incorreto." });

            return Ok(new { Sucesso = true, Mensagem = "Código válido." });
        }

        //normaliza e-mail
        [HttpPost("redefinir-senha")]
        public IActionResult RedefinirSenha([FromBody] RedefinirSenhaDTO dto)
        {
            var emailLimpo = dto.Email.Trim().ToLower();
            var reset = _context.ResetSenhas.FirstOrDefault(r => r.Email == emailLimpo);

            if (reset == null)
                return BadRequest(new { Sucesso = false, Mensagem = "Código inválido ou expirado." });

            if (reset.Expiracao < DateTime.UtcNow)
            {
                _context.ResetSenhas.Remove(reset);
                _context.SaveChanges();
                return BadRequest(new { Sucesso = false, Mensagem = "O código expirou. Solicite um novo." });
            }

            if (reset.Codigo != dto.Codigo)
                return BadRequest(new { Sucesso = false, Mensagem = "Código incorreto." });

            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == emailLimpo);
            if (usuario == null)
                return NotFound(new { Sucesso = false, Mensagem = "Usuário não encontrado." });

            usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.NovaSenha + "poupas_pepper_secret");
            _context.ResetSenhas.Remove(reset);
            _context.SaveChanges();

            return Ok(new { Sucesso = true, Mensagem = "Senha redefinida com sucesso!" });
        }

        private void EnviarEmailCodigo(string destinatario, string nomeUsuario, string codigo)
        {
            var emailConfig = _configuration.GetSection("Email");
            var smtp = new SmtpClient(emailConfig["Smtp"])
            {
                Port = int.Parse(emailConfig["Porta"]!),
                Credentials = new NetworkCredential(emailConfig["Remetente"], emailConfig["Senha"]),
                EnableSsl = true
            };

            var body = $@"
            <!DOCTYPE html>
            <html lang='pt-BR'>
            <head><meta charset='UTF-8'></head>
            <body style='margin:0;padding:0;background-color:#f5f0eb;font-family:Arial,sans-serif;'>
              <table width='100%' cellpadding='0' cellspacing='0' style='background-color:#f5f0eb;padding:40px 0;'>
                <tr>
                  <td align='center'>
                    <table width='480' cellpadding='0' cellspacing='0' style='background-color:#ffffff;border-radius:24px;overflow:hidden;box-shadow:0 4px 20px rgba(0,0,0,0.08);'>
                      <tr>
                        <td align='center' style='background-color:#E32626;padding:36px 40px 28px;'>
                          <div style='font-size:64px;line-height:1;'>🍎</div>
                          <h1 style='color:#ffffff;font-size:28px;margin:12px 0 4px;font-weight:bold;letter-spacing:1px;'>Poupas</h1>
                          <p style='color:#ffcccc;font-size:13px;margin:0;'>Um jeito divertido de aprender a poupar</p>
                        </td>
                      </tr>
                      <tr>
                        <td style='padding:36px 40px;'>
                          <p style='color:#333333;font-size:16px;margin:0 0 8px;'>Olá, <strong>{nomeUsuario}</strong>! 👋</p>
                          <p style='color:#666666;font-size:15px;margin:0 0 28px;line-height:1.6;'>
                            Recebemos uma solicitação para redefinir a sua senha no Poupas.<br>
                            Use o código abaixo para continuar:
                          </p>
                          <div style='background-color:#fff5f5;border:2px dashed #E32626;border-radius:16px;padding:24px;text-align:center;margin:0 0 28px;'>
                            <p style='color:#999999;font-size:12px;margin:0 0 8px;text-transform:uppercase;letter-spacing:2px;'>Seu código de verificação</p>
                            <p style='color:#E32626;font-size:42px;font-weight:bold;margin:0;letter-spacing:12px;font-family:monospace;'>{codigo}</p>
                          </div>
                          <div style='background-color:#fff9e6;border-left:4px solid #f5a623;border-radius:8px;padding:14px 16px;margin:0 0 28px;'>
                            <p style='color:#7a6000;font-size:13px;margin:0;'>
                              ⏱️ Este código expira em <strong>15 minutos</strong>.
                            </p>
                          </div>
                          <p style='color:#999999;font-size:13px;margin:0;line-height:1.6;'>
                            Se você não solicitou a redefinição de senha, ignore este e-mail. Sua conta está segura. 🔒
                          </p>
                        </td>
                      </tr>
                      <tr>
                        <td align='center' style='background-color:#f9f5f2;padding:20px 40px;border-top:1px solid #eeeeee;'>
                          <p style='color:#bbbbbb;font-size:12px;margin:0;'>© 2026 Poupas App · Todos os direitos reservados</p>
                          <p style='color:#bbbbbb;font-size:12px;margin:4px 0 0;'>🍎 Aprendendo a poupar de um jeito divertido</p>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
              </table>
            </body>
            </html>";

            var msg = new MailMessage
            {
                From = new MailAddress(emailConfig["Remetente"]!, "Poupas App 🍎"),
                Subject = "🔑 Seu código de verificação - Poupas",
                Body = body,
                IsBodyHtml = true
            };
            msg.To.Add(destinatario);
            smtp.Send(msg);
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

            var token = new JwtSecurityTokenHandler().CreateToken(new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"]
            });

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}