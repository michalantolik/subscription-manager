using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SubscriptionManager.Application.Common.Authentication;
using SubscriptionManager.Application.Common.Email;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Application.DigitalServices;
using SubscriptionManager.Application.Subscriptions;
using SubscriptionManager.Infrastructure.Authentication;
using SubscriptionManager.Infrastructure.Email;
using SubscriptionManager.Infrastructure.Identity;
using SubscriptionManager.Infrastructure.Persistence;
using SubscriptionManager.Infrastructure.Persistence.Repositories;

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

        return services;
    }
}
