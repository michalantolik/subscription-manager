using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
using System.ClientModel;

namespace SubscriptionManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(
            "SubscriptionManager");

        services.AddDbContext<SubscriptionManagerDbContext>(options =>
            options.UseSqlServer(connectionString));

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true;
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<SubscriptionManagerDbContext>()
            .AddDefaultTokenProviders();

        services.Configure<JwtOptions>(
            configuration.GetSection(JwtOptions.SectionName));

        services
            .AddOptions<EmailOptions>()
            .Bind(configuration.GetSection(EmailOptions.SectionName))
            .Validate(
                options => Uri.TryCreate(
                    options.ApplicationBaseUrl,
                    UriKind.Absolute,
                    out var applicationUri)
                    && applicationUri.Scheme is "http" or "https",
                "Email:ApplicationBaseUrl must be an absolute HTTP or HTTPS URL.")
            .ValidateOnStart();

        services
            .AddOptions<SavingsPlanAiOptions>()
            .Bind(
                configuration.GetSection(
                    SavingsPlanAiOptions.SectionName))
            .Validate(
                options => Uri.TryCreate(
                    options.Endpoint,
                    UriKind.Absolute,
                    out var endpoint)
                    && endpoint.Scheme == Uri.UriSchemeHttps,
                "SavingsPlanAi:Endpoint must be an absolute HTTPS URL.")
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(options.Model),
                "SavingsPlanAi:Model is required.")
            .Validate(
                options =>
                    options.MaximumIterations is >= 1 and <= 10,
                "SavingsPlanAi:MaximumIterations must be between 1 and 10.")
            .ValidateOnStart();

        var maximumIterations =
            configuration.GetValue<int?>(
                $"{SavingsPlanAiOptions.SectionName}:MaximumIterations")
            ?? 8;

        services
            .AddChatClient(serviceProvider =>
            {
                var options =
                    serviceProvider
                        .GetRequiredService<
                            IOptions<SavingsPlanAiOptions>>()
                        .Value;

                if (string.IsNullOrWhiteSpace(options.ApiKey))
                {
                    throw new InvalidOperationException(
                        "SavingsPlanAi:ApiKey is missing. Configure it using user secrets.");
                }

                var openAiOptions =
                    new OpenAIClientOptions
                    {
                        Endpoint =
                            new Uri(options.Endpoint)
                    };

                return new OpenAI.Chat.ChatClient(
                        options.Model,
                        new ApiKeyCredential(options.ApiKey),
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

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IEmailSender, DevelopmentEmailSender>();

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
            IExchangeRateRepository,
            ExchangeRateRepository>();

        services.AddHttpClient<
            IExchangeRateProvider,
            NbpExchangeRateProvider>(httpClient =>
            {
                httpClient.BaseAddress =
                    new Uri("https://api.nbp.pl/");

                httpClient.Timeout =
                    TimeSpan.FromSeconds(10);
            });

        return services;
    }
}
