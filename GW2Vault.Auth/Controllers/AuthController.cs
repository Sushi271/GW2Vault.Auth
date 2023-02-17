using GW2Vault.Auth.ActionFilters;
using GW2Vault.Auth.DTOs.Requests;
using GW2Vault.Auth.Helpers;
using GW2Vault.Auth.Infrastructure;
using GW2Vault.Auth.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GW2Vault.Auth.Controllers
{
    [ApiController]
    public class AuthController : BaseController
    {
        ILicensingService LicensingService { get; }

        public AuthController(ILicensingService licensingService)
            => LicensingService = licensingService;

        [HttpPost("api/[controller]/[action]")]
        [Consumes("text/plain")]
        [RequestConfig(typeof(ActivationRequest), Decrypt = true, DeserializeFromJson = true)]
        public IActionResult Activation()
        {
            Logger.Log("AuthController.Activation - START");

            var request = this.GetRequestBody<ActivationRequest>();

            var activationResponse = LicensingService.ProcessActivation(request);
            if (!activationResponse.IsSuccess)
            {
                Logger.Log("activationResponse.IsSuccess == false");
                if (activationResponse.ErrorDetails.Code == StatusCodes.Status500InternalServerError)
                    return this.InternalServerError();
                return ProduceActionResult(activationResponse);
            }

            Logger.Log("AuthController.Activation - END");
            return Ok();
        }

        [HttpPost("api/[controller]/[action]")]
        [Consumes("text/plain")]
        [RequestConfig(typeof(VerifyLicenseRequest), Decrypt = true, DeserializeFromJson = true)]
        [ResponseConfig(Encrypt = true, SerializeToJson = true, DoNothingWhenError = true)]
        public IActionResult VerifyLicense()
        {
            Logger.Log("AuthController.VerifyLicense - START");
            var request = this.GetRequestBody<VerifyLicenseRequest>();

            var verificationResponse = LicensingService.ProcessVerification(request);
            if (!verificationResponse.IsSuccess)
            {
                Logger.Log("verificationResponse.IsSuccess == false");
                if (verificationResponse.ErrorDetails.Code == StatusCodes.Status500InternalServerError)
                    return this.InternalServerError();
                return ProduceActionResult(verificationResponse);
            }

            var unlockTokenResponse = LicensingService.GenerateUnlockToken(request);
            if (!unlockTokenResponse.IsSuccess)
            {
                if (unlockTokenResponse.ErrorDetails.Code == StatusCodes.Status500InternalServerError)
                    return this.InternalServerError();
                return ProduceActionResult(unlockTokenResponse);
            }

            Logger.Log("AuthController.VerifyLicense - END");
            return Ok(unlockTokenResponse.ResponseDTO);
        }
    }
}
