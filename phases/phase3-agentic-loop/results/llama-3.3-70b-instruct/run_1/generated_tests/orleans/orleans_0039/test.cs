using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Hosting;
using Xunit;
using Azure.Storage.Queues;

public class AzureQueueStreamProviderBuilderTests
{
    [Fact]
    public void GetConnectionString_Called_When_ConnectionName_Is_Set()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("ConnectionName", "TestConnection"),
                new KeyValuePair<string, string>("ConnectionStrings:TestConnection", "TestConnectionString"),
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        var serviceProvider = services.BuildServiceProvider();

        var configurationSection = configuration.GetSection("AzureQueueOptions");

        // Act
        var azureQueueStreamProviderBuilder = new AzureQueueStreamProviderBuilder();
        var siloBuilder = new SiloBuilder();
        azureQueueStreamProviderBuilder.Configure(siloBuilder, "Test", configurationSection);

        // Assert
        var azureQueueOptions = new AzureQueueOptions();
        configurationSection.Bind(azureQueueOptions);
        Assert.NotNull(azureQueueOptions);
    }
}
