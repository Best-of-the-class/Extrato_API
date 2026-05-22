using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Extrato_API.Extensions
{
    public static class ControllerBaseExtensions
    {
        public const string UserNotAuthenticatedMessage = "Usuário não autenticado.";
        public const string StudentProfileNotFoundMessage = "Perfil do estudante não encontrado.";

        public static bool TryGetAuthenticatedUserId(this ControllerBase controller, out Guid userId)
        {
            var sub = controller.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? controller.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrWhiteSpace(sub) && Guid.TryParse(sub, out userId))
                return true;

            userId = Guid.Empty;
            return false;
        }

        public static IActionResult UserNotAuthenticated(this ControllerBase controller)
        {
            return controller.Unauthorized(new { Sucesso = false, Mensagem = UserNotAuthenticatedMessage });
        }

        public static IActionResult StudentProfileNotFound(this ControllerBase controller, bool includeSuccessFlag = false)
        {
            return includeSuccessFlag
                ? controller.NotFound(new { Sucesso = false, Mensagem = StudentProfileNotFoundMessage })
                : controller.NotFound(new { Mensagem = StudentProfileNotFoundMessage });
        }
    }
}