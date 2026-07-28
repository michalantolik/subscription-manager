using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.RateLimiting;
using SubscriptionManager.Api.Authentication;
using SubscriptionManager.Api.ExceptionHandling;
using SubscriptionManager.Application;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Infrastructure;
using SubscriptionManager.Infrastructure.Persistence;
using System.Text.Json.Serialization;

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

        builder.Services.AddRateLimiter(options =>
        {
            options.OnRejected = async (context, cancellationToken) =>
            {
                await Results.Problem(
                    statusCode: StatusCodes.Status429TooManyRequests,
                    title: "Too many requests.",
                    detail: "Rate limit exceeded. Please try again later.")
                .ExecuteAsync(context.HttpContext);
            };

            options.AddFixedWindowLimiter("api", limiterOptions =>
            {
                limiterOptions.PermitLimit = 300;
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.QueueLimit = 0;
                limiterOptions.AutoReplenishment = true;
            });
        });

        builder.Services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer(
                (document, context, cancellationToken) =>
                {
                    document.Info.Title =
                        "Subscription Manager API";

                    document.Info.Version = "v1";

                    document.Info.Description =
                        "REST API for managing subscriptions.";

                    return Task.CompletedTask;
                });
        });

        var app = builder.Build();

        await app.Services.InitializeDatabaseAsync();

        app.UseExceptionHandler();

        app.Use(async (context, next) =>
        {
            context.Response.Headers.XContentTypeOptions =
                "nosniff";

            context.Response.Headers.XFrameOptions =
                "DENY";

            context.Response.Headers["Referrer-Policy"] =
                "no-referrer";

            context.Response.Headers["Permissions-Policy"] =
                "camera=(), microphone=(), geolocation=()";

            await next();
        });

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseRateLimiter();

        app.UseAuthorization();

        app.MapControllers()
            .RequireRateLimiting("api");

        await app.RunAsync();
    }
}
