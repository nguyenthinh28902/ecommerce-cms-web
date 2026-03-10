# Giao diện quản lý 
## Giới thiệu
- Giao diện quản hệ thống thương mại điện tử
### Thông tin chung của dự án
[Thông tin chung dự án](https://github.com/nguyenthinh28902/mini-project-ecommerce).
## 🛠 Công nghệ
- **Framework:** .NET Core / MVC
- **Giao thức:** OpenID Connect (OIDC) & OAuth
- **Khác:** Memory cache

## 🔄 Workflow (Luồng xác thực)
### Cấu hình xác thực tại Web
- Chuyển trang sang identity server nếu có yêu cầu đăng nhập. [SignInController.cs](https://github.com/nguyenthinh28902/ecommerce-cms-web/blob/main/Ecom.Cms.Web/Controllers/SignInController.cs)
```csharp
[HttpPost("chuyen-trang-dang-nhap")]
[ValidateAntiForgeryToken] // Bảo mật chống giả mạo request
public IActionResult RedirectToLogin(string returnUrl)
{
  if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
  {
    returnUrl = "/";
  }
  var properties = new AuthenticationProperties { RedirectUri = returnUrl};
  return Challenge(properties, "oidc");
}
```
- Cấu hình xác thực. [AuthenticationExtensions](https://github.com/nguyenthinh28902/ecommerce-cms-web/blob/main/Ecom.Cms.Web/Common/Auth/AuthenticationExtensions.cs)
```csharp
  services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme; // Ép hệ thống dùng OIDC khi cần đăng nhập
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.LoginPath = "/dang-nhap-he-thong";
                options.Cookie.Name = "CMS_Auth_Cookie";
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true; // Gia hạn cookie khi user hoạt động

                // Bảo mật Cookie
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Chỉ chạy trên HTTPS
            })
            .AddOpenIdConnect("oidc", options =>
            {
                options.Authority = configServiceUrl.IdentityUrl;
                options.ClientId = clientIdentityConfig.ClientId;
                options.ClientSecret = clientIdentityConfig.ClientSecret;
                options.ResponseType = "code";
                options.ResponseMode = "query"; // Chuẩn cho Authorization Code Flow
                options.UsePkce = true;
                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.RequireHttpsMetadata = true; // Đặt false nếu chạy môi trường local không có SSL

                options.CallbackPath = "/signin-oidc";
                options.SignedOutCallbackPath = "/signout-callback-oidc";

                // Nạp Scope
                options.Scope.Clear();
                var scopes = clientIdentityConfig.AuthScope?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (scopes != null)
                {
                    foreach (var scope in scopes)
                    {
                        options.Scope.Add(scope);
                    }
                }
                options.Events = new OpenIdConnectEvents {
                    OnTokenValidated = async context =>
                    {
                        // Lấy thông tin user login cho trang web
                        await Task.CompletedTask;
                    },
                   
                };
```
- AccessToken hết hạn. [AuthenticationHeaderHandler.cs](https://github.com/nguyenthinh28902/ecommerce-cms-web/blob/main/Ecom.Cms.Web/Common/HeaderHandler/AuthenticationHeaderHandler.cs).
```csharp
  var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");
  if (!string.IsNullOrEmpty(accessToken))
  {
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
  }
  var response = await base.SendAsync(request, cancellationToken);
  // Xử lý làm mới token nếu API trả về lỗi không được phép
  if (response.StatusCode == HttpStatusCode.Unauthorized)
  {
      var refreshToken = await _httpContextAccessor.HttpContext.GetTokenAsync("refresh_token");
      if (!string.IsNullOrEmpty(refreshToken))
        {
          var authService = _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<IAuthAppService>();
          var refreshResult = await authService.RefreshTokenAsync(refreshToken);
          if (refreshResult.IsSuccess)
            {
              await _authTokenCookie.UpdateAuthCookie(refreshResult.Data);
              // Thử lại request với token mới
              request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshResult.Data.AccessToken);
              return await base.SendAsync(request, cancellationToken);
            }
      }
    }
return response;
```

### Xác thực tại Identity
[Xem tiếp](https://github.com/nguyenthinh28902/ecommerce-identity-server-cms).

### Xác thực tại Getaway 
[Xem tiếp](https://github.com/nguyenthinh28902/ecommerce-api-gateway-cms).

### Xác thực tại Service (Product servcie)
[Xem tiếp](https://github.com/nguyenthinh28902/Ecom.ProductService).

