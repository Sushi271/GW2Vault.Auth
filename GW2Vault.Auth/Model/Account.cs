using System;
using GW2Vault.Auth.DTOs.Entities;

namespace GW2Vault.Auth.Model
{
    public class Account : IEntity
    {
        public int Id { get; set; }
        public string AccountName { get; set; }
        public DateTime RegistrationDate { get; set; }
        public bool Active { get; set; }
    }
}
