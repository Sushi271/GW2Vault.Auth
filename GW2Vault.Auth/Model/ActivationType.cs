using System;

namespace GW2Vault.Auth.Model
{
    [Flags]
    public enum ActivationType
    {
        None = 0,
        Registration = 1,
        Continuation = 2,
        Both = 3
    }
}
