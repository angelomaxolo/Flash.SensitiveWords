using System;
using Flash.SensitiveWords.RestClient.Abstractions;
using Flash.SensitiveWords.RestClient.Clients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Flash.SensitiveWords.RestClient.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRestClient(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var baseUrl = configuration["ApiSettings:BaseUrl"] ?? throw new ArgumentNullException("ApiSettings:BaseUrl");
            var apiKey = configuration["ApiSettings:ApiKey"];

            services.AddHttpClient<IMessageFilterClient, MessageFilterClient>(c =>
            {
                c.BaseAddress = new Uri(baseUrl);
                if (!string.IsNullOrWhiteSpace(apiKey))
                {
                    c.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
                }
            });

            services.AddHttpClient<ISensitiveWordsClient, SensitiveWordsClient>(c =>
            {
                c.BaseAddress = new Uri(baseUrl);
                if (!string.IsNullOrWhiteSpace(apiKey))
                {
                    c.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
                }
            });

            return services;
        }
    }
}
