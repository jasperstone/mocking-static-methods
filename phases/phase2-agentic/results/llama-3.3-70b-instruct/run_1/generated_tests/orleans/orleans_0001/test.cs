using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Hosting;
using Xunit;

public class AdoNetClusteringProviderBuilderTests
{
    [Fact]
    public void Configure_SiloBuilder_WithConnectionStringFromConfigurationSection()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("AdoNet:ConnectionString", "connection-string"),
                new KeyValuePair<string, string>("AdoNet:ConnectionName", "connection-name"),
            })
            .Build());
        var serviceProvider = services.BuildServiceProvider();
        var configuration = serviceProvider.GetService<IConfiguration>();
        var configurationSection = configuration.GetSection("AdoNet");
        var builder = new SiloBuilder();

        // Act
        var providerBuilder = new AdoNetClusteringProviderBuilder();
        providerBuilder.Configure(builder, "AdoNet", configurationSection);

        // Assert
        var options = serviceProvider.GetService<IOptions<AdoNetClusteringSiloOptions>>();
        Assert.NotNull(options);
        Assert.Equal("connection-string", options.Value.ConnectionString);
    }

    [Fact]
    public void Configure_SiloBuilder_WithConnectionStringFromGetConnectionString()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("ConnectionStrings:connection-name", "connection-string"),
                new KeyValuePair<string, string>("AdoNet:ConnectionName", "connection-name"),
            })
            .Build());
        var serviceProvider = services.BuildServiceProvider();
        var configuration = serviceProvider.GetService<IConfiguration>();
        var configurationSection = configuration.GetSection("AdoNet");
        var builder = new SiloBuilder();

        // Act
        var providerBuilder = new AdoNetClusteringProviderBuilder();
        providerBuilder.Configure(builder, "AdoNet", configurationSection);

        // Assert
        var options = serviceProvider.GetService<IOptions<AdoNetClusteringSiloOptions>>();
        Assert.NotNull(options);
        Assert.Equal("connection-string", options.Value.ConnectionString);
    }

    [Fact]
    public void Configure_ClientBuilder_WithConnectionStringFromConfigurationSection()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("AdoNet:ConnectionString", "connection-string"),
                new KeyValuePair<string, string>("AdoNet:ConnectionName", "connection-name"),
            })
            .Build());
        var serviceProvider = services.BuildServiceProvider();
        var configuration = serviceProvider.GetService<IConfiguration>();
        var configurationSection = configuration.GetSection("AdoNet");
        var builder = new ClientBuilder();

        // Act
        var providerBuilder = new AdoNetClusteringProviderBuilder();
        providerBuilder.Configure(builder, "AdoNet", configurationSection);

        // Assert
        var options = serviceProvider.GetService<IOptions<AdoNetClusteringClientOptions>>();
        Assert.NotNull(options);
        Assert.Equal("connection-string", options.Value.ConnectionString);
    }

    [Fact]
    public void Configure_ClientBuilder_WithConnectionStringFromGetConnectionString()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("ConnectionStrings:connection-name", "connection-string"),
                new KeyValuePair<string, string>("AdoNet:ConnectionName", "connection-name"),
            })
            .Build());
        var serviceProvider = services.BuildServiceProvider();
        var configuration = serviceProvider.GetService<IConfiguration>();
        var configurationSection = configuration.GetSection("AdoNet");
        var builder = new ClientBuilder();

        // Act
        var providerBuilder = new AdoNetClusteringProviderBuilder();
        providerBuilder.Configure(builder, "AdoNet", configurationSection);

        // Assert
        var options = serviceProvider.GetService<IOptions<AdoNetClusteringClientOptions>>();
        Assert.NotNull(options);
        Assert.Equal("connection-string", options.Value.ConnectionString);
    }
}
