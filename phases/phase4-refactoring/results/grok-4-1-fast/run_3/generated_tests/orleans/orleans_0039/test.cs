using System;
using System.Collections.Generic;
using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Orleans.Configuration;
using Orleans.Hosting;

namespace Orleans.Hosting.Tests;

public class AzureQueueStreamProviderBuilderTests
{
    [Fact]
    public void GetQueueOptionBuilder_CallsGetConnectionString_WhenConnectionNamePresentAndConnectionStringEmpty()
    {
        // Arrange
        var configDict = new Dictionary<string, string?>
        {
            ["ConnectionName"] = "test-connection"
        };

        var configurationSection = CreateConfigurationSection(configDict);
        var rootConfigurationMock = new Mock<IConfiguration>();
        rootConfigurationMock.Setup(c => c.GetConnectionString("test-connection")).Returns("test-connection-string");

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(rootConfigurationMock.Object);
        var serviceProvider = services.BuildServiceProvider();

        var options = new AzureQueueOptions();

        // Capture the configure action
        Action<AzureQueueOptions, IServiceProvider>? configureAction = null;
        var optionsBuilder = new OptionsBuilder<AzureQueueOptions>();
        optionsBuilder.Configure<IServiceProvider>((o, sp) => configureAction = (o, sp));

        // Act
        var optionBuilder = ExtractGetQueueOptionBuilder(configurationSection);
        optionBuilder(optionsBuilder);

        // Trigger the captured configure action
        configureAction!(options, serviceProvider);

        // Assert
        rootConfigurationMock.Verify(c => c.GetConnectionString("test-connection"), Times.Once);
        Assert.NotNull(options.QueueServiceClient);
    }

    [Fact]
    public void GetQueueOptionBuilder_UsesDirectConnectionString_WhenConnectionStringPresent()
    {
        // Arrange
        var configDict = new Dictionary<string, string?>
        {
            ["ConnectionString"] = "direct-connection-string"
        };
        var configurationSection = CreateConfigurationSection(configDict);

        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        var options = new AzureQueueOptions();

        Action<AzureQueueOptions, IServiceProvider>? configureAction = null;
        var optionsBuilder = new OptionsBuilder<AzureQueueOptions>();
        optionsBuilder.Configure<IServiceProvider>((o, sp) => configureAction = (o, sp));

        // Act
        var optionBuilder = ExtractGetQueueOptionBuilder(configurationSection);
        optionBuilder(optionsBuilder);

        // Trigger the captured configure action
        configureAction!(options, serviceProvider);

        // Assert
        Assert.NotNull(options.QueueServiceClient);
    }

    [Fact]
    public void GetQueueOptionBuilder_UsesServiceKey_WhenServiceKeyPresent()
    {
        // Arrange
        var configDict = new Dictionary<string, string?>
        {
            ["ServiceKey"] = "test-service-key"
        };
        var configurationSection = CreateConfigurationSection(configDict);

        var queueServiceClient = new QueueServiceClient("DummyConnectionString");
        var services = new ServiceCollection();
        services.AddKeyedSingleton<QueueServiceClient>("test-service-key", queueServiceClient);
        var serviceProvider = services.BuildServiceProvider();

        var options = new AzureQueueOptions();

        Action<AzureQueueOptions, IServiceProvider>? configureAction = null;
        var optionsBuilder = new OptionsBuilder<AzureQueueOptions>();
        optionsBuilder.Configure<IServiceProvider>((o, sp) => configureAction = (o, sp));

        // Act
        var optionBuilder = ExtractGetQueueOptionBuilder(configurationSection);
        optionBuilder(optionsBuilder);

        // Trigger the captured configure action
        configureAction!(options, serviceProvider);

        // Assert
        Assert.Same(queueServiceClient, options.QueueServiceClient);
    }

    private static IConfigurationSection CreateConfigurationSection(Dictionary<string, string?> values)
    {
        var configurationBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(values);
        var configurationRoot = configurationBuilder.Build();
        return configurationRoot.GetSection("AzureQueue");
    }

    // Extracts the private static method for testing
    private static Action<OptionsBuilder<AzureQueueOptions>> ExtractGetQueueOptionBuilder(IConfigurationSection configurationSection)
    {
        return AzureQueueStreamProviderBuilder.GetQueueOptionBuilder(configurationSection);
    }
}
