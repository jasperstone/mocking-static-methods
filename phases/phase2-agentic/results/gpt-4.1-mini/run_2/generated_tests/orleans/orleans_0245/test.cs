using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Clustering.Redis.Hosting;
using Orleans.Hosting;
using StackExchange.Redis;
using Xunit;

namespace Orleans.Clustering.Redis.Tests
{
    public class RedisClusteringProviderBuilderTests
    {
        [Fact]
        public void Configure_SiloBuilder_WithServiceKey_UsesMultiplexerFromServices()
        {
            // Arrange
            var builderMock = new Mock<ISiloBuilder>();
            var services = new ServiceCollection();
            builderMock.Setup(b => b.UseRedisClustering(It.IsAny<Action<RedisClusteringOptions>>()))
                .Returns(builderMock.Object);
            builderMock.SetupGet(b => b.Services).Returns(services);

            var multiplexerMock = new Mock<IConnectionMultiplexer>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredKeyedService<IConnectionMultiplexer>("myServiceKey"))
                .Returns(multiplexerMock.Object);

            services.AddSingleton(serviceProviderMock.Object);

            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.SetupGet(c => c["ServiceKey"]).Returns("myServiceKey");
            configurationSectionMock.SetupGet(c => c["ConnectionName"]).Returns((string)null);
            configurationSectionMock.SetupGet(c => c["ConnectionString"]).Returns((string)null);

            var builder = builderMock.Object;
            var providerBuilder = new RedisClusteringProviderBuilder();

            // Act
            providerBuilder.Configure(builder, "name", configurationSectionMock.Object);

            // Build service provider to resolve options
            var sp = services.BuildServiceProvider();
            var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<RedisClusteringOptions>>();
            var options = optionsMonitor.Get("name");

            // Assert
            Assert.NotNull(options.CreateMultiplexer);
            var multiplexerTask = options.CreateMultiplexer(null);
            Assert.Same(multiplexerMock.Object, multiplexerTask.Result);
            Assert.NotNull(options.ConfigurationOptions);
        }

        [Fact]
        public void Configure_SiloBuilder_WithConnectionName_CallsGetConnectionString()
        {
            // Arrange
            var builderMock = new Mock<ISiloBuilder>();
            var services = new ServiceCollection();
            builderMock.Setup(b => b.UseRedisClustering(It.IsAny<Action<RedisClusteringOptions>>()))
                .Returns(builderMock.Object);
            builderMock.SetupGet(b => b.Services).Returns(services);

            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.SetupGet(c => c["ServiceKey"]).Returns((string)null);
            configurationSectionMock.SetupGet(c => c["ConnectionName"]).Returns("myConnectionName");
            configurationSectionMock.SetupGet(c => c["ConnectionString"]).Returns((string)null);

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetConnectionString("myConnectionName")).Returns("myConnectionString");

            services.AddSingleton(configurationMock.Object);

            var providerBuilder = new RedisClusteringProviderBuilder();

            // Act
            providerBuilder.Configure(builderMock.Object, "name", configurationSectionMock.Object);

            // Build service provider to resolve options
            var sp = services.BuildServiceProvider();
            var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<RedisClusteringOptions>>();
            var options = optionsMonitor.Get("name");

            // Assert
            Assert.NotNull(options.ConfigurationOptions);
            Assert.Equal("myConnectionString", options.ConfigurationOptions.ToString());
        }
    }

    // Extension method mock for GetRequiredKeyedService to allow mocking
    public static class ServiceProviderExtensions
    {
        public static T GetRequiredKeyedService<T>(this IServiceProvider provider, string key)
        {
            // This method is mocked in tests
            throw new NotImplementedException();
        }
    }
}
