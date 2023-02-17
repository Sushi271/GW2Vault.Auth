using System;

namespace GW2Vault.Auth
{
    static class EnumHelper
    {
        public static bool IsOneOf(this Enum flagEnum, Enum flagEnumValue)
            => ((int)(object)flagEnum & (int)(object)flagEnumValue) > 0;
    }
}
