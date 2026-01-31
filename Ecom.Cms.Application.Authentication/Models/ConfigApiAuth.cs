namespace Ecom.Cms.Application.Authentication.Models
{
    public class ConfigApiAuth
    {
        public static string ConnectAuthorize = "/connect/authorize";
        public const string ConnectLogout = "/connect/endsession";
        public const string ExchangeCodeForToken = "/api/auth/trao-doi-token";
    }
}
