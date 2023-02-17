using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GW2Vault.Auth.ActionFilters
{
    public class ResponseConfigAttribute : Attribute, IFilterMetadata
    {
        public bool SerializeToJson { get; set; }
        public bool Encrypt { get; set; }
        public bool DoNothingWhenError { get; set; }
    }
}
