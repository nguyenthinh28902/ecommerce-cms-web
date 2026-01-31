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
            // Lấy Token từ Claims mà chúng ta đã lưu lúc Login thành công
            var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");

            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var response = await base.SendAsync(request, cancellationToken);

                // 3. Nếu lỗi 401 (Unauthorized) - Có thể token đã hết hạn
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var refreshToken = await _httpContextAccessor.HttpContext.GetTokenAsync("refresh_token");

                    if (!string.IsNullOrEmpty(refreshToken))
                    {
                        // Lấy AuthAppService từ RequestServices (để tránh lỗi vòng lặp DI)
                        var authService = _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<IAuthAppService>();

                        // Gọi hàm RefreshTokenAsync đã viết [cite: 2026-01-19]
                        var refreshResult = await authService.RefreshTokenAsync(refreshToken);

                        if (refreshResult.IsSuccess)
                        {
                            // Lưu cặp Token mới vào Cookie
                            await _authTokenCookie.UpdateAuthCookie(refreshResult.Data);

                            // Thay Token mới vào request cũ và thử lại lần cuối
                            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshResult.Data.AccessToken);
                            return await base.SendAsync(request, cancellationToken);
                        }
                    }
                }


            }
            // 2. Thực hiện Request
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
