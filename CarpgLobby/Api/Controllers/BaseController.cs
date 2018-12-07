using CarpgLobby.Api.Model;
using CarpgLobby.Provider;
using CarpgLobby.Utils;
using System;
using System.Net.Http;
using System.Web.Http;

namespace CarpgLobby.Api.Controllers
{
    public class BaseController : ApiController
    {
        protected T HandleRequest<T>(Func<T> func) where T : BaseResponse, new()
        {
            try
            {
                return func();
            }
            catch (ProviderException ex)
            {
                return new T
                {
                    Ok = false,
                    Error = ex.Message
                };
            }
            catch (Exception ex)
            {
                Logger.Error($"Unhandled exception from {Ip}: {ex}");
                return new T
                {
                    Ok = false,
                    Error = ex.Message
                };
            }
        }

        protected string Ip => Request.GetOwinContext()?.Request?.RemoteIpAddress ?? "unknown";
    }
}
