namespace Ecom.Cms.Web.Common.HeaderHandler
{
    public static class HttpClientExtensions
    {
        public static IHttpClientBuilder AddEcomClient<TInterface, TImplementation>(
            this IServiceCollection services,
            string baseUrl,
            bool useAuth = true)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            var builder = services.AddHttpClient<TInterface, TImplementation>(client =>
            {
                client.BaseAddress = new Uri(baseUrl);
             
                client.Timeout = TimeSpan.FromMinutes(10);
            });

           
            if (useAuth)
            {
                builder.AddHttpMessageHandler<AuthenticationHeaderHandler>();
            }

            return builder;
        }
    }
}
