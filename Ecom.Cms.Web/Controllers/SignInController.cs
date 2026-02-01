using Ecom.Cms.Web.lib;
using Ecom.Cms.Web.Shared.Interfaces.Auth;
using Ecom.Cms.Web.Shared.Models.Auth.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Ecom.Cms.Web.Controllers
{
    public class SignInController : Controller
    {
        private readonly ILogger<SignInController> _logger;
        private readonly IAuthAppService _authAppService;
        public SignInController(ILogger<SignInController> logger,
            IAuthAppService authAppService)
        {
            _logger = logger;
            _authAppService = authAppService;
        }
        [HttpGet("dang-nhap-he-thong")]
        public IActionResult Index([FromForm] string returnUrl = "/", string error = null)
        {
            var viewModel = new SignInViewModel {
                ReturnUrl = returnUrl,
                SystemMessage = error ?? "Chào mừng bạn đến với hệ thống CMS"
            };

            return View(viewModel);
        }

        [HttpPost("chuyen-trang-dang-nhap")]
        [ValidateAntiForgeryToken] // Bảo mật chống giả mạo request
        public IActionResult RedirectToLogin(string returnUrl)
        {
            // 1. TẠO CODE VERIFIER (Chuỗi gốc bí mật)
            // Nếu không dùng thư viện IdentityModel, bạn có thể dùng Guid.NewGuid() hoặc RandomBytes
            string codeVerifier = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"); // Tạo chuỗi ngẫu nhiên
            string codeChallenge = LibSecurity.GenerateCodeChallenge(codeVerifier);
            HttpContext.Session.SetString("pkce_verifier", codeVerifier);
            // Server tính toán URL thông qua Service đã inject
            var identityUrl = _authAppService.GetIdentityServerLoginUrl(returnUrl, codeChallenge);

            // Ghi log trước khi chuyển hướng (sử dụng ILogger đã cài đặt)
            _logger.LogInformation("Redirecting to Identity Server via Gateway: {Url}", identityUrl);

            // Thực hiện chuyển hướng từ phía Server
            return Redirect(identityUrl);
        }

        [HttpPost("dang-xuat-he-thong")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignOut()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            // 1. Xóa Cookie của ứng dụng CMS hiện tại
            // Điều này làm sạch HttpContext.User tại CMS
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // 2. Gọi lệnh đăng xuất tới Identity Server (OIDC)
            // Nó sẽ điều hướng người dùng sang Identity Server để xóa Session ở đó
            // Sau khi xong, Identity Server sẽ quay lại RedirectUri bạn đã cấu hình
            return SignOut(new AuthenticationProperties {
                RedirectUri = baseUrl // Trang sẽ quay về sau khi đăng xuất hoàn tất
            }, "oidc");
        }

    }
}
