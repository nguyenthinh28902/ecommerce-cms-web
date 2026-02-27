using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.Cms.Application.Product
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationProductDependencyInjection(this IServiceCollection services, IConfiguration configuration)
        {


            return services;
        }

    }
}
