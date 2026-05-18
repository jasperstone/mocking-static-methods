using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using StackExchange.Redis;
using Xunit;
using Orleans.Clustering.Redis.Hosting;
using Orleans.Providers;
using Orleans.Hosting;

namespace Orleans.Clustering.Redis.Tests
{
    public class RedisClusteringProviderBuilderTests
    {
        private class TestSiloBuilder : ISiloBuilder
        {
            public IServiceCollection Services { get; } = new ServiceCollection();

            public ISiloBuilder ConfigureServices(Action<IServiceCollection> configureDelegate)
            {
                configureDelegate(Services);
                return this;
            }
        }

        [Fact]
        public void Configure_SiloBuilder_WithConnectionName_CallsGetConnectionString()
        {
            // Arrange
            var builder = new TestSiloBuilder();

            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(s => s["ServiceKey"]).Returns(string.Empty);
            configurationSectionMock.Setup(s => s["ConnectionName"]).Returns("MyConnectionName");
            configurationSectionMock.Setup(s => s["ConnectionString"]).Returns(string.Empty);

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetConnectionString("MyConnectionName")).Returns("redis-connection-string");

            builder.Services.AddSingleton(configurationMock.Object);

            var providerBuilder = (IProviderBuilder<ISiloBuilder>)Activator.CreateInstance(typeof(RedisClusteringProviderBuilder), true)!;

            // Act
            providerBuilder.Configure(builder, "name", configurationSectionMock.Object);

            // Build service provider to resolve options and trigger configuration delegate
            var serviceProvider = builder.Services.BuildServiceProvider();

            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<RedisClusteringOptions>>();
            var options = optionsMonitor.Get("name");

            // Assert
            configurationMock.Verify(c => c.GetConnectionString("MyConnectionName"), Times.Once);
            Assert.NotNull(options.ConfigurationOptions);
            Assert.Equal("redis-connection-string", options.ConfigurationOptions.ToString());
        }
    }
}
