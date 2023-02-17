using GW2Vault.Auth.DTOs.Requests;
using GW2Vault.Auth.DTOs.Responses;
using GW2Vault.Auth.Infrastructure;

namespace GW2Vault.Auth.Services
{
    public interface ILicensingService
    {
        ServiceResponse ProcessActivation(ActivationRequest activationDto);
        ServiceResponse ProcessVerification(VerifyLicenseRequest request);
        ServiceResponse<UnlockTokenResponse> GenerateUnlockToken(VerifyLicenseRequest request);
    }
}
