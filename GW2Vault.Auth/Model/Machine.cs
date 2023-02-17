using System;
using GW2Vault.Auth.DTOs.Entities;

namespace GW2Vault.Auth.Model
{
    public class Machine : IEntity
    {
        public int Id { get; set; }
        public string PcIdentifier { get; set; }
        public int AccountId { get; set; }
        public bool Active { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
