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

namespace Orleans.Clustering.Redis.Hosting
{
    // Public wrapper to allow testing internal RedisClusteringProviderBuilder
    public class RedisClusteringProviderBuilderTestWrapper
    {
        private readonly RedisClusteringProviderBuilder _inner = new();

        public void Configure(ISiloBuilder builder, string name, IConfigurationSection configurationSection)
        {
            _inner.Configure(builder, name, configurationSection);
        }
    }

    public class RedisClusteringProviderBuilderTests
    {
        [Fact]
        public void Configure_SiloBuilder_WithServiceKey_UsesMultiplexerFromServices()
        {
            var builderMock = new Mock<ISiloBuilder>();
            var services = new ServiceCollection();
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

            var wrapper = new RedisClusteringProviderBuilderTestWrapper();
            wrapper.Configure(builderMock.Object, "name", configurationSectionMock.Object);

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<RedisClusteringOptions>>().Value;

            Assert.NotNull(options.CreateMultiplexer);
            var multiplexerTask = options.CreateMultiplexer(null);
            Assert.Same(multiplexerMock.Object, multiplexerTask.Result);
            Assert.NotNull(options.ConfigurationOptions);
        }

        [Fact]
        public void Configure_SiloBuilder_WithConnectionName_CallsGetConnectionString()
        {
            var builderMock = new Mock<ISiloBuilder>();
            var services = new ServiceCollection();
            builderMock.SetupGet(b => b.Services).Returns(services);

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetConnectionString("myConnectionName")).Returns("myConnectionString");

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IConfiguration>()).Returns(configurationMock.Object);

            services.AddSingleton(serviceProviderMock.Object);

            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.SetupGet(c => c["ServiceKey"]).Returns((string)null);
            configurationSectionMock.SetupGet(c => c["ConnectionName"]).Returns("myConnectionName");
            configurationSectionMock.SetupGet(c => c["ConnectionString"]).Returns((string)null);

            var wrapper = new RedisClusteringProviderBuilderTestWrapper();
            wrapper.Configure(builderMock.Object, "name", configurationSectionMock.Object);

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<RedisClusteringOptions>>().Value;

            Assert.NotNull(options.ConfigurationOptions);
            Assert.Equal("myConnectionString", options.ConfigurationOptions.ToString());
        }
    }
}
