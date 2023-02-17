using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GW2Vault.Auth.ActionFilters
{
    public class RequestConfigAttribute : Attribute, IFilterMetadata
    {
        public Type DtoType { get; }
        public bool DeserializeFromJson { get; set; }
        public bool Decrypt { get; set; }

        public RequestConfigAttribute(Type dtoType)
            => DtoType = dtoType;
    }
}
