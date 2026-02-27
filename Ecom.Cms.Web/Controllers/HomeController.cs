using Ecom.Cms.Application.Product.Interfaces;
using Ecom.Cms.Web.Models;
using Ecom.Cms.Web.Services;
using Ecom.Cms.Web.Shared.Models.AuthWeb;
using Ecom.Cms.Web.Shared.Models.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Diagnostics;
using System.Security.Claims;

namespace Ecom.Cms.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductSummaryService _productSummaryService;
        public HomeController(ILogger<HomeController> logger, IProductSummaryService productSummaryService)
        {
            _logger = logger;
            _productSummaryService = productSummaryService;
        }
        public async Task<IActionResult> Index()
        {
            var dashboardViewModels = new List<DashboardViewModel>();
            try
            {
                var deptCodes = User.GetRoles();
                if (deptCodes.Contains(DepartmentCode.Content.ToString()))
                {
                    var resultContent = await _productSummaryService.GetProductSummaryDashboard();
                    if (resultContent.IsSuccess && resultContent.Data != null)
                    {
                        dashboardViewModels.Add(resultContent.Data);
                    }
                    else
                    {
                        // Truyền thông báo lỗi từ API nếu có
                        ViewBag.ErrorMessage = resultContent.Noti ?? "Không thể lấy dữ liệu từ hệ thống.";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi mvc CMS {nameof(HomeController)}/{nameof(Index)}: {ex.Message}");
                // Gán thông báo lỗi để hiển thị lên View
                ViewBag.ErrorMessage = "Đã xảy ra lỗi trong quá trình xử lý dữ liệu. Vui lòng thử lại sau.";
            }

            return View(dashboardViewModels);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
