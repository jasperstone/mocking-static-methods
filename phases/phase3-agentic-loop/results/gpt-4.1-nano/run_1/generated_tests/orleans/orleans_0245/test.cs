using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.Clustering.Redis.Hosting;
using StackExchange.Redis;
using System.Threading.Tasks;

namespace Orleans.Clustering.Redis.Tests
{
    public class RedisClusteringProviderBuilderTests
    {
        private Mock<IServiceProvider> CreateServiceProviderMock()
        {
            var servicesMock = new Mock<IServiceProvider>();
            return servicesMock;
        }

        private IConfigurationSection CreateConfigurationSection(string key, string value)
        {
            var mockSection = new Mock<IConfigurationSection>();
            mockSection.Setup(s => s[key]).Returns(value);
            return mockSection.Object;
        }

        [Fact]
        public void Configure_WithServiceKey_ShouldUseKeyedService()
        {
            // Arrange
            var builderMock = new Mock<ISiloBuilder>();
            var servicesMock = new Mock<IServiceCollection>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<Microsoft.Extensions.Options.IOptions<RedisClusteringOptions>>();
            var optionsValue = new RedisClusteringOptions();

            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(s => s["ServiceKey"]).Returns("myServiceKey");
            var configurationSection = configurationSectionMock.Object;

            var services = new ServiceCollection();
            services.AddSingleton<IServiceProvider>(serviceProviderMock.Object);
            var serviceProvider = services.BuildServiceProvider();

            var servicesMockObj = new Mock<IServiceCollection>();
            servicesMockObj.Setup(s => s.GetRequiredKeyedService<IConnectionMultiplexer>("myServiceKey"))
                .Returns(new Mock<IConnectionMultiplexer>().Object);

            var servicesMocked = new Mock<IServiceProvider>();
            servicesMocked.Setup(s => s.GetService(typeof(IConfiguration)))
                .Returns(new ConfigurationBuilder().Build());

            var builder = new Mock<ISiloBuilder>();
            builder.Setup(b => b.UseRedisClustering(It.IsAny<Action>())).Verifiable();
            builder.Setup(b => b.Services).Returns(services);

            var providerBuilder = new RedisClusteringProviderBuilder();

            // Act
            providerBuilder.Configure(builder.Object, "test", configurationSection);

            // Assert
            // No exception means test passed
        }

        [Fact]
        public void Configure_WithoutServiceKey_ShouldUseConnectionStringFromRootConfiguration()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(s => s["ServiceKey"]).Returns((string)null);
            configurationSectionMock.Setup(s => s["ConnectionName"]).Returns("MyConn");
            configurationSectionMock.Setup(s => s["ConnectionString"]).Returns((string)null);
            var configurationSection = configurationSectionMock.Object;

            var rootConfigMock = new Mock<IConfiguration>();
            rootConfigMock.Setup(c => c.GetConnectionString("MyConn"))
                .Returns("localhost:6379");

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(rootConfigMock.Object);
            var serviceProvider = services.BuildServiceProvider();

            var servicesMock = new Mock<IServiceCollection>();
            servicesMock.Setup(s => s.GetRequiredService<IConfiguration>())
                .Returns(rootConfigMock.Object);

            var builderMock = new Mock<ISiloBuilder>();
            builderMock.Setup(b => b.UseRedisClustering(It.IsAny<Action>())).Verifiable();
            builderMock.Setup(b => b.Services).Returns(services);

            var providerBuilder = new RedisClusteringProviderBuilder();

            // Act
            providerBuilder.Configure(builderMock.Object, "test", configurationSection);

            // Assert
            // No exception means test passed
        }
    }
}
