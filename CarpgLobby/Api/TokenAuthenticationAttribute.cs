using CarpgLobby.Properties;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace CarpgLobby.Api
{
    public class TokenAuthenticationAttribute : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (IsAuthorized(actionContext))
                base.OnAuthorization(actionContext);
            else
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
        }

        private bool IsAuthorized(HttpActionContext actionContext)
        {
            if (actionContext.Request.Headers.Authorization == null)
                return false;
            string token = actionContext.Request.Headers.Authorization.Parameter;
            return token == Settings.Default.ApiKey;
        }
    }
}
