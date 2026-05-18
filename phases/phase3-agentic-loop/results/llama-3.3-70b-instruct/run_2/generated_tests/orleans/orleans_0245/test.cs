using Xunit;
using Microsoft.Extensions.Configuration;
using Orleans.Clustering.Redis.Hosting;
using Moq;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Orleans.Clustering.Redis.Tests
{
    public class RedisClusteringProviderBuilderTests
    {
        [Fact]
        public async Task Configure_WithConnectionNameAndNoConnectionString_ConfiguresOptions()
        {
            // Arrange
            var configurationSection = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("ConnectionName", "TestConnection"),
                })
                .Build()
                .GetSection("Redis");

            var rootConfiguration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("ConnectionStrings:TestConnection", "localhost"),
                })
                .Build();

            var serviceProvider = new ServiceCollection()
                .AddSingleton<IConfiguration>(rootConfiguration)
                .AddOptions<RedisClusteringOptions>()
                .BuildServiceProvider();

            var builder = new Mock<Orleans.Hosting.ISiloBuilder>();
            var providerBuilder = new Orleans.Clustering.Redis.Hosting.RedisClusteringProviderBuilder();

            // Act
            providerBuilder.Configure(builder.Object, "Test", configurationSection);

            // Assert
            var options = serviceProvider.GetService<IOptions<RedisClusteringOptions>>();
            Assert.NotNull(options);
            Assert.NotNull(options.Value.ConfigurationOptions);
            Assert.Equal("localhost", options.Value.ConfigurationOptions.EndPoints[0].ToString());
        }
    }
}
