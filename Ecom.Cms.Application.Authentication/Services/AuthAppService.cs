using Ecom.Cms.Web.Shared.Interfaces.Auth;
using Ecom.Cms.Web.Shared.Models;
using Ecom.Cms.Web.Shared.Models.Auth.Models;
using Ecom.Cms.Web.Shared.Models.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
namespace Ecom.Cms.Application.Authentication.Services
{
    public class AuthAppService : IAuthAppService
    {
        private readonly ILogger<AuthAppService> _logger;
        private readonly ConfigClientIdentity _config;
        private readonly HttpClient _httpClient;
        public AuthAppService(ILogger<AuthAppService> logger,
            IOptions<ConfigClientIdentity> options,
            IHttpClientFactory httpClientFactory,
            HttpClient httpClient)
        {
            _logger = logger;
            _config = options.Value;
            _httpClient = httpClient;
        }

        public async Task<Result<TokenResponseDto>> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                _logger.LogInformation("Đang tiến hành làm mới Access Token bằng Refresh Token...");

                // 1. Chuẩn bị Payload theo chuẩn OAuth2 Grant Type 'refresh_token'
                var dict = new Dictionary<string, string>
                {
                    { "grant_type", "refresh_token" },
                    { "refresh_token", refreshToken },
                    { "client_id", _config.ClientId }, // Ví dụ: cms_admin_client [cite: 2026-01-19]
                    { "scope", _config.AuthScope }     // openid profile offline_access
                };

                // 2. Identity Server thường yêu cầu dữ liệu dạng FormUrlEncoded cho endpoint Token
                var requestContent = new FormUrlEncodedContent(dict);

                // 3. Gọi qua Gateway (Endpoint đã được định nghĩa trong proxy-config.yaml) [cite: 2026-01-19]
                var response = await _httpClient.PostAsync("api/auth/connect/token", requestContent);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Làm mới Token thất bại. Gateway trả về: {Status} - {Error}", response.StatusCode, errorContent);
                    return Result<TokenResponseDto>.Failure("Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại.");
                }

                // 4. Đọc dữ liệu Token mới
                var result = await response.Content.ReadFromJsonAsync<TokenResponseDto>();

                if (result == null || string.IsNullOrEmpty(result.AccessToken))
                {
                    return Result<TokenResponseDto>.Failure("Dữ liệu Token mới không hợp lệ.");
                }

                _logger.LogInformation("Làm mới Access Token thành công.");
                return Result<TokenResponseDto>.Success(result, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi ngoại lệ khi gọi RefreshToken qua Gateway");
                return Result<TokenResponseDto>.Failure("Hệ thống gặp sự cố khi gia hạn phiên làm việc.");
            }
        }
    }
}
