using Microsoft.Extensions.Configuration;
using Orleans.Clustering.Redis.Hosting;
using Orleans.Hosting;
using Xunit;
using Moq;
using Microsoft.Extensions.Options;

namespace Orleans.Clustering.Redis.Tests
{
    public class RedisClusteringProviderBuilderTests
    {
        [Fact]
        public void Configure_SiloBuilder_ConfiguresRedisClusteringOptions()
        {
            // Arrange
            var builder = new HostBuilder();
            var configurationSection = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("ServiceKey", "serviceKey"),
                    new KeyValuePair<string, string>("ConnectionName", "connectionName"),
                    new KeyValuePair<string, string>("ConnectionString", "connectionString")
                })
                .Build()
                .GetSection("RedisClustering");

            var providerBuilder = new RedisClusteringProviderBuilder();

            // Act
            providerBuilder.Configure((ISiloBuilder)builder, "name", configurationSection);

            // Assert
            var services = builder.Services;
            var redisClusteringOptions = services.GetService<IOptions<RedisClusteringOptions>>();
            Assert.NotNull(redisClusteringOptions);
        }

        [Fact]
        public void Configure_ClientBuilder_ConfiguresRedisClusteringOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("ServiceKey", "serviceKey"),
                    new KeyValuePair<string, string>("ConnectionName", "connectionName"),
                    new KeyValuePair<string, string>("ConnectionString", "connectionString")
                })
                .Build();
            var builder = new ClientBuilder(services, configuration);
            var configurationSection = configuration.GetSection("RedisClustering");

            var providerBuilder = new RedisClusteringProviderBuilder();

            // Act
            providerBuilder.Configure(builder, "name", configurationSection);

            // Assert
            var redisClusteringOptions = services.GetService<IOptions<RedisClusteringOptions>>();
            Assert.NotNull(redisClusteringOptions);
        }

        [Fact]
        public void Configure_GetConnectionString_CalledWhenConnectionNameIsSpecified()
        {
            // Arrange
            var builder = new HostBuilder();
            var configurationSection = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("ConnectionName", "connectionName")
                })
                .Build()
                .GetSection("RedisClustering");

            var providerBuilder = new RedisClusteringProviderBuilder();
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetConnectionString("connectionName")).Returns("connectionString");

            // Act
            providerBuilder.Configure((ISiloBuilder)builder, "name", configurationSection);

            // Assert
            configurationMock.Verify(c => c.GetConnectionString("connectionName"), Times.Once);
        }
    }
}
