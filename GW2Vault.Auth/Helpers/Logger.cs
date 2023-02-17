using System;
using System.IO;

namespace GW2Vault.Auth.Helpers
{
    public static class Logger
    {
        const string Directory = "/var/www/publishapplication/log/";

        static bool loggingFailed = false;

        public static void Log(string text)
        {
            if (loggingFailed) return;
            try
            {
                var filename = $"{Directory}{DateTime.UtcNow.Date:yyyy-MM-dd}_log.txt";
                File.AppendAllText(filename, $"[{DateTime.UtcNow}] {text}\n");
            }
            catch (Exception ex)
            {
                loggingFailed = true;
                File.AppendAllText("/tmp/loggingException.txt", ex.ToString());
            }
        }
    }
}
