using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GW2Vault.Auth.Controllers;
using GW2Vault.Auth.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GW2Vault.Auth.ActionFilters
{
    public static class ActionExecutingContextExtensions
    {
        public static ControllerBase GetController(this ActionExecutingContext context)
            => (ControllerBase)context.Controller;
        
        public static object GetItem(this ActionExecutingContext context, Type t)
        {
            context.HttpContext.Items.TryGetValue(t, out var item);
            return item;
        }

        public static void SetItem(this ActionExecutingContext context, Type t, object item)
            => context.HttpContext.Items[t] = item;

        public static T GetItem<T>(this ActionExecutingContext context)
            => (T)context.GetItem(typeof(T));

        public static void SetItem<T>(this ActionExecutingContext context, T item)
            => context.SetItem(typeof(T), item);

        public static bool TryInitDescriptor(this ActionExecutingContext context)
        {
            if (!(context.ActionDescriptor is ControllerActionDescriptor descriptor))
            {
                context.Result = context.GetController().InternalServerError();
                return false;
            }

            context.HttpContext.Items[typeof(ControllerActionDescriptor)] = descriptor;
            return true;
        }

        public static string GetControllerName(this ActionExecutingContext context)
        {
            var descriptor = context.GetItem<ControllerActionDescriptor>();
            return descriptor.ControllerName;
        }

        public static string GetActionName(this ActionExecutingContext context)
        {
            var descriptor = context.GetItem<ControllerActionDescriptor>();
            return descriptor.ActionName;
        }

        public static async Task<string> AcquireBody(this ActionExecutingContext context)
        {
            using var sr = new StreamReader(context.HttpContext.Request.Body);
            return await sr.ReadToEndAsync();
        }

        public static T GetActionFilterAttribute<T>(this ActionExecutingContext context)
            where T : IFilterMetadata
            => context.ActionDescriptor.FilterDescriptors
                .Select(x => x.Filter).OfType<T>().FirstOrDefault();

        public static void GenerateActionResult(this ActionExecutingContext context, ServiceResponse serviceResponse, string customMessage = null)
        {
            var controller = (ControllerBase)context.Controller;
            context.Result = serviceResponse.IsSuccess ?
                (ActionResult)controller.Ok() :
                controller.StatusCode(serviceResponse.ErrorDetails.Code,
                    customMessage ?? serviceResponse.ErrorDetails.Message);
        }
    }
}
