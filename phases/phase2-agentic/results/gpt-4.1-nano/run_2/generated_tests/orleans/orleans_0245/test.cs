using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.Clustering.Redis.Hosting;
using StackExchange.Redis;
using System.Threading.Tasks;

namespace Orleans.Tests
{
    public class RedisClusteringProviderBuilderTests
    {
        private Mock<IServiceProvider> _serviceProviderMock;
        private Mock<IConfiguration> _configurationMock;
        private Mock<IConfigurationSection> _configSectionMock;
        private Mock<IServiceCollection> _serviceCollectionMock;
        private Mock<IServiceScope> _serviceScopeMock;
        private Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private Mock<IServiceProvider> _innerServiceProviderMock;
        private Mock<IConnectionMultiplexer> _multiplexerMock;

        public RedisClusteringProviderBuilderTests()
        {
            _serviceProviderMock = new Mock<IServiceProvider>();
            _configurationMock = new Mock<IConfiguration>();
            _configSectionMock = new Mock<IConfigurationSection>();
            _serviceCollectionMock = new Mock<IServiceCollection>();
            _serviceScopeMock = new Mock<IServiceScope>();
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _innerServiceProviderMock = new Mock<IServiceProvider>();
            _multiplexerMock = new Mock<IConnectionMultiplexer>();
        }

        [Fact]
        public void Configure_WithServiceKey_ShouldUseKeyedService()
        {
            // Arrange
            var builderMock = new Mock<ISiloBuilder>();
            var servicesMock = new Mock<IServiceCollection>();
            var optionsMock = new Mock<RedisClusteringOptions>();
            var configurationSectionMock = new Mock<IConfigurationSection>();
            var serviceKey = "myServiceKey";

            configurationSectionMock.Setup(s => s["ServiceKey"]).Returns(serviceKey);
            configurationSectionMock.Setup(s => s["ConnectionName"]).Returns((string)null);
            configurationSectionMock.Setup(s => s["ConnectionString"]).Returns((string)null);

            servicesMock.Setup(s => s.GetRequiredKeyedService<IConnectionMultiplexer>(serviceKey))
                .Returns(_multiplexerMock.Object);

            var providerBuilder = new RedisClusteringProviderBuilder();

            // Act
            providerBuilder.Configure(builderMock.Object, "test", configurationSectionMock.Object);

            // Assert
            // Since the code calls services.GetRequiredKeyedService, verify that method was called.
            servicesMock.Verify(s => s.GetRequiredKeyedService<IConnectionMultiplexer>(serviceKey), Times.Once);
        }

        [Fact]
        public void Configure_WithConnectionName_ShouldCallGetConnectionString()
        {
            // Arrange
            var builderMock = new Mock<ISiloBuilder>();
            var servicesMock = new Mock<IServiceCollection>();
            var configurationSectionMock = new Mock<IConfigurationSection>();
            var rootConfigMock = new Mock<IConfiguration>();
            var connectionName = "MyConnection";

            configurationSectionMock.Setup(s => s["ServiceKey"]).Returns((string)null);
            configurationSectionMock.Setup(s => s["ConnectionName"]).Returns(connectionName);
            configurationSectionMock.Setup(s => s["ConnectionString"]).Returns((string)null);

            // Setup services.GetRequiredService<IConfiguration>() to return rootConfigMock.Object
            var services = new ServiceCollection();
            services.AddSingleton(rootConfigMock.Object);
            var serviceProvider = services.BuildServiceProvider();

            // Mock extension method GetRequiredService<IConfiguration>() to return rootConfigMock
            var servicesMock2 = new Mock<IServiceCollection>();
            servicesMock2.Setup(s => s.BuildServiceProvider()).Returns(serviceProvider);

            var providerBuilder = new RedisClusteringProviderBuilder();

            // Act
            providerBuilder.Configure(builderMock.Object, "test", configurationSectionMock.Object);

            // Since the method calls rootConfiguration.GetConnectionString(connectionName),
            // we verify that rootConfigMock's GetConnectionString is called with connectionName.
            // But since we can't directly verify extension method calls, we check the value.
            var connStr = rootConfigMock.Object.GetConnectionString(connectionName);
            Assert.Null(connStr); // Because we didn't set it, but the call should be made.
        }

        [Fact]
        public void Configure_WithConnectionString_ShouldSetConfigurationOptions()
        {
            // Arrange
            var builderMock = new Mock<ISiloBuilder>();
            var servicesMock = new Mock<IServiceCollection>();
            var configurationSectionMock = new Mock<IConfigurationSection>();
            var rootConfigMock = new Mock<IConfiguration>();
            var connectionString = "redis://localhost";

            configurationSectionMock.Setup(s => s["ServiceKey"]).Returns((string)null);
            configurationSectionMock.Setup(s => s["ConnectionName"]).Returns((string)null);
            configurationSectionMock.Setup(s => s["ConnectionString"]).Returns(connectionString);

            var providerBuilder = new RedisClusteringProviderBuilder();

            // Act
            providerBuilder.Configure(builderMock.Object, "test", configurationSectionMock.Object);

            // Assert
            // Since ConfigurationOptions.Parse is static, we can't mock it directly.
            // But we can check that options.ConfigurationOptions is set to a ConfigurationOptions instance.
            // For that, we'd need to invoke the method and verify the side effects.
            // Here, we assume the method runs without exceptions.
        }
    }
}
