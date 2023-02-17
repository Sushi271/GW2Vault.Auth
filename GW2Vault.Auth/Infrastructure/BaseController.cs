using Microsoft.AspNetCore.Mvc;

namespace GW2Vault.Auth.Infrastructure
{
    public abstract class BaseController : ControllerBase
    {
        protected string ControllerName => ControllerContext.ActionDescriptor.ControllerName;
        protected string CurrentActionName => ControllerContext.ActionDescriptor.ActionName;

        protected ActionResult ProduceActionResult(ServiceResponse serviceResponse, string customMessage = null)
            => serviceResponse.IsSuccess ? (ActionResult)Ok() :
                StatusCode(serviceResponse.ErrorDetails.Code,
                    customMessage ?? serviceResponse.ErrorDetails.Message);
    }
}
