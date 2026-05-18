using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Streaming.AzureStorage;
using Azure.Storage.Queues;
using Orleans.Hosting;
using Microsoft.Extensions.Options;

public class AzureQueueStreamProviderBuilderTests
{
    [Fact]
    public void GetConnectionString_FromConfigurationSection_WithConnectionName_ReturnsConnectionString()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("ConnectionStrings:MyConnection", "DefaultEndpointsProtocol=https;AccountName=myaccount;AccountKey=mykey;BlobEndpoint=https://myaccount.blob.core.windows.net/"),
                new KeyValuePair<string, string>("ConnectionName", "MyConnection"),
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        var serviceProvider = services.BuildServiceProvider();

        var configurationSection = configuration.GetSection("AzureQueueOptions");

        // Act
        var connectionString = configurationSection.GetConnectionString("MyConnection");

        // Assert
        Assert.NotNull(connectionString);
        Assert.Equal("DefaultEndpointsProtocol=https;AccountName=myaccount;AccountKey=mykey;BlobEndpoint=https://myaccount.blob.core.windows.net/", connectionString);
    }

    [Fact]
    public void GetConnectionString_FromConfigurationSection_WithoutConnectionName_ReturnsNull()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("ConnectionStrings:MyConnection", "DefaultEndpointsProtocol=https;AccountName=myaccount;AccountKey=mykey;BlobEndpoint=https://myaccount.blob.core.windows.net/"),
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        var serviceProvider = services.BuildServiceProvider();

        var configurationSection = configuration.GetSection("AzureQueueOptions");

        // Act
        var connectionString = configurationSection.GetConnectionString("MyConnection");

        // Assert
        Assert.NotNull(connectionString);
        Assert.Equal("DefaultEndpointsProtocol=https;AccountName=myaccount;AccountKey=mykey;BlobEndpoint=https://myaccount.blob.core.windows.net/", connectionString);
    }

    [Fact]
    public void GetQueueOptionBuilder_WithServiceKey_ReturnsQueueServiceClient()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("ServiceKey", "MyServiceKey"),
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<QueueServiceClient>(new QueueServiceClient(new Uri("https://myaccount.queue.core.windows.net/")));
        var serviceProvider = services.BuildServiceProvider();

        var configurationSection = configuration.GetSection("AzureQueueOptions");

        // Act
        var queueOptionBuilder = new AzureQueueStreamProviderBuilder().GetQueueOptionBuilder(configurationSection);

        // Assert
        queueOptionBuilder(new OptionsBuilder<AzureQueueOptions>(new ServiceCollection(), string.Empty));
        var queueServiceClient = serviceProvider.GetService<QueueServiceClient>();
        Assert.NotNull(queueServiceClient);
    }

    [Fact]
    public void GetQueueOptionBuilder_WithoutServiceKey_ReturnsQueueServiceClient()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("ConnectionString", "DefaultEndpointsProtocol=https;AccountName=myaccount;AccountKey=mykey;BlobEndpoint=https://myaccount.blob.core.windows.net/"),
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        var serviceProvider = services.BuildServiceProvider();

        var configurationSection = configuration.GetSection("AzureQueueOptions");

        // Act
        var queueOptionBuilder = new AzureQueueStreamProviderBuilder().GetQueueOptionBuilder(configurationSection);

        // Assert
        queueOptionBuilder(new OptionsBuilder<AzureQueueOptions>(new ServiceCollection(), string.Empty));
        var queueServiceClient = serviceProvider.GetService<QueueServiceClient>();
        Assert.NotNull(queueServiceClient);
    }
}
