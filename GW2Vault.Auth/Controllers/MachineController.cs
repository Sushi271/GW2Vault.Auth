using GW2Vault.Auth.ActionFilters;
using GW2Vault.Auth.DTOs.Requests;
using GW2Vault.Auth.DTOs.Responses;
using GW2Vault.Auth.Infrastructure;
using GW2Vault.Auth.Model;
using GW2Vault.Auth.Services;
using Microsoft.AspNetCore.Mvc;

namespace GW2Vault.Auth.Controllers
{
    [ApiController]
    public class MachineController : BaseController
    {
        IAuthenticationService AuthenticationService { get; }
        IMachineService MachineService { get; }

        public MachineController(IAuthenticationService authenticationService,
            IMachineService machineService)
        {
            AuthenticationService = authenticationService;
            MachineService = machineService;
        }

        [HttpPost("api/[controller]/[action]")]
        [Consumes("text/plain")]
        [Produces("text/plain")]
        [RequestConfig(typeof(SearchByAccountIdRequest), Decrypt = true, DeserializeFromJson = true)]
        [ResponseConfig(Encrypt = true, SerializeToJson = true)]
        public IActionResult List()
        {
            var dto = this.GetRequestBody<SearchByAccountIdRequest>();

            var authenticationResponse = AuthenticationService.Authenticate(dto.Username, dto.PasswordHash);
            if (!authenticationResponse.IsSuccess)
                return Unauthorized("Authentication failed. Request denied.");

            var getKeyPairListResponse = MachineService.SearchMachinesByAccount(dto.AccountId);
            if (!getKeyPairListResponse.IsSuccess)
                return StatusCode(500, "Internal server error.");

            var response = new EntityListResponse<Machine> { Items = getKeyPairListResponse.ResponseDTO };
            return Ok(response);
        }

    }
}
