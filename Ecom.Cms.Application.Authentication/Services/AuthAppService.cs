using Ecom.Cms.Application.Authentication.Models;
using Ecom.Cms.Web.Shared.Interfaces.Auth;
using Ecom.Cms.Web.Shared.Models;
using Ecom.Cms.Web.Shared.Models.Auth.Models;
using Ecom.Cms.Web.Shared.Models.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;
namespace Ecom.Cms.Application.Authentication.Services
{
    public class AuthAppService : IAuthAppService
    {
        private readonly ILogger<AuthAppService> _logger;
        private readonly SystemConfig _config;
        private readonly HttpClient _httpClient;
        public AuthAppService(ILogger<AuthAppService> logger,
            IOptions<SystemConfig> options,
            IHttpClientFactory httpClientFactory,
            HttpClient httpClient)
        {
            _logger = logger;
            _config = options.Value;
            _httpClient = httpClient;
        }

        public string GetIdentityServerLoginUrl(string redirectUri, string codeChallenge)
        {
            // 1. Đảm bảo đường dẫn URL chuẩn xác
            var identityServerUrl = $"{_config.GatewayUrl}{ConfigApiAuth.ConnectAuthorize}";

            // 2. Encode các tham số để an toàn khi truyền trên URL
            var encodedClientId = Uri.EscapeDataString(_config.ClientId);
            var encodedScope = Uri.EscapeDataString(_config.AuthScope);
            var encodedCallbackUrl = Uri.EscapeDataString(_config.CallbackUrl);
            var encodedChallenge = Uri.EscapeDataString(codeChallenge);
            var encodedState = Uri.EscapeDataString(redirectUri);


            // 3. Xây dựng URL với đầy đủ tham số PKCE
            // code_challenge_method=S256 cho Identity Server biết chúng ta dùng SHA256
            return $"{identityServerUrl}" +
                   $"?client_id={encodedClientId}" +
                   $"&response_type=code" +
                   $"&scope={encodedScope}" +
                   $"&redirect_uri={encodedCallbackUrl}" +
                   $"&state={encodedState}" +
                   $"&code_challenge={encodedChallenge}" +
                   $"&code_challenge_method=S256";
        }

        public async Task<Result<TokenResponseDto>> ExchangeCodeForTokenAsync(string code, string codeVerifier)
        {
            // 1. Chuẩn bị dữ liệu Body (Theo chuẩn OAuth2: application/x-www-form-urlencoded)
            // Chuyển từ anonymous object sang Dictionary để dùng cho FormUrlEncodedContent
            var dict = new Dictionary<string, string>
            {
                { "code", code },
                { "redirect_uri", _config.CallbackUrl },
                { "code_verifier", codeVerifier },
            };

            var content = new FormUrlEncodedContent(dict);

            try
            {
                // 2. Tạo RequestMessage để can thiệp vào Header
                var exchangeUrl = ConfigApiAuth.ExchangeCodeForToken;
                var request = new HttpRequestMessage(HttpMethod.Post, exchangeUrl) {
                    Content = content
                };

                // 3. Cấu hình Basic Auth Header: Base64(client_id:client_secret)
                var authString = Convert.ToBase64String(
                    System.Text.Encoding.ASCII.GetBytes($"{_config.ClientId}:{_config.ClientSecret}"));

                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authString);

                // Log thông tin để kiểm tra (tương tự hàm trên)
                _logger.LogInformation($"data form client change token: {JsonSerializer.Serialize(dict)}");

                // 4. Thực thi gửi request bằng SendAsync thay vì PostAsJsonAsync
                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorDetails = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Lỗi Identity Server: {Status} - {Details}", response.StatusCode, errorDetails);
                    return Result<TokenResponseDto>.Failure($"Lỗi xác thực: {response.StatusCode}");
                }

                // 5. Đọc dữ liệu trả về
                var result = await response.Content.ReadFromJsonAsync<Result<TokenResponseDto>>();

                if (result == null || result.Data == null)
                {
                    return Result<TokenResponseDto>.Failure("Dữ liệu trả về từ Identity Server bị trống.");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExchangeCodeForTokenAsync: Lỗi khi trao đổi token qua Gateway");
                return Result<TokenResponseDto>.Failure("Hệ thống xác thực tạm thời gián đoạn.");
            }
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
