using Ecom.Cms.Web.Shared.Models.Settings;

namespace Ecom.Cms.Web.Common.Config
{
    public static class ConfigAppSetting
    {
        public static IServiceCollection AddConfigAppSetting(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ConfigClientIdentity>(
                configuration.GetSection(nameof(ConfigClientIdentity)));
            services.Configure<ConfigServiceUrl>(
               configuration.GetSection(nameof(ConfigServiceUrl)));
            return services;
        }
    }
}
