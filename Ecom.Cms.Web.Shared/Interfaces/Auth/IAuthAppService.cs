using Ecom.Cms.Web.Shared.Models;
using Ecom.Cms.Web.Shared.Models.Auth.Models;

namespace Ecom.Cms.Web.Shared.Interfaces.Auth
{
    public interface IAuthAppService
    {
        public Task<Result<TokenResponseDto>> RefreshTokenAsync(string refreshToken);
    }
}
