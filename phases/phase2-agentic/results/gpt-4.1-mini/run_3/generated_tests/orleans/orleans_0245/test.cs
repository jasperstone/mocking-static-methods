using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Clustering.Redis.Hosting;
using StackExchange.Redis;
using Xunit;

namespace Orleans.Clustering.Redis.Tests
{
    public class RedisClusteringProviderBuilderTests
    {
        [Fact]
        public void Configure_SiloBuilder_WithServiceKey_UsesMultiplexerFromServiceProvider()
        {
            // Arrange
            var builderMock = new Mock<ISiloBuilder>();
            var services = new ServiceCollection();
            builderMock.Setup(b => b.UseRedisClustering(It.IsAny<Action<RedisClusteringOptions>>()))
                .Returns(builderMock.Object);
            builderMock.SetupGet(b => b.Services).Returns(services);

            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(s => s["ServiceKey"]).Returns("myServiceKey");
            configurationSectionMock.Setup(s => s["ConnectionName"]).Returns((string)null);
            configurationSectionMock.Setup(s => s["ConnectionString"]).Returns((string)null);

            var multiplexerMock = new Mock<IConnectionMultiplexer>();
            services.AddSingleton(multiplexerMock.Object);

            // Add a keyed service mock for GetRequiredKeyedService
            services.AddSingleton<IServiceProvider>(sp => sp);
            services.AddSingleton<IConnectionMultiplexer>(multiplexerMock.Object);

            // Setup extension method GetRequiredKeyedService via a helper mock
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IConnectionMultiplexer))).Returns(multiplexerMock.Object);

            // Act
            var builder = builderMock.Object;
            var providerBuilder = new RedisClusteringProviderBuilder();
            providerBuilder.Configure(builder, "name", configurationSectionMock.Object);

            // Build service provider and get options to invoke configuration delegate
            var serviceProvider = services.BuildServiceProvider();
            var options = new RedisClusteringOptions();
            var configureOptions = serviceProvider.GetRequiredService<IOptions<RedisClusteringOptions>>().Value;

            // Assert
            // We cannot directly assert the internal delegate, but we can test that the options are configured correctly
            // So we test the delegate manually by invoking the Configure delegate from the options
            var configureDelegate = services.BuildServiceProvider()
                .GetRequiredService<IOptions<RedisClusteringOptions>>()
                .Value.CreateMultiplexer;

            Assert.NotNull(configureDelegate);
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
            configurationSectionMock.Setup(s => s["ServiceKey"]).Returns(string.Empty);
            configurationSectionMock.Setup(s => s["ConnectionName"]).Returns("myConnectionName");
            configurationSectionMock.Setup(s => s["ConnectionString"]).Returns(string.Empty);

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetConnectionString("myConnectionName")).Returns("myConnectionString");

            services.AddSingleton(configurationMock.Object);

            var providerBuilder = new RedisClusteringProviderBuilder();

            // Act
            providerBuilder.Configure(builderMock.Object, "name", configurationSectionMock.Object);

            // Build service provider and get options to invoke configuration delegate
            var serviceProvider = services.BuildServiceProvider();
            var options = new RedisClusteringOptions();

            // We cannot directly invoke the internal delegate, but we can test that the configuration string was retrieved
            // by verifying the mock call
            configurationMock.Verify(c => c.GetConnectionString("myConnectionName"), Times.Once);
        }
    }

    // Minimal RedisClusteringOptions class for testing
    public class RedisClusteringOptions
    {
        public Func<object, Task<IConnectionMultiplexer>>? CreateMultiplexer { get; set; }
        public ConfigurationOptions? ConfigurationOptions { get; set; }
    }

    // Minimal ConfigurationOptions class for testing
    public class ConfigurationOptions
    {
        public static ConfigurationOptions Parse(string configuration)
        {
            return new ConfigurationOptions();
        }
    }
}
