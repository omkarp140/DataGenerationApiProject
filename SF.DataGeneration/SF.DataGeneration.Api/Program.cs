//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.

//builder.Services.AddControllers();
//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//builder.Services.AddHttpClient();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

//app.Run();

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
