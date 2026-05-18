using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.Clustering.Cosmos;
using System.Threading.Tasks;

public class CosmosClusteringProviderBuilderTests
{
    [Fact]
    public void Configure_Should_Call_GetConnectionString_When_ConnectionName_Provided_And_ConnectionString_Is_Empty()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string>
        {
            { "ConnectionName", "TestConnection" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        var serviceProvider = services.BuildServiceProvider();

        var builder = new MockSiloBuilder();
        var providerBuilder = new CosmosClusteringProviderBuilder();

        var configurationSection = new MockConfigurationSection(inMemorySettings);

        // Act
        providerBuilder.Configure(builder, null, configurationSection);

        // Assert
        Assert.True(builder.CalledGetRequiredService);
        Assert.Equal("TestConnection", builder.CalledServiceKey);
        Assert.True(builder.CalledConfigureCosmosClient);
    }

    // Additional tests can be added here to cover other branches
}

// Mock classes for testing
public class MockSiloBuilder : ISiloBuilder
{
    public bool CalledConfigure = false;
    public bool CalledGetRequiredService = false;
    public string CalledServiceKey = null;
    public bool CalledConfigureCosmosClient = false;
    public string ConfiguredConnectionString = null;

    public void UseCosmosClustering(Action<OptionsBuilder> optionsBuilderAction)
    {
        CalledConfigure = true;
        // Simulate options builder
        optionsBuilderAction(new OptionsBuilder(this));
    }
}

public class OptionsBuilder
{
    private readonly MockSiloBuilder _parent;
    public OptionsBuilder(MockSiloBuilder parent)
    {
        _parent = parent;
    }

    public void Configure<T>(Action<Options, IServiceProvider> configureOptions)
    {
        // No-op for test
    }

    public void ConfigureCosmosClient(string connectionString)
    {
        _parent.CalledConfigureCosmosClient = true;
        _parent.ConfiguredConnectionString = connectionString;
    }

    public void ConfigureCosmosClient(System.Func<IServiceProvider, ValueTask<CosmosClient>> factory)
    {
        _parent.CalledConfigureCosmosClient = true;
        _parent.CalledGetRequiredService = true;
        _parent.CalledServiceKey = "TestServiceKey";
    }
}

public class MockConfigurationSection : IConfigurationSection
{
    private readonly Dictionary<string, string> _settings;

    public MockConfigurationSection(Dictionary<string, string> settings)
    {
        _settings = settings;
    }

    public string this[string key]
    {
        get => _settings.ContainsKey(key) ? _settings[key] : null;
        set => _settings[key] = value;
    }

    public string Key => null;
    public string Path => null;
    public string Value { get => null; set { } }

    public IEnumerable<IConfigurationSection> GetChildren() => null;
    public IChangeToken GetReloadToken() => null;
    public IConfigurationSection GetSection(string key) => null;
}
