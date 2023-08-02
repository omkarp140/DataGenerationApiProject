using SF.DataGeneration.BLL.Interfaces;
using SF.DataGeneration.BLL.Services;
using SF.DataGeneration.Models.Settings;

namespace SF.DataGeneration.Api.Helpers
{
    public static class ServiceCollectionExtension
    {
        public static void AddCommonServices(this IServiceCollection services)
        {
            services.AddScoped<IDocumentGenerationService, DocumentGenerationService>();
            services.AddSingleton<IDocumentbotStudioApiService, DocumentbotStudioApiService>();
        }

        public static void BindApiSettings(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<StudioApiBaseUrl>(options => configuration.GetSection("StudioApiBaseUrl").Bind(options));
        }
    }
}
