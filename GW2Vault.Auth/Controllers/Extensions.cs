using Microsoft.AspNetCore.Mvc;

namespace GW2Vault.Auth.Controllers
{
    public static class Extensions
    {
        public static IActionResult BadRequest(this ControllerBase controller, string customMessage = null)
            => controller.StatusCode(400, customMessage ?? "Bad Request.");

        public static IActionResult Unauthorized(this ControllerBase controller, string customMessage = null)
            => controller.StatusCode(401, customMessage ?? "Unauthorized.");

        public static IActionResult Forbidden(this ControllerBase controller, string customMessage = null)
            => controller.StatusCode(403, customMessage ?? "Forbidden.");
        
        public static IActionResult InternalServerError(this ControllerBase controller, string customMessage = null)
            => controller.StatusCode(500, customMessage ?? "Internal Server Error.");

        public static T GetRequestBody<T>(this ControllerBase controller)
        {
            controller.HttpContext.Items.TryGetValue("__REQUESTBODY__", out var item);
            return (T)item;
        }
    }
}
