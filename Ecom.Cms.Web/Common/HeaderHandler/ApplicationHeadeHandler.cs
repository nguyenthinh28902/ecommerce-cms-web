using Ecom.Cms.Application.Authentication.Services;
using Ecom.Cms.Application.Order.Interfaces;
using Ecom.Cms.Application.Order.Services;
using Ecom.Cms.Application.Product.Interfaces;
using Ecom.Cms.Application.Product.Models;
using Ecom.Cms.Application.Product.Services;
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
            var systemConfig = configuration.GetSection(nameof(ConfigClientIdentity)).Get<ConfigClientIdentity>();
            var configServiceUrl = configuration.GetSection(nameof(ConfigServiceUrl)).Get<ConfigServiceUrl>();
            if (systemConfig == null || configServiceUrl == null)
            {
                throw new Exception("Thiếu cấu hình SystemConfig hoặc GatewayUrl trong appsettings.json");
            }

            services.AddEcomClient<IAuthAppService, AuthAppService>(configServiceUrl.IdentityUrl, useAuth: false);

            services.AddEcomClient<IUserInformation, UserInformation>($"{configServiceUrl.GatewayUrl}{ConfigApiUser.GetDefault}");

            services.AddEcomClient<IProductSummaryService, ProductSummaryService>($"{configServiceUrl.GatewayUrl}{ConfigApiProductService.GetDefault}");

            services.AddEcomClient<IOrderService, OrderService>($"{configServiceUrl.GatewayUrl}{ConfigApiOrderService.GetDefault}");
           
            return services;
        }
    }
}
