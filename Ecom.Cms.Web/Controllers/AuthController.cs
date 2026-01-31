using Ecom.Cms.Web.Shared.Interfaces.Auth;
using Ecom.Cms.Web.Shared.Interfaces.User;
using Ecom.Cms.Web.Shared.Models.Auth.Models;
using Ecom.Cms.Web.Shared.Models.Custom;
using Ecom.Cms.Web.Shared.Models.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Ecom.Cms.Web.Controllers
{
    [Route("auth")]
    public class AuthController : Controller
    {
        ILogger<AuthController> _logger;
        private readonly IAuthAppService _authAppService;
        private readonly IUserInformation _userInformation;
        public AuthController(ILogger<AuthController> logger, IAuthAppService authAppService, IUserInformation userInformation)
        {
            _logger = logger;
            _authAppService = authAppService;
            _userInformation = userInformation;
        }

        [HttpGet("callback")] // URL này phải khớp với redirect_uri đã đăng ký
        public async Task<IActionResult> Callback(string code, string state)
        {
            // 1. Lấy "chìa khóa gốc" đã lưu trong Session lúc Redirect
            var codeVerifier = HttpContext.Session.GetString("pkce_verifier");

            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(codeVerifier))
            {
                _logger.LogWarning("Callback thất bại: Thiếu code hoặc code_verifier (Session có thể đã hết hạn)");
                return RedirectToAction("Login", "Auth", new { error = "Phiên làm việc hết hạn" });
            }

            // 2. Gọi sang Identity Server (qua Gateway 7145) để đổi lấy Token vỏ
            // Chúng ta truyền: code, codeVerifier, và đường dẫn callback để IS đối chiếu
            var result = await _authAppService.ExchangeCodeForTokenAsync(code, codeVerifier);

            if (result.IsSuccess && result.Data != null)
            {
                //get thông tin đăng nhập
                var userInfoResult = await _userInformation.GetUserInfoAsync(result.Data.AccessToken);
                if (!userInfoResult.IsSuccess) return RedirectToAction("Index", "SignIn", new { error = userInfoResult.Noti });
                // 3. Đăng nhập thành công -> Lưu AccessToken vỏ vào Cookie của CMS
                await SignInUserAsync(result.Data, userInfoResult.Data);

                // 4. Lấy lại trang đích từ tham số 'state'
                // Nếu state trống, mặc định về trang chủ
                var targetUrl = !string.IsNullOrEmpty(state) ? state : "/";

                _logger.LogInformation("Đăng nhập thành công, chuyển hướng về: {Target}", targetUrl);
                return Redirect(targetUrl);
            }

            // Xử lý khi đổi token thất bại
            return RedirectToAction("Index", "SignIn", new { error = result.Noti });
        }

        private async Task SignInUserAsync(TokenResponseDto tokenDto, UserInforDto userInforDto)
        {
            var claims = new List<Claim>
            {

                 new Claim(ClaimTypes.NameIdentifier, userInforDto.UserName),
                 new Claim(ClaimTypes.Name, userInforDto.FullName),
                 new Claim(ClaimCustomTypes.Avatar.ToString(), userInforDto.Avatar),
                 new Claim(ClaimCustomTypes.DepartmentName.ToString(), userInforDto.DepartmentName),
                 new Claim(ClaimCustomTypes.WorkplaceName.ToString(), userInforDto.WorkplaceName),

             };
            foreach (var deptCode in userInforDto.DeptCodes)
            {
                claims.Add(new Claim(ClaimTypes.Role, deptCode));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddSeconds(tokenDto.ExpiresIn)
            };
            authProperties.StoreTokens(new[]
            {
                new AuthenticationToken { Name = "access_token", Value = tokenDto.AccessToken },
                new AuthenticationToken { Name = "refresh_token", Value = tokenDto.RefreshToken }
            });
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }
    }
}
