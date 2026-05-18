using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Hosting;
using Xunit;
using Azure.Storage.Queues;

namespace Orleans.Hosting.Tests;

public class AzureQueueStreamProviderBuilderTests
{
    private static readonly MethodInfo GetQueueOptionBuilderMethod = typeof(AzureQueueStreamProviderBuilder)
        .GetMethod("GetQueueOptionBuilder", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!;

    [Fact]
    public void GetQueueOptionBuilder_ConnectionNamePresentAndConnectionStringEmpty_CallsGetConnectionString()
    {
        // Arrange
        var configSection = new Mock<IConfigurationSection>();
        configSection.Setup(c => c["ConnectionName"]).Returns("test-connection");
        configSection.Setup(c => c["ConnectionString"]).Returns((string)null);
        configSection.Setup(c => c["ServiceKey"]).Returns((string)null);

        var rootConfig = new Mock<IConfiguration>();
        rootConfig.Setup(c => c.GetConnectionString("test-connection")).Returns("fake-connection-string");

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(rootConfig.Object);
        var serviceProvider = services.BuildServiceProvider();

        var calledConfigureAction = false;
        var optionsBuilderMock = new Mock<OptionsBuilder<AzureQueueOptions>>();
        optionsBuilderMock.Setup(ob => ob.Configure<IServiceProvider>(It.IsAny<Action<AzureQueueOptions, IServiceProvider>>()))
            .Callback((Action<AzureQueueOptions, IServiceProvider> action) =>
            {
                calledConfigureAction = true;
                action(new AzureQueueOptions(), serviceProvider);
            });

        var optionBuilder = (Action<OptionsBuilder<AzureQueueOptions>>)GetQueueOptionBuilderMethod.Invoke(null, new[] { configSection.Object })!;

        // Act
        optionBuilder(optionsBuilderMock.Object);

        // Assert
        Assert.True(calledConfigureAction);
        rootConfig.Verify(c => c.GetConnectionString("test-connection"), Times.Once);
    }

    [Fact]
    public void GetQueueOptionBuilder_ServiceKeyPresent_DoesNotCallGetConnectionString()
    {
        // Arrange
        var configSection = new Mock<IConfigurationSection>();
        configSection.Setup(c => c["ServiceKey"]).Returns("service-key");

        var rootConfig = new Mock<IConfiguration>();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(rootConfig.Object);
        var serviceProvider = services.BuildServiceProvider();

        var optionsBuilderMock = new Mock<OptionsBuilder<AzureQueueOptions>>();
        optionsBuilderMock.Setup(ob => ob.Configure<IServiceProvider>(It.IsAny<Action<AzureQueueOptions, IServiceProvider>>()))
            .Callback((Action<AzureQueueOptions, IServiceProvider> action) => action(new AzureQueueOptions(), serviceProvider));

        var optionBuilder = (Action<OptionsBuilder<AzureQueueOptions>>)GetQueueOptionBuilderMethod.Invoke(null, new[] { configSection.Object })!;

        // Act
        optionBuilder(optionsBuilderMock.Object);

        // Assert
        rootConfig.Verify(c => c.GetConnectionString(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void GetQueueOptionBuilder_ConnectionStringPresent_DoesNotCallGetConnectionString()
    {
        // Arrange
        var configSection = new Mock<IConfigurationSection>();
        configSection.Setup(c => c["ConnectionName"]).Returns("test-connection");
        configSection.Setup(c => c["ConnectionString"]).Returns("direct-connection-string");
        configSection.Setup(c => c["ServiceKey"]).Returns((string)null);

        var rootConfig = new Mock<IConfiguration>();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(rootConfig.Object);
        var serviceProvider = services.BuildServiceProvider();

        var optionsBuilderMock = new Mock<OptionsBuilder<AzureQueueOptions>>();
        optionsBuilderMock.Setup(ob => ob.Configure<IServiceProvider>(It.IsAny<Action<AzureQueueOptions, IServiceProvider>>()))
            .Callback((Action<AzureQueueOptions, IServiceProvider> action) => action(new AzureQueueOptions(), serviceProvider));

        var optionBuilder = (Action<OptionsBuilder<AzureQueueOptions>>)GetQueueOptionBuilderMethod.Invoke(null, new[] { configSection.Object })!;

        // Act
        optionBuilder(optionsBuilderMock.Object);

        // Assert
        rootConfig.Verify(c => c.GetConnectionString(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void GetQueueOptionBuilder_ConnectionNameEmpty_DoesNotCallGetConnectionString()
    {
        // Arrange
        var configSection = new Mock<IConfigurationSection>();
        configSection.Setup(c => c["ConnectionName"]).Returns((string)null);
        configSection.Setup(c => c["ConnectionString"]).Returns((string)null);
        configSection.Setup(c => c["ServiceKey"]).Returns((string)null);

        var rootConfig = new Mock<IConfiguration>();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(rootConfig.Object);
        var serviceProvider = services.BuildServiceProvider();

        var optionsBuilderMock = new Mock<OptionsBuilder<AzureQueueOptions>>();
        optionsBuilderMock.Setup(ob => ob.Configure<IServiceProvider>(It.IsAny<Action<AzureQueueOptions, IServiceProvider>>()))
            .Callback((Action<AzureQueueOptions, IServiceProvider> action) => action(new AzureQueueOptions(), serviceProvider));

        var optionBuilder = (Action<OptionsBuilder<AzureQueueOptions>>)GetQueueOptionBuilderMethod.Invoke(null, new[] { configSection.Object })!;

        // Act
        optionBuilder(optionsBuilderMock.Object);

        // Assert
        rootConfig.Verify(c => c.GetConnectionString(It.IsAny<string>()), Times.Never);
    }
}

// Test-specific AzureQueueOptions matching the source code usage
public class AzureQueueOptions
{
    public List<string>? QueueNames { get; set; }
    public TimeSpan MessageVisibilityTimeout { get; set; }
    public QueueServiceClient? QueueServiceClient { get; set; }
}
