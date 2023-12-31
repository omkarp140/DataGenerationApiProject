﻿using SF.DataGeneration.BLL.BackgroundProcessing;
using SF.DataGeneration.BLL.Interfaces;
using SF.DataGeneration.BLL.Services;
using SF.DataGeneration.BLL.Services.BackgroundServices.DocumentGeneration;
using SF.DataGeneration.BLL.Services.BackgroundServices.EmailDataGeneration;
using SF.DataGeneration.Models.BackgroundJob.DocumentGeneration;
using SF.DataGeneration.Models.BackgroundJob.EmailDataGeneration;
using SF.DataGeneration.Models.Settings;

namespace SF.DataGeneration.Api.Helpers
{
    public static class ServiceCollectionExtension
    {
        public static void AddCommonServices(this IServiceCollection services)
        {
            services.AddScoped<IDocumentGenerationService, DocumentGenerationService>();
            services.AddSingleton<IDocumentbotStudioApiService, DocumentbotStudioApiService>();
            services.AddScoped<IEmailDataGenerationService, EmailDataGenerationService>();
            services.AddSingleton<IEmailBotStudioApiService, EmailBotStudioApiService>();
        }

        public static void BindApiSettings(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<StudioApiBaseUrl>(options => configuration.GetSection("StudioApiBaseUrl").Bind(options));
        }

        public static void AddBackgroundServices(this IServiceCollection services)
        {
            services.AddHostedService<DocumentGenerationBackgroundService>();
            services.AddScoped<IDocumentGenerationQueueManager, DocumentGenerationQueueManager>();
            services.AddSingleton<BackgroundChannel<DocumentGenerationBackgroundJob>>();

            services.AddHostedService<EmailDataGenerationBackgroundService>();
            services.AddScoped<IEmailDataGenerationQueueManager, EmailDataGenerationQueueManager>();
            services.AddSingleton<BackgroundChannel<EmailDataGenerationBackgroundJob>>();
        }
    }
}
