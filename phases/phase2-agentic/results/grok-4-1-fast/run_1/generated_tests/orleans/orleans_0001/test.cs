using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Providers;
using Xunit;

namespace Orleans.Hosting.Tests;

public class AdoNetClusteringProviderBuilderTests
{
    private readonly Mock<ISiloBuilder> _siloBuilderMock;
    private readonly Mock<IClientBuilder> _clientBuilderMock;
    private readonly Mock<IConfigurationSection> _configSectionMock;
    private readonly Mock<IConfiguration> _configurationMock;

    public AdoNetClusteringProviderBuilderTests()
    {
        _siloBuilderMock = new Mock<ISiloBuilder>();
        _clientBuilderMock = new Mock<IClientBuilder>();
        
        _configSectionMock = new Mock<IConfigurationSection>();
        _configSectionMock.Setup(s => s[nameof(AdoNetClusteringSiloOptions.Invariant)]).Returns((string)null);
        _configSectionMock.Setup(s => s[nameof(AdoNetClusteringSiloOptions.ConnectionString)]).Returns((string)null);
        _configSectionMock.Setup(s => s["ConnectionName"]).Returns("TestConnection");

        _configurationMock = new Mock<IConfiguration>();
    }

    [Fact]
    public void Configure_SiloBuilder_CallsGetConnectionString_WhenConnectionStringEmptyAndConnectionNamePresent()
    {
        // Arrange
        var builder = new AdoNetClusteringProviderBuilder();
        var services = new ServiceCollection();
        services.AddSingleton(_configurationMock.Object);
        var serviceProvider = services.BuildServiceProvider();

        _siloBuilderMock
            .Setup(b => b.UseAdoNetClustering(It.IsAny<Action<OptionsBuilder<AdoNetClusteringSiloOptions>>>()))
            .Callback<Action<OptionsBuilder<AdoNetClusteringSiloOptions>>>(configure =>
            {
                var optionsBuilder = new OptionsBuilder<AdoNetClusteringSiloOptions>();
                configure(optionsBuilder);
                optionsBuilder.PostConfigure(o => { });
                var options = new AdoNetClusteringSiloOptions();
                optionsBuilder.Configure(serviceProvider)(options, serviceProvider);
            });

        _configurationMock.Setup(c => c.GetConnectionString("TestConnection")).Returns("test-connection-string");

        // Act
        builder.Configure(_siloBuilderMock.Object, "test", _configSectionMock.Object);

        // Assert
        _configurationMock.Verify(c => c.GetConnectionString("TestConnection"), Times.Once);
    }

    [Fact]
    public void Configure_SiloBuilder_DoesNotCallGetConnectionString_WhenConnectionStringPresent()
    {
        // Arrange
        _configSectionMock.Setup(s => s[nameof(AdoNetClusteringSiloOptions.ConnectionString)]).Returns("direct-connection-string");

        var builder = new AdoNetClusteringProviderBuilder();
        var services = new ServiceCollection();
        services.AddSingleton(_configurationMock.Object);
        var serviceProvider = services.BuildServiceProvider();

        _siloBuilderMock
            .Setup(b => b.UseAdoNetClustering(It.IsAny<Action<OptionsBuilder<AdoNetClusteringSiloOptions>>>()))
            .Callback<Action<OptionsBuilder<AdoNetClusteringSiloOptions>>>(configure =>
            {
                var optionsBuilder = new OptionsBuilder<AdoNetClusteringSiloOptions>();
                configure(optionsBuilder);
                optionsBuilder.PostConfigure(o => { });
                var options = new AdoNetClusteringSiloOptions();
                optionsBuilder.Configure(serviceProvider)(options, serviceProvider);
            });

        // Act
        builder.Configure(_siloBuilderMock.Object, "test", _configSectionMock.Object);

        // Assert
        _configurationMock.Verify(c => c.GetConnectionString(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Configure_SiloBuilder_DoesNotCallGetConnectionString_WhenConnectionNameEmpty()
    {
        // Arrange
        _configSectionMock.Setup(s => s["ConnectionName"]).Returns((string)null);

        var builder = new AdoNetClusteringProviderBuilder();
        var services = new ServiceCollection();
        services.AddSingleton(_configurationMock.Object);
        var serviceProvider = services.BuildServiceProvider();

        _siloBuilderMock
            .Setup(b => b.UseAdoNetClustering(It.IsAny<Action<OptionsBuilder<AdoNetClusteringSiloOptions>>>()))
            .Callback<Action<OptionsBuilder<AdoNetClusteringSiloOptions>>>(configure =>
            {
                var optionsBuilder = new OptionsBuilder<AdoNetClusteringSiloOptions>();
                configure(optionsBuilder);
                optionsBuilder.PostConfigure(o => { });
                var options = new AdoNetClusteringSiloOptions();
                optionsBuilder.Configure(serviceProvider)(options, serviceProvider);
            });

        // Act
        builder.Configure(_siloBuilderMock.Object, "test", _configSectionMock.Object);

        // Assert
        _configurationMock.Verify(c => c.GetConnectionString(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Configure_ClientBuilder_CallsGetConnectionString_WhenConnectionStringEmptyAndConnectionNamePresent()
    {
        // Arrange
        var builder = new AdoNetClusteringProviderBuilder();
        var services = new ServiceCollection();
        services.AddSingleton(_configurationMock.Object);
        var serviceProvider = services.BuildServiceProvider();

        _clientBuilderMock
            .Setup(b => b.UseAdoNetClustering(It.IsAny<Action<OptionsBuilder<AdoNetClusteringClientOptions>>>()))
            .Callback<Action<OptionsBuilder<AdoNetClusteringClientOptions>>>(configure =>
            {
                var optionsBuilder = new OptionsBuilder<AdoNetClusteringClientOptions>();
                configure(optionsBuilder);
                optionsBuilder.PostConfigure(o => { });
                var options = new AdoNetClusteringClientOptions();
                optionsBuilder.Configure(serviceProvider)(options, serviceProvider);
            });

        _configurationMock.Setup(c => c.GetConnectionString("TestConnection")).Returns("test-connection-string");

        // Act
        builder.Configure(_clientBuilderMock.Object, "test", _configSectionMock.Object);

        // Assert
        _configurationMock.Verify(c => c.GetConnectionString("TestConnection"), Times.Once);
    }
}
