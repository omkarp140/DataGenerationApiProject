using SF.DataGeneration.Api.Helpers;

namespace SF.DataGeneration.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            ApplicationSetupHelper.SetupLogger(builder);
            ApplicationSetupHelper.AddBaseServices(builder.Services);

            // Add services to the container.
            ConfigureServices(builder.Services, builder.Configuration);

            var app = builder.Build();
            ConfigureRequestPipeline(app);
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            ServiceCollectionExtension.AddCommonServices(services);
            ServiceCollectionExtension.BindApiSettings(services, configuration);
            ServiceCollectionExtension.AddBackgroundServices(services);
        }

        private static void ConfigureRequestPipeline(WebApplication app)
        {
            app.UseSwagger();

            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
