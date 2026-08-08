using System.ClientModel;
using System.Text;
using Azure.Communication.Email;
using Azure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenAI;
using SubscriptionManager.Application.Common.Authentication;
using SubscriptionManager.Application.Common.Email;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Application.DigitalServices;
using SubscriptionManager.Application.ExchangeRates;
using SubscriptionManager.Application.SavingsPlans;
using SubscriptionManager.Application.Subscriptions;
using SubscriptionManager.Infrastructure.Authentication;
using SubscriptionManager.Infrastructure.Email;
using SubscriptionManager.Infrastructure.ExchangeRates;
using SubscriptionManager.Infrastructure.Identity;
using SubscriptionManager.Infrastructure.Persistence;
using SubscriptionManager.Infrastructure.Persistence.Repositories;
using SubscriptionManager.Infrastructure.SavingsPlans;

namespace SubscriptionManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var connectionString =
            configuration.GetConnectionString(
                "SubscriptionManager");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'SubscriptionManager' is missing.");
        }

        services.AddDbContext<SubscriptionManagerDbContext>(
            options =>
                options.UseSqlServer(
                    connectionString));

        services
            .AddHealthChecks()
            .AddDbContextCheck<SubscriptionManagerDbContext>();

        services
            .AddIdentityCore<ApplicationUser>(
                options =>
                {
                    options.SignIn.RequireConfirmedEmail = true;

                    options.User.RequireUniqueEmail = true;

                    options.Password.RequiredLength = 8;

                    options.Lockout.AllowedForNewUsers = true;
                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Lockout.DefaultLockoutTimeSpan =
                        TimeSpan.FromMinutes(5);
                })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<
                SubscriptionManagerDbContext>()
            .AddDefaultTokenProviders();

        services
            .AddOptions<JwtOptions>()
            .Bind(
                configuration.GetSection(
                    JwtOptions.SectionName))
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(
                        options.Issuer),
                "Jwt:Issuer is required.")
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(
                        options.Audience),
                "Jwt:Audience is required.")
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(
                        options.SigningKey) &&
                    Encoding.UTF8.GetByteCount(
                        options.SigningKey) >= 32,
                "Jwt:SigningKey must contain at least 32 bytes.")
            .Validate(
                options =>
                    options.ExpirationInMinutes > 0,
                "Jwt:ExpirationInMinutes must be greater than zero.")
            .ValidateOnStart();

        services
            .AddOptions<EmailOptions>()
            .Bind(
                configuration.GetSection(
                    EmailOptions.SectionName))
            .Validate(
                options =>
                    Uri.TryCreate(
                        options.ApplicationBaseUrl,
                        UriKind.Absolute,
                        out var applicationUri) &&
                    applicationUri.Scheme is "http" or "https",
                "Email:ApplicationBaseUrl must be an absolute HTTP or HTTPS URL.")
            .ValidateOnStart();

        var azureEmailOptions =
            services
                .AddOptions<AzureEmailOptions>()
                .Bind(
                    configuration.GetSection(
                        AzureEmailOptions.SectionName));

        if (!environment.IsDevelopment())
        {
            azureEmailOptions
                .Validate(
                    options =>
                        Uri.TryCreate(
                            options.Endpoint,
                            UriKind.Absolute,
                            out var endpoint) &&
                        endpoint.Scheme == Uri.UriSchemeHttps,
                    "AzureEmail:Endpoint must be an absolute HTTPS URL.")
                .Validate(
                    options =>
                        !string.IsNullOrWhiteSpace(
                            options.SenderAddress),
                    "AzureEmail:SenderAddress is required.")
                .ValidateOnStart();
        }

        services
            .AddOptions<SavingsPlanAiOptions>()
            .Bind(
                configuration.GetSection(
                    SavingsPlanAiOptions.SectionName))
            .Validate(
                options =>
                    Uri.TryCreate(
                        options.Endpoint,
                        UriKind.Absolute,
                        out var endpoint) &&
                    endpoint.Scheme == Uri.UriSchemeHttps,
                "SavingsPlanAi:Endpoint must be an absolute HTTPS URL.")
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(
                        options.Model),
                "SavingsPlanAi:Model is required.")
            .Validate(
                options =>
                    options.MaximumIterations
                        is >= 1 and <= 10,
                "SavingsPlanAi:MaximumIterations must be between 1 and 10.")
            .Validate(
                options =>
                    options.MaximumOutputTokens
                        is >= 100 and <= 4_000,
                "SavingsPlanAi:MaximumOutputTokens must be between 100 and 4000.")
            .Validate(
                options =>
                    options.RequestTimeoutSeconds
                        is >= 5 and <= 120,
                "SavingsPlanAi:RequestTimeoutSeconds must be between 5 and 120.")
            .ValidateOnStart();

        var maximumIterations =
            configuration.GetValue<int?>(
                $"{SavingsPlanAiOptions.SectionName}:MaximumIterations")
            ?? 5;

        services
            .AddChatClient(
                serviceProvider =>
                {
                    var options =
                        serviceProvider
                            .GetRequiredService<
                                IOptions<SavingsPlanAiOptions>>()
                            .Value;

                    if (string.IsNullOrWhiteSpace(
                            options.ApiKey))
                    {
                        throw new SavingsPlanUnavailableException(
                            "The savings plan AI provider is not configured.");
                    }

                    var openAiOptions =
                        new OpenAIClientOptions
                        {
                            Endpoint =
                                new Uri(
                                    options.Endpoint)
                        };

                    return new OpenAI.Chat.ChatClient(
                            options.Model,
                            new ApiKeyCredential(
                                options.ApiKey),
                            openAiOptions)
                        .AsIChatClient();
                })
            .UseFunctionInvocation(
                configure: options =>
                    options.MaximumIterationsPerRequest =
                        maximumIterations);

        services.AddScoped<
            ISavingsPlanAgent,
            OpenAiSavingsPlanAgent>();

        services.AddScoped<
            IIdentityService,
            IdentityService>();

        services.AddSingleton<
            AccountEmailLinkBuilder>();

        if (environment.IsDevelopment())
        {
            services.AddScoped<
                IEmailSender,
                DevelopmentEmailSender>();
        }
        else
        {
            services.AddSingleton(
                serviceProvider =>
                {
                    var options =
                        serviceProvider
                            .GetRequiredService<
                                IOptions<AzureEmailOptions>>()
                            .Value;

                    var credential =
                        new DefaultAzureCredential();

                    return new EmailClient(
                        new Uri(options.Endpoint),
                        credential);
                });

            services.AddScoped<
                IEmailSender,
                AzureEmailSender>();
        }

        services.AddScoped<
            IAccessTokenGenerator,
            JwtAccessTokenGenerator>();

        services.AddScoped<
            IDigitalServiceRepository,
            DigitalServiceRepository>();

        services.AddScoped<
            ISubscriptionRepository,
            SubscriptionRepository>();

        services.AddScoped<
            ISavingsPlanUsageRepository,
            SavingsPlanUsageRepository>();

        services.AddScoped<
            IExchangeRateRepository,
            ExchangeRateRepository>();

        services.AddHttpClient<
            IExchangeRateProvider,
            NbpExchangeRateProvider>(
                httpClient =>
                {
                    httpClient.BaseAddress =
                        new Uri(
                            "https://api.nbp.pl/");

                    httpClient.Timeout =
                        TimeSpan.FromSeconds(10);
                });

        return services;
    }
}
