using Ecom.Cms.Web.Shared.Interfaces.User;
using Ecom.Cms.Web.Shared.Models.Custom;
using Ecom.Cms.Web.Shared.Models.Settings;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Security.Claims;

namespace Ecom.Cms.Web.Common.Auth
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddAuthenticationExtensions(this IServiceCollection services, IConfiguration configuration)
        {
            var clientIdentityConfig = configuration.GetSection(nameof(ConfigClientIdentity)).Get<ConfigClientIdentity>();
            var configServiceUrl = configuration.GetSection(nameof(ConfigServiceUrl)).Get<ConfigServiceUrl>();
            if (clientIdentityConfig == null || configServiceUrl == null)
            {
                throw new ArgumentNullException("Không tìm thấy cấu hình trong appsettings.");
            }
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
                        var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;
                        if (claimsIdentity == null) return;
                        var accessToken = context.TokenEndpointResponse?.AccessToken;
                        if (!string.IsNullOrEmpty(accessToken))
                        {
                            claimsIdentity.AddClaim(new Claim("access_token", accessToken));
                        }

                        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                        if (string.IsNullOrEmpty(userId)) return;

                        // 3. GỌI API BỔ SUNG THÔNG TIN (Ví dụ lấy Avatar, Rank, hoặc Role đặc thù)
                        try
                        {
                            var userInformation = context.HttpContext.RequestServices.GetRequiredService<IUserInformation>();
                            var extraInfo = await userInformation.GetUserInfoAsync(accessToken);
                            if (extraInfo != null && extraInfo.IsSuccess)
                            {
                                var userInforDto = extraInfo.Data;

                                claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userInforDto.Id.ToString()));
                                claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, userInforDto.FullName));
                                claimsIdentity.AddClaim(new Claim(ClaimCustomTypes.Avatar.ToString(), userInforDto.Avatar));
                                claimsIdentity.AddClaim(new Claim(ClaimCustomTypes.DepartmentName.ToString(), userInforDto.DepartmentName));
                                claimsIdentity.AddClaim(new Claim(ClaimCustomTypes.WorkplaceName.ToString(), userInforDto.WorkplaceName));

                                foreach (var deptCode in userInforDto.DeptCodes)
                                {
                                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, deptCode));
                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                            logger.LogError(ex, "Lỗi xảy ra khi gọi UserService để lấy thêm thông tin cho User {UserId}", userId);
                        }
                        await Task.CompletedTask;
                    },
                    OnRemoteFailure = context =>
                    {
                        var linkGenerator = context.HttpContext.RequestServices.GetRequiredService<LinkGenerator>();
                        var redirectUrl = linkGenerator.GetPathByAction(
                            context.HttpContext,
                            action: "Error",
                            controller: "SignIn",
                            values: new { message = context.Failure?.Message ?? "Unknown error" }
                        );

                        context.Response.Redirect(redirectUrl ?? "/Home/Error");
                        context.HandleResponse();
                        return Task.CompletedTask;
                    },
                    OnRedirectToIdentityProvider = context =>
                    {
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Tăng lên một chút cho thoải mái
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            return services;
        }
    }
}
