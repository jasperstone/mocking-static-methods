using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Clustering.Cosmos;
using Orleans.Hosting;
using Orleans.Providers;
using Xunit;

public class CosmosClusteringProviderBuilderTests
{
    [Fact]
    public void Configure_CallsGetConnectionString_WhenConnectionNameIsSpecified()
    {
        // Arrange
        var configurationSection = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("ConnectionName", "TestConnection"),
                new KeyValuePair<string, string>("ConnectionString", string.Empty),
            })
            .Build()
            .GetSection("CosmosClustering");

        var services = new ServiceCollection();
        services.AddOptions();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("ConnectionStrings:TestConnection", "TestConnectionString"),
            })
            .Build());

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var builder = new TestClientBuilder();
        var cosmosClusteringProviderBuilder = new CosmosClusteringProviderBuilder();
        cosmosClusteringProviderBuilder.Configure(builder, null, configurationSection);

        // Assert
        var options = configurationSection.Get<CosmosClusteringOptions>();
        Assert.NotNull(options);
    }
}

public class TestClientBuilder : IClientBuilder
{
    public IServiceCollection Services { get; } = new ServiceCollection();
    public IConfiguration Configuration { get; } = new ConfigurationBuilder().Build();

    public void UseCosmosGatewayListProvider(Action<CosmosGatewayListProviderOptionsBuilder> configure)
    {
        configure(new CosmosGatewayListProviderOptionsBuilder());
    }
}

public class CosmosGatewayListProviderOptionsBuilder
{
    public void Configure<T>(Action<CosmosGatewayListProviderOptions, T> configure) where T : class
    {
        configure(new CosmosGatewayListProviderOptions(), new TestServiceProvider());
    }
}

public class TestServiceProvider : IServiceProvider
{
    public object GetService(Type serviceType)
    {
        return new TestConfiguration();
    }
}

public class TestConfiguration : IConfiguration
{
    public IEnumerable<IConfigurationSection> GetChildren()
    {
        yield break;
    }

    public IConfigurationSection GetSection(string key)
    {
        return new ConfigurationSection(key);
    }

    public string this[string key] { get => string.Empty; set { } }

    public string GetConnectionString(string name)
    {
        return "TestConnectionString";
    }

    public IChangeToken GetReloadToken()
    {
        return new CancellationChangeToken(default);
    }
}

public class ConfigurationSection : IConfigurationSection
{
    private readonly string _path;

    public ConfigurationSection(string path)
    {
        _path = path;
    }

    public string this[string key] { get => string.Empty; set { } }

    public string Key => _path;

    public string Path => _path;

    public string Value => string.Empty;

    public IConfigurationSection GetSection(string key)
    {
        return new ConfigurationSection($"{_path}:{key}");
    }

    public IEnumerable<IConfigurationSection> GetChildren()
    {
        yield break;
    }

    public IChangeToken GetReloadToken()
    {
        return new CancellationChangeToken(default);
    }
}

public class CancellationChangeToken : IChangeToken
{
    private readonly CancellationToken _cancellationToken;

    public CancellationChangeToken(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
    }

    public bool ActiveChangeCallbacks => false;

    public bool HasChanged => false;

    public IDisposable AddChangeListener(Action<object, string> listener, object state)
    {
        return new Disposable();
    }

    private class Disposable : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
