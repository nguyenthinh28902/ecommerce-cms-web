# Ecommerce CMS - Hệ thống Quản trị Nội bộ

## 📝 Giới thiệu
Hệ thống quản trị (Dashboard) dành cho nhân viên vận hành, quản lý toàn bộ nghiệp vụ của hệ thống thương mại điện tử Microservices. Dự án tập trung vào tính bảo mật cao, khả năng xử lý dữ liệu phức tạp và tối ưu hóa trải nghiệm quản trị viên.

### 🔗 Core Security & Implementation (Liên kết kỹ thuật trọng tâm)

> **Tổng quan dự án xem tại đây:** [Xem đầy đủ kiến trúc tại đây](https://github.com/nguyenthinh28902/mini-project-ecommerce)

Để đi sâu vào các cấu hình bảo mật hệ thống, bạn có thể tham khảo trực tiếp tại các module sau:

* **Client Security:** Triển khai OIDC Middleware, quản lý Secure Cookie và luồng Challenge.
  * [Cấu hình tại Web CMS](https://github.com/nguyenthinh28902/ecommerce-cms-web)
* **Identity Provider:** Định nghĩa Resource, Scope và Custom Profile Service để mapping Claims.
  * [Cấu hình tại Identity Server](https://github.com/nguyenthinh28902/ecommerce-identity-server-cms)
* **API Gateway (YARP):** Quản lý Reverse Proxy Routing và thiết lập Auth Policy tập trung.
  * [Cấu hình tại Gateway CMS](https://github.com/nguyenthinh28902/ecommerce-api-gateway-cms)
* **Resource Server:** Cấu hình JWT Bearer và phân quyền dựa trên Policy (Policy-based Authorization).
  * [Cấu hình tại Product Service](https://github.com/nguyenthinh28902/Ecom.ProductService)

---
## 🛠 Công nghệ & Giải pháp
* **Framework:** .NET 10 / ASP.NET Core MVC.
* **Security Protocol:** OpenID Connect (OIDC) & OAuth 2.0.
* **State Management:** Memory Cache, Secure Cookies.
* **UI Stack:** Tailwind CSS & Vue.js (Hybrid Model).

---

## 🔐 Workflow: Authentication & Authorization (Luồng xác thực)

Dự án triển khai cơ chế xác thực tập trung để đảm bảo an toàn cho dữ liệu quản trị:

### 1. Phân tầng bảo mật (Middleware Configuration)
Hệ thống sử dụng cơ chế **Challenge OIDC** để ép buộc xác thực ngay khi người dùng truy cập các tài nguyên nhạy cảm.

* **Cấu hình Cookie:** Sử dụng `HttpOnly`, `SameSite.Lax` và `SecurePolicy.Always` để chống lại các cuộc tấn công XSS và CSRF.
* **OIDC Integration:** Triển khai **Authorization Code Flow với PKCE** để tăng cường bảo mật trong quá trình trao đổi mã xác thực.

### 2. Xử lý Token & Phiên làm việc
Hệ thống tự động quản lý vòng đời của Access Token để đảm bảo trải nghiệm người dùng không bị gián đoạn:

* **Gia hạn tự động (Sliding Expiration):** Cookie được tự động gia hạn khi quản trị viên đang hoạt động.
* **Cơ chế Token Refresh:** Sử dụng `DelegatingHandler` để đánh chặn (intercept) các yêu cầu API. Nếu nhận mã lỗi `401 Unauthorized`, hệ thống tự động sử dụng `refresh_token` để lấy Access Token mới và thực hiện lại yêu cầu (Retry) một cách trong suốt với người dùng.

---

## 💻 Technical Snippets (Điểm nhấn kỹ thuật)

### 1. External Identity Provider Challenge (Chuyển hướng xác thực)
Thực hiện cơ chế Challenge để yêu cầu định danh từ Identity Server khi người dùng truy cập các tài nguyên yêu cầu bảo mật.

* **File:** [SignInController.cs](https://github.com/nguyenthinh28902/ecommerce-cms-web/blob/main/Ecom.Cms.Web/Controllers/SignInController.cs#L32)
* **Giải pháp:** Sử dụng `ValidateAntiForgeryToken` và kiểm tra `IsLocalUrl` để ngăn chặn các cuộc tấn công giả mạo (CSRF) và điều hướng không an toàn (Open Redirect).

```csharp
[HttpPost("chuyen-trang-dang-nhap")]
[ValidateAntiForgeryToken]
public IActionResult RedirectToLogin(string returnUrl)
{
    var url = (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl)) ? "/" : returnUrl;
    return Challenge(new AuthenticationProperties { RedirectUri = url }, "oidc");
}
```

### 2. Security Middleware & Policy (Cấu hình chính sách bảo mật)
Thiết lập cơ chế lưu trữ phiên làm việc qua Cookie và giao thức kết nối bảo mật với Identity Provider.

* **File:** [AuthenticationExtensions.cs](https://github.com/nguyenthinh28902/ecommerce-cms-web/blob/main/Ecom.Cms.Web/Common/Auth/AuthenticationExtensions.cs#L21)
* **Giải pháp:** Triển khai **Authorization Code Flow với PKCE** và thiết lập thuộc tính Cookie nghiêm ngặt (`HttpOnly`, `SecurePolicy`) để bảo vệ Access Token.

```csharp
services.AddAuthentication(options => {
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "oidc";
})
.AddCookie(options => {
    options.Cookie.Name = "CMS_Auth_Cookie";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true; // Gia hạn khi user hoạt động
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
})
.AddOpenIdConnect("oidc", options => {
    options.Authority = config.IdentityUrl;
    options.ClientId = config.ClientId;
    options.ResponseType = "code";
    options.UsePkce = true;
    options.SaveTokens = true;
    options.CallbackPath = "/signin-oidc";
});
```

### 3. Transparent Token Refresh Mechanism (Cơ chế làm mới Token tự động)
Sử dụng `DelegatingHandler` để tự động đính kèm mã thông báo và xử lý gia hạn phiên làm việc một cách trong suốt.

* **File:** [AuthenticationHeaderHandler.cs](https://github.com/nguyenthinh28902/ecommerce-cms-web/blob/main/Ecom.Cms.Web/Common/HeaderHandler/AuthenticationHeaderHandler.cs)
* **Giải pháp:** Đánh chặn lỗi `401 Unauthorized`, tự động gọi `RefreshTokenAsync` và thực hiện lại (retry) request gốc với token mới để đảm bảo trải nghiệm người dùng không bị gián đoạn.

```csharp
protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
{
    var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");
    if (!string.IsNullOrEmpty(accessToken))
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

    var response = await base.SendAsync(request, ct);

    if (response.StatusCode == HttpStatusCode.Unauthorized)
    {
        var refreshToken = await _httpContextAccessor.HttpContext.GetTokenAsync("refresh_token");
        var refreshResult = await _authService.RefreshTokenAsync(refreshToken);
        if (refreshResult.IsSuccess)
        {
            await _authTokenCookie.UpdateAuthCookie(refreshResult.Data);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshResult.Data.AccessToken);
            return await base.SendAsync(request, ct); // Retry request
        }
    }
    return response;
}
```

---
