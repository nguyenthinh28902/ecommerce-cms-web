using Ecom.Cms.Web.Shared.Models;
using Ecom.Cms.Web.Shared.Models.Auth.Models;

namespace Ecom.Cms.Web.Shared.Interfaces.Auth
{
    public interface IAuthAppService
    {
        string GetIdentityServerLoginUrl(string returnUrl, string codeVerifier);
        public Task<Result<TokenResponseDto>> ExchangeCodeForTokenAsync(string code, string codeVerifier);
        public Task<Result<TokenResponseDto>> RefreshTokenAsync(string refreshToken);
    }
}
