using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using SubscriptionManager.Application.Authentication;
using SubscriptionManager.Infrastructure.Authentication.Email;

namespace SubscriptionManager.Infrastructure.Tests.Email;

public sealed class EmailDependencyInjectionTests
{
    [Fact]
    public void AddInfrastructure_ShouldRegisterDevelopmentEmailSender_WhenEnvironmentIsDevelopment()
    {
        var services = new ServiceCollection();

        var configuration = CreateConfiguration();

        var environment =
            new TestHostEnvironment
            {
                EnvironmentName =
                    Environments.Development
            };

        services.AddInfrastructure(
            configuration,
            environment);

        var descriptor =
            Assert.Single(
                services,
                descriptor =>
                    descriptor.ServiceType == typeof(IEmailSender));

        Assert.Equal(
            typeof(DevelopmentEmailSender),
            descriptor.ImplementationType);
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterAzureEmailSender_WhenEnvironmentIsProduction()
    {
        var services = new ServiceCollection();

        var configuration = CreateConfiguration();

        var environment =
            new TestHostEnvironment
            {
                EnvironmentName =
                    Environments.Production
            };

        services.AddInfrastructure(
            configuration,
            environment);

        var descriptor =
            Assert.Single(
                services,
                descriptor =>
                    descriptor.ServiceType == typeof(IEmailSender));

        Assert.Equal(
            typeof(AzureEmailSender),
            descriptor.ImplementationType);
    }

    private static IConfiguration CreateConfiguration()
    {
        var settings =
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:SubscriptionManager"] =
                    "Server=(localdb)\\MSSQLLocalDB;Database=SubscriptionManagerTests;Trusted_Connection=True;",

                ["Email:ApplicationBaseUrl"] =
                    "https://localhost:7000",

                ["AzureEmail:Endpoint"] =
                    "https://example.communication.azure.com/",

                ["AzureEmail:SenderAddress"] =
                    "DoNotReply@example.com",

                ["SavingsPlanAi:Endpoint"] =
                    "https://api.openai.com/v1",

                ["SavingsPlanAi:Model"] =
                    "gpt-5-mini",

                ["SavingsPlanAi:MaximumIterations"] =
                    "5",

                ["SavingsPlanAi:MaximumOutputTokens"] =
                    "800",

                ["SavingsPlanAi:RequestTimeoutSeconds"] =
                    "30"
            };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }

    private sealed class TestHostEnvironment
        : IHostEnvironment
    {
        public string EnvironmentName { get; set; } =
            Environments.Development;

        public string ApplicationName { get; set; } =
            nameof(EmailDependencyInjectionTests);

        public string ContentRootPath { get; set; } =
            Directory.GetCurrentDirectory();

        public IFileProvider ContentRootFileProvider { get; set; } =
            null!;
    }
}
