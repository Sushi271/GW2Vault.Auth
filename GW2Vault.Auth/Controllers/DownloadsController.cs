using GW2Vault.Auth.Model;
using Microsoft.AspNetCore.Mvc;

namespace GW2Vault.Auth.Controllers
{
    [ApiController]
    public class DownloadsController : Controller
    {
        const string MinerFullEnabledFilename = "__MinerFullEnabled__";

        const string DownloadsPath = "/var/www/publishapplication/downloads/";
        const string MinerDirectory = "GW2Vault.Miner";
        const string MinerFullFilename = "GW2Vault.MinerFull";
        const string ActivationFilename = "GW2Vault.Activation";

        [HttpGet("[controller]/[action]")]
        public IActionResult Activation()
        {
            var filePath = $"{DownloadsPath}{ActivationFilename}";
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            var contentType = "application/force-download";
            return File(fileBytes, contentType, ActivationFilename);
        }
        
        [HttpGet("[controller]/[action]")]
        public IActionResult Miner(string appKey)
        {
            if (appKey == null)
                return NotFound();

            var filePath = $"{DownloadsPath}{MinerDirectory}/{appKey}";
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var contentType = "application/force-download";
            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, contentType, MinerDirectory);
        }

        [HttpGet("[controller]/[action]")]
        public IActionResult MinerFull()
        {
            var enabledFilePath = $"{DownloadsPath}{MinerFullEnabledFilename}";
            if (!System.IO.File.Exists(enabledFilePath))
                return Forbid();

            var text = System.IO.File.ReadAllText(enabledFilePath);
            if (!bool.TryParse(text, out var enabled) || !enabled)
                return Forbid();

            var filePath = $"{DownloadsPath}{MinerFullFilename}";
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var contentType = "application/force-download";
            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, contentType, MinerFullFilename);
        }
    }
}
