using Ecom.Cms.Web.Shared.Models.Auth.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Ecom.Cms.Web.Common.AuthCookie
{
    public class AuthTokenCookie
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Inject IHttpContextAccessor vào qua Constructor
        public AuthTokenCookie(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task UpdateAuthCookie(TokenResponseDto tokenResponse)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return;

            // 1. Lấy thông tin Authenticate hiện tại
            var authResult = await httpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (authResult.Succeeded && authResult.Principal != null)
            {
                // 2. Cập nhật các Token mới vào AuthenticationProperties
                authResult.Properties.StoreTokens(new[]
                {
                new AuthenticationToken { Name = "access_token", Value = tokenResponse.AccessToken },
                new AuthenticationToken { Name = "refresh_token", Value = tokenResponse.RefreshToken }
            });

                // Quan trọng: Giữ cho phiên đăng nhập tồn tại lâu dài (Persistent)
                authResult.Properties.IsPersistent = true;

                // 3. Ghi đè lại Cookie cũ
                await httpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    authResult.Principal,
                    authResult.Properties);
            }
        }
    }
}
