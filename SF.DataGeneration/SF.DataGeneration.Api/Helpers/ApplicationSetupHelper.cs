using Serilog;
using Serilog.Events;
using System.Reflection;

namespace SF.DataGeneration.Api.Helpers
{
    public static class ApplicationSetupHelper
    {
        public static void SetupLogger(WebApplicationBuilder builder)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Assembly", Assembly.GetExecutingAssembly().GetName().Name)
                .WriteTo.File(path: @"logs/msg.log", fileSizeLimitBytes: 1_000_000,
                                                     flushToDiskInterval: TimeSpan.FromSeconds(5),
                                                     shared: true,
                                                     restrictedToMinimumLevel: LogEventLevel.Information)
                .CreateLogger();
            builder.Host.UseSerilog();
        }

        public static void AddBaseServices(this IServiceCollection services)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddHttpClient();
            services.AddSwaggerGen();
        }
    }
}
