using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ecom.Cms.Application.User
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationUserDependencyInjection(this IServiceCollection services, IConfiguration configuration)
        {


            return services;
        }
    }
}
