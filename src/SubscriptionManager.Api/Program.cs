using System.Text.Json.Serialization;
using SubscriptionManager.Api.Authentication;
using SubscriptionManager.Api.ExceptionHandling;
using SubscriptionManager.Application;
using SubscriptionManager.Application.Common.Authentication;
using SubscriptionManager.Infrastructure;
using SubscriptionManager.Infrastructure.Persistence;

namespace SubscriptionManager.Api;

public partial class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(builder.Configuration);

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddScoped<
            ICurrentUser,
            DevelopmentCurrentUser>();

        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<ApiExceptionHandler>();

        builder.Services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(
                    new JsonStringEnumConverter());
            });

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(
                new JsonStringEnumConverter());
        });

        builder.Services.AddOpenApi();

        var app = builder.Build();

        await app.Services.InitializeDatabaseAsync();

        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();

        app.MapControllers();

        await app.RunAsync();
    }
}
