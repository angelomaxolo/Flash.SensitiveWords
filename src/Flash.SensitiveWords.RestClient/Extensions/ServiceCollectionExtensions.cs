using Flash.SensitiveWords.RestClient.Abstractions;
using Flash.SensitiveWords.RestClient.Clients;
using Microsoft.Extensions.DependencyInjection;

namespace Flash.SensitiveWords.RestClient.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRestClient(
            this IServiceCollection services,
            string baseUrl)
        {
            services.AddHttpClient<IMessageFilterClient, MessageFilterClient>(c =>
            {
                c.BaseAddress = new Uri(baseUrl);
            });

            services.AddHttpClient<ISensitiveWordsClient, SensitiveWordsClient>(c =>
            {
                c.BaseAddress = new Uri(baseUrl);
            });

            return services;
        }
    }
}
