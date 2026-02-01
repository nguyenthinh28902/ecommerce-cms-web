using Ecom.Cms.Application.Authentication.Services;
using Ecom.Cms.Application.User.Models;
using Ecom.Cms.Application.User.Services;
using Ecom.Cms.Web.Shared.Interfaces.Auth;
using Ecom.Cms.Web.Shared.Interfaces.User;
using Ecom.Cms.Web.Shared.Models.Settings;

namespace Ecom.Cms.Web.Common.HeaderHandler
{
    public static class ApplicationHeadeHandler
    {
        public static IServiceCollection AddApplicationHeadeHandler(this IServiceCollection services, IConfiguration configuration)
        {
            var systemConfig = configuration.GetSection("SystemConfig").Get<SystemConfig>();

            if (systemConfig == null || string.IsNullOrEmpty(systemConfig.GatewayUrl))
            {
                throw new Exception("Thiếu cấu hình SystemConfig hoặc GatewayUrl trong appsettings.json");
            }
            services.AddHttpClient<IAuthAppService, AuthAppService>(client =>
            {
                client.BaseAddress = new Uri($"{systemConfig.GatewayUrl}");
                client.DefaultRequestHeaders.Add("X-App-Name", systemConfig.ClientId);
                // Để hẳn 10 phút cho thoải mái Debug
                client.Timeout = TimeSpan.FromMinutes(10);
            }); // Cần thêm dòng này
            services.AddHttpClient<IUserInformation, UserInformation>(client =>
            {
                client.BaseAddress = new Uri($"{systemConfig.GatewayUrl}{ConfigApiUser.GetDefault}");
                client.DefaultRequestHeaders.Add("X-App-Name", systemConfig.ClientId);
                // Để hẳn 10 phút cho thoải mái Debug
                client.Timeout = TimeSpan.FromMinutes(10);
            }).AddHttpMessageHandler<AuthenticationHeaderHandler>(); // Cần thêm dòng này
            return services;
        }
    }
}
