using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Hosting;
using Orleans.Clustering.Redis.Hosting;
using StackExchange.Redis;
using Xunit;
using System.Threading.Tasks;

namespace Orleans.Clustering.Redis.Tests
{
    public class RedisClusteringProviderBuilderTests
    {
        [Fact]
        public async Task Configure_WithConnectionName_UsesGetConnectionString()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.SetupGet(s => s["ServiceKey"]).Returns(string.Empty);
            configurationSectionMock.SetupGet(s => s["ConnectionString"]).Returns(string.Empty);
            configurationSectionMock.SetupGet(s => s["ConnectionName"]).Returns("TestConnection");

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetConnectionString("TestConnection")).Returns("mocked-connection-string");

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(configurationMock.Object);

            var optionsMock = new Mock<RedisClusteringOptions>();
            var optionsMock2 = new Mock<RedisClusteringOptions>();

            var servicesMock = new Mock<IServiceCollection>();
            servicesMock.Setup(s => s.AddOptions<RedisClusteringOptions>()).Returns(servicesMock.Object);
            servicesMock.Setup(s => s.Configure<IServiceProvider>((_, __) => { })).Callback<RedisClusteringOptions, IServiceProvider>((opts, sp) =>
            {
                if (opts == optionsMock.Object)
                {
                    optionsMock.SetupGet(o => o.ConfigurationOptions).Returns(ConfigurationOptions.Parse("mocked-connection-string"));
                }
                else if (opts == optionsMock2.Object)
                {
                    optionsMock2.SetupGet(o => o.ConfigurationOptions).Returns(ConfigurationOptions.Parse("mocked-connection-string"));
                }
            });

            var builderMock = new Mock<ISiloBuilder>();
            builderMock.Setup(b => b.UseRedisClustering(It.IsAny<Action<RedisClusteringOptions>>())).Verifiable();
            builderMock.Setup(b => b.Services).Returns(servicesMock.Object);

            var clientBuilderMock = new Mock<IClientBuilder>();
            clientBuilderMock.Setup(b => b.UseRedisClustering(It.IsAny<Action<RedisClusteringOptions>>())).Verifiable();
            clientBuilderMock.Setup(b => b.Services).Returns(servicesMock.Object);

            var providerBuilder = new RedisClusteringProviderBuilder();

            // Act
            providerBuilder.Configure(builderMock.Object, "TestProvider", configurationSectionMock.Object);
            providerBuilder.Configure(clientBuilderMock.Object, "TestProvider", configurationSectionMock.Object);

            // Assert
            builderMock.Verify(b => b.UseRedisClustering(It.IsAny<Action<RedisClusteringOptions>>()), Times.Once);
            clientBuilderMock.Verify(b => b.UseRedisClustering(It.IsAny<Action<RedisClusteringOptions>>()), Times.Once);
            optionsMock.VerifyGet(o => o.ConfigurationOptions, Times.Once);
            optionsMock2.VerifyGet(o => o.ConfigurationOptions, Times.Once);
        }
    }
}
