using Orleans.Clustering.Redis.Hosting;
using Orleans.Hosting;
using Microsoft.Extensions.Configuration;

public class RedisClusteringProviderBuilderWrapper : IProviderBuilder<ISiloBuilder>, IProviderBuilder<IClientBuilder>
{
    private readonly RedisClusteringProviderBuilder _inner;

    public RedisClusteringProviderBuilderWrapper()
    {
        _inner = new RedisClusteringProviderBuilder();
    }

    public void Configure(ISiloBuilder builder, string name, IConfigurationSection configurationSection)
    {
        _inner.Configure(builder, name, configurationSection);
    }

    public void Configure(IClientBuilder builder, string name, IConfigurationSection configurationSection)
    {
        _inner.Configure(builder, name, configurationSection);
    }
}
