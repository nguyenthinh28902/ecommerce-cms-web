using Ecom.Cms.Web.Common.AuthCookie;
using Ecom.Cms.Web.Shared.Interfaces.Auth;
using Microsoft.AspNetCore.Authentication;
using System.Net;
using System.Net.Http.Headers;

namespace Ecom.Cms.Web.Common.HeaderHandler
{
    public class AuthenticationHeaderHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AuthTokenCookie _authTokenCookie;

        public AuthenticationHeaderHandler(IHttpContextAccessor httpContextAccessor,
            AuthTokenCookie authTokenCookie)
        {
            _httpContextAccessor = httpContextAccessor;
            _authTokenCookie = authTokenCookie;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");

            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            var response = await base.SendAsync(request, cancellationToken);

            // Xử lý làm mới token nếu API trả về lỗi không được phép
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshToken = await _httpContextAccessor.HttpContext.GetTokenAsync("refresh_token");

                if (!string.IsNullOrEmpty(refreshToken))
                {
                    var authService = _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<IAuthAppService>();
                    var refreshResult = await authService.RefreshTokenAsync(refreshToken);

                    if (refreshResult.IsSuccess)
                    {
                        await _authTokenCookie.UpdateAuthCookie(refreshResult.Data);

                        // Thử lại request với token mới
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshResult.Data.AccessToken);
                        return await base.SendAsync(request, cancellationToken);
                    }
                }
            }

            return response;
        }
    }
}
