using Microsoft.AspNetCore.Mvc;

namespace GW2Vault.Auth.Controllers
{
    [ApiController]
    public class HomeController : ControllerBase
    {
        [Route("[action]")]
        public ActionResult<string> Ping()
            => Ok("200 OK - GW2Vault.Auth API is up and running.");
    }
}
