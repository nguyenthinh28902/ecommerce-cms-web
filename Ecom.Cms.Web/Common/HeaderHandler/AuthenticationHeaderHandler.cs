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
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        public AuthenticationHeaderHandler(IHttpContextAccessor httpContextAccessor,
            AuthTokenCookie authTokenCookie)
        {
            _httpContextAccessor = httpContextAccessor;
            _authTokenCookie = authTokenCookie;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null)
            {
                return await base.SendAsync(request, ct);
            }

            var accessToken = await context.GetTokenAsync("access_token");
            if (!string.IsNullOrEmpty(accessToken))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await base.SendAsync(request, ct);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await _semaphore.WaitAsync(ct);
                try
                {
                    // Kiểm tra xem token đã được cập nhật bởi thread khác chưa
                    var latestToken = await context.GetTokenAsync("access_token");
                    if (latestToken != accessToken)
                    {
                        // Nếu token đã mới hơn, dùng luôn token này để retry, không gọi API Refresh nữa
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", latestToken);
                        return await base.SendAsync(request, ct);
                    }

                    var refreshToken = await context.GetTokenAsync("refresh_token");
                    if (!string.IsNullOrEmpty(refreshToken))
                    {
                        var authService = context.RequestServices.GetRequiredService<IAuthAppService>();
                        var refreshResult = await authService.RefreshTokenAsync(refreshToken);

                        if (refreshResult?.Data != null && refreshResult.IsSuccess)
                        {
                            await _authTokenCookie.UpdateAuthCookie(refreshResult.Data);

                            // Thử lại request với token mới
                            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshResult.Data.AccessToken);
                            return await base.SendAsync(request, ct);
                        }
                    }
                }
                finally
                {
                    _semaphore.Release();
                }
            }

            return response;
        }
    }
}
