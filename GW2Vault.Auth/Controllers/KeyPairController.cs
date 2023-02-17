using GW2Vault.Auth.ActionFilters;
using GW2Vault.Auth.DTOs.Entities;
using GW2Vault.Auth.DTOs.Requests;
using GW2Vault.Auth.DTOs.Responses;
using GW2Vault.Auth.Infrastructure;
using GW2Vault.Auth.Services;
using Microsoft.AspNetCore.Mvc;

namespace GW2Vault.Auth.Controllers
{
    [ApiController]
    public class KeyPairController : BaseController
    {
        IAuthenticationService AuthenticationService { get; }
        IKeyPairService KeyPairService { get; }

        public KeyPairController(IAuthenticationService authenticationService,
            IKeyPairService keyConfigService)
        {
            AuthenticationService = authenticationService;
            KeyPairService = keyConfigService;
        }

        [HttpPost("api/[controller]/[action]")]
        [Consumes("text/plain")]
        [Produces("text/plain")]
        [RequestConfig(typeof(CredentialsRequest), Decrypt = true, DeserializeFromJson = true)]
        [ResponseConfig(Encrypt = true, SerializeToJson = true)]
        public IActionResult List()
        {
            var dto = this.GetRequestBody<CredentialsRequest>();

            var authenticationResponse = AuthenticationService.Authenticate(dto.Username, dto.PasswordHash);
            if (!authenticationResponse.IsSuccess)
                return Unauthorized("Authentication failed. Request denied.");

            var getKeyPairListResponse = KeyPairService.GetKeyPairList();
            if (!getKeyPairListResponse.IsSuccess)
                return StatusCode(500, "Internal server error.");

            var response = new EntityListResponse<SimplifiedKeyPairEntity> { Items = getKeyPairListResponse.ResponseDTO };
            return Ok(response);
        }

        [HttpPost("api/[controller]/[action]")]
        [Consumes("text/plain")]
        [Produces("text/plain")]
        [RequestConfig(typeof(EntityRequest), Decrypt = true, DeserializeFromJson = true)]
        [ResponseConfig(Encrypt = true, SerializeToJson = true)]
        public IActionResult Get()
        {
            var dto = this.GetRequestBody<EntityRequest>();

            var authenticationResponse = AuthenticationService.Authenticate(dto.Username, dto.PasswordHash);
            if (!authenticationResponse.IsSuccess)
                return Unauthorized("Authentication failed. Request denied.");

            var getKeyPairResponse = KeyPairService.GetKeyPair(dto.Id.Value);
            if (!getKeyPairResponse.IsSuccess)
            {
                if (getKeyPairResponse.ErrorDetails.Code == 404)
                    return NotFound(getKeyPairResponse.ErrorDetails.Message);
                return StatusCode(500, "Internal server error.");
            }

            var responseDto = new KeyPairResponse
            {
                Id = getKeyPairResponse.ResponseDTO.Id,
                EntityDetails = getKeyPairResponse.ResponseDTO
            };
            return Ok(responseDto);
        }

        [HttpPost("api/[controller]/[action]")]
        [Consumes("text/plain")]
        [Produces("text/plain")]
        [RequestConfig(typeof(SaveKeyPairRequest), Decrypt = true, DeserializeFromJson = true)]
        [ResponseConfig(Encrypt = true, SerializeToJson = true)]
        public IActionResult Save()
        {
            var dto = this.GetRequestBody<SaveKeyPairRequest>();

            var authenticationResponse = AuthenticationService.Authenticate(dto.Username, dto.PasswordHash);
            if (!authenticationResponse.IsSuccess)
                return Unauthorized("Authentication failed. Request denied.");

            var addKeyPairResponse = KeyPairService.SaveKeyPair(dto);
            if (!addKeyPairResponse.IsSuccess)
            {
                if (addKeyPairResponse.ErrorDetails.Code == 404)
                    return NotFound(addKeyPairResponse.ErrorDetails.Message);
                return StatusCode(500, "Internal server error.");
            }
            
            var responseDto = new KeyPairResponse
            {
                Id = addKeyPairResponse.ResponseDTO.Id,
                EntityDetails = addKeyPairResponse.ResponseDTO
            };
            return Ok(responseDto);
        }

        [HttpPost("api/[controller]/[action]")]
        [Consumes("text/plain")]
        [Produces("text/plain")]
        [RequestConfig(typeof(EntityRequest), Decrypt = true, DeserializeFromJson = true)]
        public IActionResult Remove()
        {
            var dto = this.GetRequestBody<EntityRequest>();

            var authenticationResponse = AuthenticationService.Authenticate(dto.Username, dto.PasswordHash);
            if (!authenticationResponse.IsSuccess)
                return Unauthorized("Authentication failed. Request denied.");
            
            var removeKeyPairResponse = KeyPairService.RemoveKeyPair(dto.Id.Value);
            if (!removeKeyPairResponse.IsSuccess)
            {
                if (removeKeyPairResponse.ErrorDetails.Code == 404)
                    return NotFound(removeKeyPairResponse.ErrorDetails.Message);
                return StatusCode(500, "Internal server error.");
            }

            return Ok();
        }
    }
}
