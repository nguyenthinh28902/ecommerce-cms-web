using Ecom.Cms.Web.Shared.Models.Settings;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Ecom.Cms.Web.Common.Auth
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddAuthenticationExtensions(this IServiceCollection services, IConfiguration configuration)
        {
            var systemConfig = configuration.GetSection("SystemConfig").Get<SystemConfig>();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/dang-nhap-he-thong"; // Đường dẫn đến trang login của bạn

                // Cấu hình cookie
                options.Cookie.Name = "CMS_Auth_Cookie";
                options.ExpireTimeSpan = TimeSpan.FromHours(8); // Thời gian sống của login
            }).AddOpenIdConnect("oidc", options =>
            {
                options.Authority = systemConfig.GatewayUrl;
                options.ClientId = systemConfig.ClientId;
                options.ResponseType = "code";
                // QUAN TRỌNG: Thêm các Scope để lấy Refresh Token
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("offline_access"); // Đây là chìa khóa để có RefreshToken

                options.SaveTokens = true; // Phải có để lấy được token sau này
                options.GetClaimsFromUserInfoEndpoint = true;
            });
            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.SameSite = SameSiteMode.Lax; // Cho phép gửi cookie khi redirect từ site khác
            });
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(10);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });


            return services;
        }
    }
}
