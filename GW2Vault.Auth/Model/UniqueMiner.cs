using GW2Vault.Auth.DTOs.Entities;

namespace GW2Vault.Auth.Model
{
    public class UniqueMiner : IEntity
    {
        public int Id { get; set; }
        public string Signature { get; set; }
        public int MachineId { get; set; }
        public string DownloadFilename { get; set; }
    }
}