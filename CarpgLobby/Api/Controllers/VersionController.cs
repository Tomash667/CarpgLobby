using CarpgLobby.Api.Model;
using CarpgLobby.Provider;
using CarpgLobby.Utils;
using System.Web.Http;

namespace CarpgLobby.Api.Controllers
{
    public class VersionController : BaseController
    {
        // Get version
        [Route("api/version")]
        public GetVersionResponse GetVersion()
        {
            return HandleRequest(() =>
            {
                Logger.Verbose($"Get version from {Ip}.");
                return new GetVersionResponse
                {
                    Ok = true,
                    Version = Version.Number,
                    VersionString = Version.Current
                };
            });
        }

        // Get version
        [Route("api/version/details")]
        [TokenAuthentication]
        public GetVersionDetailsResponse GetVersionDetails()
        {
            return HandleRequest(() =>
            {
                Logger.Verbose($"Get version details from {Ip}.");
                FtpProvider ftp = new FtpProvider();
                int version2 = ftp.GetVersion();
                return new GetVersionDetailsResponse
                {
                    Ok = true,
                    Version = Version.Number,
                    VersionString = Version.Current,
                    Version2 = version2,
                    VersionString2 = Version.ToString(version2)
                };
            });
        }

        // Set version
        [Route("api/version")]
        [TokenAuthentication]
        public BaseResponse PostVersion([FromUri]string version)
        {
            return HandleRequest(() =>
            {
                Lobby.Instance.SetVersion(version, Ip);
                FtpProvider ftp = new FtpProvider();
                ftp.SetVersion(Version.ParseVersion(version));
                return new BaseResponse { Ok = true };
            });
        }
    }
}
