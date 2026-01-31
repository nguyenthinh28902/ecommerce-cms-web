using Ecom.Cms.Application.User.Models;
using Ecom.Cms.Web.Shared.Interfaces.User;
using Ecom.Cms.Web.Shared.Models;
using Ecom.Cms.Web.Shared.Models.Settings;
using Ecom.Cms.Web.Shared.Models.User;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Ecom.Cms.Application.User.Services
{
    public class UserInformation : IUserInformation
    {
        private readonly ILogger<UserInformation> _logger;
        private readonly SystemConfig _config;
        private readonly HttpClient _httpClient;
        public UserInformation(ILogger<UserInformation> logger,
            IOptions<SystemConfig> options,
           HttpClient httpClient)
        {
            _logger = logger;
            _config = options.Value;
            _httpClient = httpClient;
        }
        public async Task<Result<UserInforDto>> GetUserInfoAsync(string accessToken)
        {
            try
            {
                // 1. Endpoint lấy thông tin user (đã cấu hình trong proxy-config.yaml) [cite: 2026-01-19]
                var userInfoUrl = ConfigApiUser.GetUserInfo;

                // 2. Thiết lập Token vào Header
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                // 3. Gọi API lấy dữ liệu
                var response = await _httpClient.GetAsync(userInfoUrl);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("GetUserInfoAsync: Không thể lấy thông tin User. Identity Server trả về: {StatusCode}", response.StatusCode);
                    return Result<UserInforDto>.Failure("Xác thực thông tin người dùng thất bại.");
                }

                // 4. Giải mã dữ liệu (Hứng đúng cấu trúc Result<UserInforDto>)
                var result = await response.Content.ReadFromJsonAsync<Result<UserInforDto>>();

                if (result == null || !result.IsSuccess)
                {
                    return Result<UserInforDto>.Failure("Dữ liệu User nhận được không hợp lệ.");
                }

                _logger.LogInformation("GetUserInfoAsync: Đã lấy thông tin chi tiết cho User {UserName} thành công.", result.Data?.UserName);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetUserInfoAsync: Lỗi kết nối khi truy vấn thông tin User qua Gateway");
                return Result<UserInforDto>.Failure("Lỗi hệ thống khi lấy thông tin chi tiết.");
            }
        }
    }
}
