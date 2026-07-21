using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SubscriptionManager.Infrastructure.Persistence;

namespace SubscriptionManager.Api.Tests;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName =
        $"SubscriptionManagerTests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<
                DbContextOptions<SubscriptionManagerDbContext>>();

            services.RemoveAll<
                IDbContextOptionsConfiguration<SubscriptionManagerDbContext>>();

            services.AddDbContext<SubscriptionManagerDbContext>(
                options =>
                {
                    options.UseInMemoryDatabase(_databaseName);
                });
        });
    }
}
