using System;
using System.Text;
using GW2Vault.Auth.Helpers;
using GW2Vault.Auth.Infrastructure;
using GW2Vault.Auth.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

namespace GW2Vault.Auth.ActionFilters
{
    public sealed class EncryptionAuthenticationFilter : IActionFilter
    {
        IEncryptionService EncryptionService { get; }

        public EncryptionAuthenticationFilter(IEncryptionService encryptionService)
            => EncryptionService = encryptionService;

        public void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                Logger.Log("EncryptionAuthenticationFilter.OnActionExecuting - START");
                if (!context.TryInitDescriptor())
                {
                    Logger.Log($"Init Descriptor for {context.ActionDescriptor.DisplayName} failed.");
                    return;
                }
                Logger.Log($"Running decryption before {context.GetControllerName()}.{context.GetActionName()} ({context.ActionDescriptor.DisplayName})");

                var requestConfig = context.GetActionFilterAttribute<RequestConfigAttribute>();

                if (requestConfig != null)
                {
                    var requestBody = context.AcquireBody().Result;
                    if (requestConfig.Decrypt)
                    {
                        var bytes = Convert.FromBase64String(requestBody);
                        var decryptResponse = EncryptionService.ComplexDecrypt(
                            context.GetControllerName(), context.GetActionName(), bytes);
                        if (!decryptResponse.IsSuccess)
                        {
                            Logger.Log("Body decryption failed. Request denied.");
                            context.GenerateActionResult(ServiceResponse.Unauthorized("Body decryption failed. Request denied."));
                            return;
                        }
                        requestBody = Encoding.UTF8.GetString(decryptResponse.ResponseDTO);
                    }

                    if (requestConfig.DeserializeFromJson)
                    {
                        var dto = JsonConvert.DeserializeObject(requestBody, requestConfig.DtoType);
                        context.HttpContext.Items["__REQUESTBODY__"] = dto;
                        Logger.Log(dto?.ToString() ?? "NULL");
                        Logger.Log(requestBody);
                    }
                }
                Logger.Log("EncryptionAuthenticationFilter.OnActionExecuting - END");
            }
            catch (Exception ex)
            {
                context.GenerateActionResult(ServiceResponse.InternalServerError());
                Logger.Log(ex.ToString());
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (!(context.Result is ObjectResult result))
                return;

            var responseConfig = context.GetActionFilterAttribute<ResponseConfigAttribute>();
            if (responseConfig != null)
            {
                if (result.StatusCode != 200 && responseConfig.DoNothingWhenError)
                    return;

                string responseBody;
                if (responseConfig.SerializeToJson)
                    responseBody = JsonConvert.SerializeObject(result.Value);
                else responseBody = result.Value.ToString();

                if (responseConfig.Encrypt)
                {
                    var bytes = Encoding.UTF8.GetBytes(responseBody);
                    var encryptResponse = EncryptionService.ComplexEncrypt(
                        context.GetControllerName(), context.GetActionName(), bytes);
                    if (!encryptResponse.IsSuccess)
                    {
                        context.GenerateActionResult(ServiceResponse.InternalServerError());
                        return;
                    }
                    responseBody = Convert.ToBase64String(encryptResponse.ResponseDTO);
                }

                result.Value = responseBody;
            }

        }
    }
}
