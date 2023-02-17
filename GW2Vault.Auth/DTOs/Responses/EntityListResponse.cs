using System.Collections.Generic;
using GW2Vault.Auth.DTOs.Entities;

namespace GW2Vault.Auth.DTOs.Responses
{
    public class EntityListResponse<T>
        where T : IEntity
    {
        public List<T> Items { get; set; }
    }
}
