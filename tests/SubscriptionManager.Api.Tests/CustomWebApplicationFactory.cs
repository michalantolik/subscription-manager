using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SubscriptionManager.Api.Authentication;
using SubscriptionManager.Api.Tests.Authentication;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Infrastructure.Persistence;

namespace SubscriptionManager.Api.Tests;

public sealed class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
{
    private readonly string _databaseName =
        $"SubscriptionManagerTests-{Guid.NewGuid()}";

    public HttpClient CreateAuthenticatedClient(
        Guid userId)
    {
        var client = CreateClient();

        client.DefaultRequestHeaders.Add(
            TestAuthenticationHandler.UserIdHeaderName,
            userId.ToString());

        return client;
    }

    public HttpClient CreateUnauthenticatedClient()
    {
        var factory = WithWebHostBuilder(
            builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services
                        .AddAuthentication(options =>
                        {
                            options.DefaultAuthenticateScheme =
                                UnauthenticatedTestAuthenticationHandler.AuthenticationScheme;

                            options.DefaultChallengeScheme =
                                UnauthenticatedTestAuthenticationHandler.AuthenticationScheme;
                        })
                        .AddScheme<
                            AuthenticationSchemeOptions,
                            UnauthenticatedTestAuthenticationHandler>(
                            UnauthenticatedTestAuthenticationHandler.AuthenticationScheme,
                            _ => { });
                });
            });

        return factory.CreateClient();
    }

    public HttpClient CreateJwtClient()
    {
        var factory = WithWebHostBuilder(
            builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme =
                            JwtBearerDefaults.AuthenticationScheme;

                        options.DefaultChallengeScheme =
                            JwtBearerDefaults.AuthenticationScheme;
                    });
                });
            });

        return factory.CreateClient();
    }

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<
                DbContextOptions<SubscriptionManagerDbContext>>();

            services.RemoveAll<
                IDbContextOptionsConfiguration<
                    SubscriptionManagerDbContext>>();

            services.AddDbContext<SubscriptionManagerDbContext>(
                options =>
                {
                    options.UseInMemoryDatabase(_databaseName);
                });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    TestAuthenticationHandler.AuthenticationScheme;

                options.DefaultChallengeScheme =
                    TestAuthenticationHandler.AuthenticationScheme;
            })
            .AddScheme<
                AuthenticationSchemeOptions,
                TestAuthenticationHandler>(
                TestAuthenticationHandler.AuthenticationScheme,
                _ => { });

            services.RemoveAll<ICurrentUser>();

            services.AddScoped<
                ICurrentUser,
                CurrentUser>();
        });
    }
}
