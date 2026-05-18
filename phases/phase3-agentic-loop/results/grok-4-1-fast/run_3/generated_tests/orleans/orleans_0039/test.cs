using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Orleans.Hosting;

namespace Orleans.Hosting.Tests;

public class AzureQueueStreamProviderBuilderTests
{
    [Fact]
    public void Configure_Silo_CallsGetConnectionString_WhenConnectionNameProvidedNoConnectionString()
    {
        // Arrange
        var configDict = new Dictionary<string, string?>
        {
            ["ConnectionName"] = "test-connection"
        };
        var rootConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:test-connection"] = "retrieved-connection-string"
            })
            .Build();

        var section = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict)
            .Build()
            .GetSection("AzureQueue");

        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c.GetConnectionString("test-connection"))
                       .Returns("retrieved-connection-string");

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configurationMock.Object);

        var builderMock = new Mock<ISiloBuilder>();
        builderMock.Setup(b => b.AddAzureQueueStreams(It.IsAny<string>(), It.IsAny<Action<OptionsBuilder<object>>>()))
                  .Callback<string, Action<OptionsBuilder<object>>>((name, action) =>
                  {
                      // Trigger the configuration logic
                      var optionsBuilder = new Mock<OptionsBuilder<object>>();
                      optionsBuilder.Setup(ob => ob.Configure<object>(It.IsAny<Action<object, IServiceProvider>>()));
                      action(optionsBuilder.Object);
                  });
        builderMock.Setup(b => b.Services).Returns(services);

        var builder = builderMock.Object;

        // Act
        var streamBuilder = new AzureQueueStreamProviderBuilder();
        streamBuilder.Configure(builder, "test-provider", section);

        // Assert - Verify GetConnectionString was called
        configurationMock.Verify(c => c.GetConnectionString("test-connection"), Times.Once);
    }

    [Fact]
    public void Configure_Client_CallsGetConnectionString_WhenConnectionNameProvidedNoConnectionString()
    {
        // Arrange - same logic as silo
        var configDict = new Dictionary<string, string?>
        {
            ["ConnectionName"] = "test-connection"
        };
        var rootConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:test-connection"] = "retrieved-connection-string"
            })
            .Build();

        var section = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict)
            .Build()
            .GetSection("AzureQueue");

        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c.GetConnectionString("test-connection"))
                       .Returns("retrieved-connection-string");

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configurationMock.Object);

        var builderMock = new Mock<IClientBuilder>();
        builderMock.Setup(b => b.AddAzureQueueStreams(It.IsAny<string>(), It.IsAny<Action<OptionsBuilder<object>>>()))
                  .Callback<string, Action<OptionsBuilder<object>>>((name, action) =>
                  {
                      var optionsBuilder = new Mock<OptionsBuilder<object>>();
                      optionsBuilder.Setup(ob => ob.Configure<object>(It.IsAny<Action<object, IServiceProvider>>()));
                      action(optionsBuilder.Object);
                  });
        builderMock.Setup(b => b.Services).Returns(services);

        var builder = builderMock.Object;

        // Act
        var streamBuilder = new AzureQueueStreamProviderBuilder();
        streamBuilder.Configure(builder, "test-provider", section);

        // Assert
        configurationMock.Verify(c => c.GetConnectionString("test-connection"), Times.Once);
    }

    [Fact]
    public void Configure_DoesNotCallGetConnectionString_WhenConnectionStringDirectlyProvided()
    {
        // Arrange
        var configDict = new Dictionary<string, string?>
        {
            ["ConnectionString"] = "direct-connection-string"
        };

        var section = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict)
            .Build()
            .GetSection("AzureQueue");

        var configurationMock = new Mock<IConfiguration>();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configurationMock.Object);

        var builderMock = new Mock<ISiloBuilder>();
        builderMock.Setup(b => b.AddAzureQueueStreams(It.IsAny<string>(), It.IsAny<Action<OptionsBuilder<object>>>()))
                  .Callback<string, Action<OptionsBuilder<object>>>((name, action) =>
                  {
                      var optionsBuilder = new Mock<OptionsBuilder<object>>();
                      optionsBuilder.Setup(ob => ob.Configure<object>(It.IsAny<Action<object, IServiceProvider>>()));
                      action(optionsBuilder.Object);
                  });
        builderMock.Setup(b => b.Services).Returns(services);

        // Act
        var streamBuilder = new AzureQueueStreamProviderBuilder();
        streamBuilder.Configure(builderMock.Object, "test-provider", section);

        // Assert - GetConnectionString should NOT be called
        configurationMock.Verify(c => c.GetConnectionString(It.IsAny<string>()), Times.Never);
    }
}
