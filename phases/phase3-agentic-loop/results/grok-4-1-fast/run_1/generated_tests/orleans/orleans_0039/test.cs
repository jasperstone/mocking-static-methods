using System;
using System.Collections.Generic;
using Moq;
using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Azure.Storage.Queues;
using Orleans.Hosting;

namespace Orleans.Hosting.Tests;

public class AzureQueueStreamProviderBuilderTests
{
    [Fact]
    public void GetQueueOptionBuilder_ConnectionNamePresentAndConnectionStringEmpty_CallsGetConnectionString()
    {
        // Arrange
        var configurationSection = new Mock<IConfigurationSection>();
        configurationSection.Setup(c => c["ConnectionName"]).Returns("test-connection");
        configurationSection.Setup(c => c["ConnectionString"]).Returns((string)null);
        configurationSection.Setup(c => c["ServiceKey"]).Returns((string)null);
        configurationSection.Setup(c => c.GetSection("QueueNames")).Returns((IConfigurationSection)null);

        var rootConfiguration = new Mock<IConfiguration>();
        rootConfiguration.Setup(c => c.GetConnectionString("test-connection")).Returns("test-connection-string");

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(rootConfiguration.Object);
        var serviceProvider = services.BuildServiceProvider();

        var optionsBuilder = new Mock<OptionsBuilder<object>>();
        optionsBuilder.Setup(b => b.Configure<IServiceProvider>(It.IsAny<Action<object, IServiceProvider>>()))
            .Callback<Action<object, IServiceProvider>>((action) => action(null!, serviceProvider));

        // Use reflection to access private static method
        var getQueueOptionBuilderMethod = typeof(AzureQueueStreamProviderBuilder)
            .GetMethod("GetQueueOptionBuilder", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!;
        var configureAction = (Action<OptionsBuilder<object>>)getQueueOptionBuilderMethod.Invoke(null, new object[] { configurationSection.Object })!;

        // Act
        configureAction(optionsBuilder.Object);

        // Assert
        rootConfiguration.Verify(c => c.GetConnectionString("test-connection"), Times.Once);
    }

    [Fact]
    public void GetQueueOptionBuilder_ConnectionStringDirectlyPresent_DoesNotCallGetConnectionString()
    {
        // Arrange
        var configurationSection = new Mock<IConfigurationSection>();
        configurationSection.Setup(c => c["ConnectionString"]).Returns("direct-connection-string");
        configurationSection.Setup(c => c["ServiceKey"]).Returns((string)null);
        configurationSection.Setup(c => c.GetSection("QueueNames")).Returns((IConfigurationSection)null);

        var rootConfiguration = new Mock<IConfiguration>();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(rootConfiguration.Object);
        var serviceProvider = services.BuildServiceProvider();

        var optionsBuilder = new Mock<OptionsBuilder<object>>();
        optionsBuilder.Setup(b => b.Configure<IServiceProvider>(It.IsAny<Action<object, IServiceProvider>>()))
            .Callback<Action<object, IServiceProvider>>((action) => action(null!, serviceProvider));

        var getQueueOptionBuilderMethod = typeof(AzureQueueStreamProviderBuilder)
            .GetMethod("GetQueueOptionBuilder", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!;
        var configureAction = (Action<OptionsBuilder<object>>)getQueueOptionBuilderMethod.Invoke(null, new object[] { configurationSection.Object })!;

        // Act
        configureAction(optionsBuilder.Object);

        // Assert
        rootConfiguration.Verify(c => c.GetConnectionString(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void GetQueueOptionBuilder_ServiceKeyPresent_DoesNotCallGetConnectionString()
    {
        // Arrange
        var configurationSection = new Mock<IConfigurationSection>();
        configurationSection.Setup(c => c["ServiceKey"]).Returns("test-key");
        configurationSection.Setup(c => c.GetSection("QueueNames")).Returns((IConfigurationSection)null);

        var rootConfiguration = new Mock<IConfiguration>();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(rootConfiguration.Object);
        var serviceProvider = services.BuildServiceProvider();

        var optionsBuilder = new Mock<OptionsBuilder<object>>();
        optionsBuilder.Setup(b => b.Configure<IServiceProvider>(It.IsAny<Action<object, IServiceProvider>>()))
            .Callback<Action<object, IServiceProvider>>((action) => action(null!, serviceProvider));

        var getQueueOptionBuilderMethod = typeof(AzureQueueStreamProviderBuilder)
            .GetMethod("GetQueueOptionBuilder", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!;
        var configureAction = (Action<OptionsBuilder<object>>)getQueueOptionBuilderMethod.Invoke(null, new object[] { configurationSection.Object })!;

        // Act
        configureAction(optionsBuilder.Object);

        // Assert
        rootConfiguration.Verify(c => c.GetConnectionString(It.IsAny<string>()), Times.Never);
    }
}
