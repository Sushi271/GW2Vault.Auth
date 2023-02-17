using System;
using System.IO;
using System.Linq;
using GW2Vault.Auth.Controllers;
using GW2Vault.Auth.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GW2Vault.Auth.ActionFilters
{
    public static class ActionExecutedContextExtensions
    {
        public static ControllerBase GetController(this ActionExecutedContext context)
            => (ControllerBase)context.Controller;
        
        public static object GetItem(this ActionExecutedContext context, Type t)
        {
            context.HttpContext.Items.TryGetValue(t, out var item);
            return item;
        }

        public static void SetItem(this ActionExecutedContext context, Type t, object item)
            => context.HttpContext.Items[t] = item;

        public static T GetItem<T>(this ActionExecutedContext context)
            => (T)context.GetItem(typeof(T));

        public static void SetItem<T>(this ActionExecutedContext context, T item)
            => context.SetItem(typeof(T), item);

        public static bool TryInitDescriptor(this ActionExecutedContext context)
        {
            if (!(context.ActionDescriptor is ControllerActionDescriptor descriptor))
            {
                context.Result = context.GetController().InternalServerError();
                return false;
            }

            context.HttpContext.Items[typeof(ControllerActionDescriptor)] = descriptor;
            return true;
        }

        public static string GetControllerName(this ActionExecutedContext context)
        {
            var descriptor = context.GetItem<ControllerActionDescriptor>();
            return descriptor.ControllerName;
        }

        public static string GetActionName(this ActionExecutedContext context)
        {
            var descriptor = context.GetItem<ControllerActionDescriptor>();
            return descriptor.ActionName;
        }

        public static byte[] AcquireBody(this ActionExecutedContext context)
        {
            byte[] bytes;
            using (var ms = new MemoryStream())
            {
                context.HttpContext.Request.Body.CopyToAsync(ms).Wait();
                bytes = ms.ToArray();
            }
            return bytes;
        }

        public static T GetActionFilterAttribute<T>(this ActionExecutedContext context)
            where T : IFilterMetadata
            => context.ActionDescriptor.FilterDescriptors
                .Select(x => x.Filter).OfType<T>().FirstOrDefault();

        public static void GenerateActionResult(this ActionExecutedContext context, ServiceResponse serviceResponse, string customMessage = null)
        {
            var controller = (ControllerBase)context.Controller;
            context.Result = serviceResponse.IsSuccess ?
                (ActionResult)controller.Ok() :
                controller.StatusCode(serviceResponse.ErrorDetails.Code,
                    customMessage ?? serviceResponse.ErrorDetails.Message);
        }
    }
}
