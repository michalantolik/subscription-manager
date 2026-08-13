using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using SubscriptionManager.Api.Authentication;
using SubscriptionManager.Api.ExceptionHandling;
using SubscriptionManager.Application;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Infrastructure;
using SubscriptionManager.Infrastructure.Authentication.Jwt;
using SubscriptionManager.Infrastructure.Persistence;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

namespace SubscriptionManager.Api;

public partial class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var applicationInsightsConnectionString =
            builder.Configuration[
                "ApplicationInsights:ConnectionString"];

        if (!string.IsNullOrWhiteSpace(
                applicationInsightsConnectionString))
        {
            builder.Services.AddApplicationInsightsTelemetry();
        }

        builder.Services.AddApplication();

        builder.Services.AddInfrastructure(
            builder.Configuration,
            builder.Environment);

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddScoped<
            ICurrentUser,
            CurrentUser>();

        var jwtOptions = builder.Configuration
            .GetSection(JwtOptions.SectionName)
            .Get<JwtOptions>()
            ?? throw new InvalidOperationException(
                "JWT configuration is missing.");

        builder.Services
            .AddAuthentication(
                JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;

                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtOptions.Issuer,

                        ValidateAudience = true,
                        ValidAudience = jwtOptions.Audience,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(
                                    jwtOptions.SigningKey)),

                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var userIdValue = context.Principal?
                            .FindFirst(
                                JwtRegisteredClaimNames.Sub)?
                            .Value;

                        if (!Guid.TryParse(
                                userIdValue,
                                out var userId)
                            || userId == Guid.Empty)
                        {
                            context.Fail(
                                "The access token does not contain a valid user identifier.");
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        builder.Services.AddAuthorization();

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
            options.OnRejected = async (
                context,
                cancellationToken) =>
            {
                await Results.Problem(
                    statusCode:
                        StatusCodes.Status429TooManyRequests,
                    title:
                        "Too many requests.",
                    detail:
                        "Rate limit exceeded. Please try again later.")
                    .ExecuteAsync(
                        context.HttpContext);
            };

            options.GlobalLimiter =
                PartitionedRateLimiter.Create<HttpContext, string>(
                    _ =>
                        RateLimitPartition.GetFixedWindowLimiter(
                            partitionKey: "api",
                            factory: _ =>
                                new FixedWindowRateLimiterOptions
                                {
                                    PermitLimit = 300,
                                    Window =
                                        TimeSpan.FromMinutes(1),
                                    QueueLimit = 0,
                                    AutoReplenishment = true
                                }));

            options.AddFixedWindowLimiter(
                "login",
                limiterOptions =>
                {
                    limiterOptions.PermitLimit = 10;

                    limiterOptions.Window =
                        TimeSpan.FromMinutes(1);

                    limiterOptions.QueueLimit = 0;

                    limiterOptions.AutoReplenishment = true;
                });

            options.AddFixedWindowLimiter(
                "register",
                limiterOptions =>
                {
                    limiterOptions.PermitLimit = 5;

                    limiterOptions.Window =
                        TimeSpan.FromMinutes(5);

                    limiterOptions.QueueLimit = 0;

                    limiterOptions.AutoReplenishment = true;
                });

            options.AddFixedWindowLimiter(
                "forgot-password",
                limiterOptions =>
                {
                    limiterOptions.PermitLimit = 5;

                    limiterOptions.Window =
                        TimeSpan.FromMinutes(5);

                    limiterOptions.QueueLimit = 0;

                    limiterOptions.AutoReplenishment = true;
                });

            options.AddFixedWindowLimiter(
                "reset-password",
                limiterOptions =>
                {
                    limiterOptions.PermitLimit = 5;

                    limiterOptions.Window =
                        TimeSpan.FromMinutes(5);

                    limiterOptions.QueueLimit = 0;

                    limiterOptions.AutoReplenishment = true;
                });
        });

        builder.Services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer(
                (
                    document,
                    context,
                    cancellationToken) =>
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

        if (app.Environment.IsDevelopment())
        {
            await app.Services.InitializeDatabaseAsync();
        }

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

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.MapHealthChecks("/health");

        await app.RunAsync();
    }
}
