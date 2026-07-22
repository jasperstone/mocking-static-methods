using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Clustering.Redis.Hosting;
using Orleans.Hosting;
using StackExchange.Redis;
using Moq;
using Xunit;
using Microsoft.Extensions.Options;

namespace Orleans.Clustering.Redis.Tests
{
    public class RedisClusteringProviderBuilderTests
    {
        [Fact]
        public void Configure_GetConnectionStringCalled_WhenConnectionNameIsSet()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("ConnectionStrings:TestConnection", "test-connection-string")
                })
                .Build();

            var serviceProvider = new ServiceCollection()
                .AddSingleton<IConfiguration>(configuration)
                .BuildServiceProvider();

            var configurationSection = configuration.GetSection("TestSection");
            configurationSection["ConnectionName"] = "TestConnection";

            var builder = new TestSiloBuilder();
            var providerBuilder = new Orleans.Clustering.Redis.Hosting.RedisClusteringProviderBuilder();

            // Act
            providerBuilder.Configure(builder, "TestProvider", configurationSection);

            // Assert
            var services = builder.Services.BuildServiceProvider();
            Assert.NotNull(services.GetService<IOptions<Orleans.Clustering.Redis.Hosting.RedisClusteringOptions>>());
            var options = services.GetService<IOptions<Orleans.Clustering.Redis.Hosting.RedisClusteringOptions>>().Value;
            Assert.NotNull(options.CreateMultiplexer);
            Assert.NotNull(options.ConfigurationOptions);
        }

        private class TestSiloBuilder : ISiloBuilder
        {
            public IServiceCollection Services { get; } = new ServiceCollection();
            public IConfiguration Configuration { get; } = new ConfigurationBuilder().Build();
            public void UseRedisClustering(Action<Orleans.Clustering.Redis.Hosting.RedisClusteringOptions> configure) { }
        }
    }
}
