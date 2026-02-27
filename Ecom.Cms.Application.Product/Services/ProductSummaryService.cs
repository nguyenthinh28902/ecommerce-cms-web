using Ecom.Cms.Application.Product.Interfaces;
using Ecom.Cms.Web.Shared.Interfaces.User;
using Ecom.Cms.Web.Shared.Models.Dashboard;
using Ecom.Cms.Web.Shared.Models.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Ecom.Cms.Web.Shared.Models.AuthWeb;
using Ecom.Cms.Application.Product.Models;
using Ecom.Cms.Web.Shared.Models;
using System.Net.Http.Json;

namespace Ecom.Cms.Application.Product.Services
{
    public class ProductSummaryService : IProductSummaryService
    {
        private readonly ILogger<ProductSummaryService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ProductSummaryService(ILogger<ProductSummaryService> logger, HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor) { 
            _logger = logger;
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<DashboardViewModel>> GetProductSummaryDashboard()
        {
            var deptCodes = _httpContextAccessor.HttpContext?.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            if (deptCodes == null || !deptCodes.Any(x => x == DepartmentCode.Content.ToString())) return Result<DashboardViewModel>.Failure("Lỗi khi lấy thông tin."); var productSummaryDashboardUrl = ConfigApiProductService.GetProductSummary;
            var response = await _httpClient.GetAsync(productSummaryDashboardUrl);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"{nameof(GetProductSummaryDashboard)}: Không thể lấy thông tin: {response.StatusCode}");
                return Result<DashboardViewModel>.Failure($"Lỗi hệ thống khi lấy thông tin. {response.StatusCode}");
            }
            var result = await response.Content.ReadFromJsonAsync<Result<DashboardViewModel>>();
            if (result == null || !result.IsSuccess)
            {
                _logger.LogError($"{nameof(GetProductSummaryDashboard)}: Lỗi khi đọc dữ liệu. Trạng thái {response.StatusCode}. Phản hồi: {result?.Noti ?? "kết quả null"}");
                return Result<DashboardViewModel>.Failure("Lỗi khi đọc dữ liệu thống kê nội dung ngành hàng.");
            }
            return Result<DashboardViewModel>.Success(result.Data ?? new DashboardViewModel(), result.Noti ?? string.Empty);

        }
    }
}
