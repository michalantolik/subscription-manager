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
using SubscriptionManager.Infrastructure.Identity;
using SubscriptionManager.Infrastructure.Persistence;

namespace SubscriptionManager.Api.Tests;

public sealed class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
{
    private static readonly Guid DefaultUserId =
        Guid.Parse(
            "11111111-1111-1111-1111-111111111111");

    private readonly string _databaseName =
        $"SubscriptionManagerTests-{Guid.NewGuid()}";

    public new HttpClient CreateClient()
    {
        EnsureUserExists(
            DefaultUserId);

        var client =
            base.CreateClient();

        client.DefaultRequestHeaders.Add(
            TestAuthenticationHandler.UserIdHeaderName,
            DefaultUserId.ToString());

        return client;
    }

    public HttpClient CreateAuthenticatedClient(
        Guid userId)
    {
        EnsureUserExists(
            userId);

        var client =
            base.CreateClient();

        client.DefaultRequestHeaders.Add(
            TestAuthenticationHandler.UserIdHeaderName,
            userId.ToString());

        return client;
    }

    public HttpClient CreateUnauthenticatedClient()
    {
        var factory =
            WithWebHostBuilder(
                builder =>
                {
                    builder.ConfigureTestServices(
                        services =>
                        {
                            services
                                .AddAuthentication(
                                    options =>
                                    {
                                        options.DefaultAuthenticateScheme =
                                            UnauthenticatedTestAuthenticationHandler
                                                .AuthenticationScheme;

                                        options.DefaultChallengeScheme =
                                            UnauthenticatedTestAuthenticationHandler
                                                .AuthenticationScheme;
                                    })
                                .AddScheme<
                                    AuthenticationSchemeOptions,
                                    UnauthenticatedTestAuthenticationHandler>(
                                    UnauthenticatedTestAuthenticationHandler
                                        .AuthenticationScheme,
                                    _ => { });
                        });
                });

        return factory.CreateClient();
    }

    public HttpClient CreateJwtClient()
    {
        var factory =
            WithWebHostBuilder(
                builder =>
                {
                    builder.ConfigureTestServices(
                        services =>
                        {
                            services.AddAuthentication(
                                options =>
                                {
                                    options.DefaultAuthenticateScheme =
                                        JwtBearerDefaults
                                            .AuthenticationScheme;

                                    options.DefaultChallengeScheme =
                                        JwtBearerDefaults
                                            .AuthenticationScheme;
                                });
                        });
                });

        return factory.CreateClient();
    }

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.ConfigureServices(
            services =>
            {
                services.RemoveAll<
                    DbContextOptions<
                        SubscriptionManagerDbContext>>();

                services.RemoveAll<
                    IDbContextOptionsConfiguration<
                        SubscriptionManagerDbContext>>();

                services.AddDbContext<
                    SubscriptionManagerDbContext>(
                    options =>
                    {
                        options.UseInMemoryDatabase(
                            _databaseName);
                    });

                services.AddAuthentication(
                    options =>
                    {
                        options.DefaultAuthenticateScheme =
                            TestAuthenticationHandler
                                .AuthenticationScheme;

                        options.DefaultChallengeScheme =
                            TestAuthenticationHandler
                                .AuthenticationScheme;
                    })
                    .AddScheme<
                        AuthenticationSchemeOptions,
                        TestAuthenticationHandler>(
                        TestAuthenticationHandler
                            .AuthenticationScheme,
                        _ => { });

                services.RemoveAll<ICurrentUser>();

                services.AddScoped<
                    ICurrentUser,
                    CurrentUser>();
            });
    }

    private void EnsureUserExists(
        Guid userId)
    {
        using var scope =
            Services.CreateScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<
                    SubscriptionManagerDbContext>();

        var userExists =
            dbContext.Users.Any(
                user => user.Id == userId);

        if (userExists)
        {
            return;
        }

        dbContext.Users.Add(
            new ApplicationUser
            {
                Id = userId,
                UserName =
                    $"{userId}@example.com",
                Email =
                    $"{userId}@example.com"
            });

        dbContext.SaveChanges();
    }
}
