using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Orleans.Configuration;
using Orleans.Hosting;
using Azure.Storage.Queues;

namespace Orleans.Hosting.Tests;

public class AzureQueueStreamProviderBuilderTests
{
    [Fact]
    public void GetQueueOptionBuilder_ConnectionNamePresentAndConnectionStringEmpty_CallsGetConnectionString()
    {
        // Arrange
        var configSection = new Mock<IConfigurationSection>();
        configSection.Setup(c => c["ConnectionName"]).Returns("test-connection");
        configSection.Setup(c => c["ConnectionString"]).Returns((string)null);
        configSection.Setup(c => c.GetSection("QueueNames")).Returns(Mock.Of<IConfigurationSection>());

        var rootConfig = new Mock<IConfiguration>();
        rootConfig.Setup(c => c.GetConnectionString("test-connection")).Returns("test-connection-string");

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(rootConfig.Object);
        var serviceProvider = services.BuildServiceProvider();

        var optionBuilder = new Mock<OptionsBuilder<AzureQueueOptions>>();
        optionBuilder.Setup(ob => ob.Configure<IServiceProvider>(It.IsAny<Action<AzureQueueOptions, IServiceProvider>>()))
            .Callback<Action<AzureQueueOptions, IServiceProvider>>((action) =>
            {
                action(new AzureQueueOptions(), serviceProvider);
            });

        // Use reflection to access private static method
        var getQueueOptionBuilderMethod = typeof(AzureQueueStreamProviderBuilder)
            .GetMethod("GetQueueOptionBuilder", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!;

        // Act
        var queueOptionBuilder = (Action<OptionsBuilder<AzureQueueOptions>>)getQueueOptionBuilderMethod.Invoke(null, new[] { configSection.Object })!;
        queueOptionBuilder(optionBuilder.Object);

        // Assert
        rootConfig.Verify(c => c.GetConnectionString("test-connection"), Times.Once);
    }

    [Fact]
    public void GetQueueOptionBuilder_ConnectionStringPresent_DoesNotCallGetConnectionString()
    {
        // Arrange
        var configSection = new Mock<IConfigurationSection>();
        configSection.Setup(c => c["ConnectionName"]).Returns("test-connection");
        configSection.Setup(c => c["ConnectionString"]).Returns("direct-connection-string");
        configSection.Setup(c => c.GetSection("QueueNames")).Returns(Mock.Of<IConfigurationSection>());

        var rootConfig = new Mock<IConfiguration>();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(rootConfig.Object);
        var serviceProvider = services.BuildServiceProvider();

        var optionBuilder = new Mock<OptionsBuilder<AzureQueueOptions>>();

        var getQueueOptionBuilderMethod = typeof(AzureQueueStreamProviderBuilder)
            .GetMethod("GetQueueOptionBuilder", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!;

        // Act
        var queueOptionBuilder = (Action<OptionsBuilder<AzureQueueOptions>>)getQueueOptionBuilderMethod.Invoke(null, new[] { configSection.Object })!;
        queueOptionBuilder(optionBuilder.Object);

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
        configSection.Setup(c => c.GetSection("QueueNames")).Returns(Mock.Of<IConfigurationSection>());

        var rootConfig = new Mock<IConfiguration>();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(rootConfig.Object);
        var serviceProvider = services.BuildServiceProvider();

        var optionBuilder = new Mock<OptionsBuilder<AzureQueueOptions>>();

        var getQueueOptionBuilderMethod = typeof(AzureQueueStreamProviderBuilder)
            .GetMethod("GetQueueOptionBuilder", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!;

        // Act
        var queueOptionBuilder = (Action<OptionsBuilder<AzureQueueOptions>>)getQueueOptionBuilderMethod.Invoke(null, new[] { configSection.Object })!;
        queueOptionBuilder(optionBuilder.Object);

        // Assert
        rootConfig.Verify(c => c.GetConnectionString(It.IsAny<string>()), Times.Never);
    }
}
