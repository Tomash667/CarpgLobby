using CarpgLobby.Api.Model;
using CarpgLobby.Provider;
using CarpgLobby.Utils;
using System.Collections.Generic;
using System.Web.Http;

namespace CarpgLobby.Api.Controllers
{
    public class VersionController : BaseController
    {
        // Get version
        [Route("api/version")]
        public GetVersionResponse GetVersion(int? ver = null, string lang = null)
        {
            return HandleRequest(() =>
            {
                if (ver.HasValue)
                {
                    Logger.Verbose($"Get new version from {Ip}.");
                    GetVersionResponse response = new GetVersionResponse
                    {
                        Ok = true,
                        Version = Lobby.Instance.Version,
                        VersionString = Lobby.Instance.VersionStr
                    };
                    if (ver.Value != response.Version)
                    {
                        response.Changelog = Lobby.Instance.GetChangelogSimple(lang);
                        response.Update = Lobby.Instance.CanUpdate(ver.Value);
                    }
                    return response;
                }
                else
                {
                    Logger.Verbose($"Get version from {Ip}.");
                    return new GetVersionResponse
                    {
                        Ok = true,
                        Version = Lobby.Instance.Version,
                        VersionString = Lobby.Instance.VersionStr
                    };
                }
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
                    Version = Lobby.Instance.Version,
                    VersionString = Lobby.Instance.VersionStr,
                    Version2 = version2,
                    VersionString2 = Version.ToString(version2)
                };
            });
        }

        // Set version
        [Route("api/version")]
        [TokenAuthentication]
        public BaseResponse PostVersion([FromUri] string version, [FromBody] Dictionary<string, string> changelog)
        {
            return HandleRequest(() =>
            {
                Logger.Info($"Set version '{version}' from {Ip}.");
                Lobby.Instance.SetVersion(version);
                if (changelog?.Count > 0)
                {
                    foreach (var item in changelog)
                    {
                        Logger.Info($"Set changelog '{item.Key}' to '{item.Value}' from {Ip}.");
                        Lobby.Instance.SetChangelog(item.Key, item.Value);
                    }
                }
                return new BaseResponse { Ok = true };
            });
        }

        // Get version changelog (for all or single language)
        [Route("api/version/changelog")]
        public GetChangelogResponse GetChangelog([FromUri] string lang)
        {
            return HandleRequest(() =>
            {
                Logger.Verbose($"Get changelog '{lang}' from {Ip}.");
                return new GetChangelogResponse
                {
                    Ok = true,
                    Changes = Lobby.Instance.GetChangelog(lang)
                };
            });
        }

        // Set version changelog for language
        [Route("api/version/changelog")]
        [TokenAuthentication]
        public BaseResponse PostChangelog([FromUri] string lang, [FromBody] string changelog)
        {
            return HandleRequest(() =>
            {
                Logger.Info($"Set changelog '{lang}' to '{changelog}' from {Ip}.");
                Lobby.Instance.SetChangelog(lang, changelog);
                return new BaseResponse { Ok = true };
            });
        }
    }
}
