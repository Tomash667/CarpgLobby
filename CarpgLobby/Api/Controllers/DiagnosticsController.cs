using System.Web.Http;

namespace CarpgLobby.Api.Controllers
{
    public class DiagnosticsController : BaseController
    {
        [Route("api/diagnostics")]
        public IHttpActionResult GetIsAlive()
        {
            return Ok();
        }
    }
}
