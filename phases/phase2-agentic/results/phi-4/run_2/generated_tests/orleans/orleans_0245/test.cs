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
        [Fact]
        public void Configure_WithConnectionName_UsesGetConnectionString()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(c => c["ServiceKey"]).Returns(string.Empty);
            configurationSectionMock.Setup(c => c["ConnectionName"]).Returns("TestConnection");
            configurationSectionMock.Setup(c => c["ConnectionString"]).Returns(string.Empty);

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetConnectionString("TestConnection")).Returns("redis://localhost:6379");

            var servicesMock = new Mock<IServiceProvider>();
            servicesMock.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(configurationMock.Object);

            var optionsMock = new Mock<RedisClusteringOptions>();
            var optionsProviderMock = new Mock<IOptions<RedisClusteringOptions>>();
            optionsProviderMock.Setup(p => p.Value).Returns(optionsMock.Object);

            servicesMock.Setup(s => s.GetRequiredService<IOptions<RedisClusteringOptions>>()).Returns(optionsProviderMock.Object);

            var builderMock = new Mock<ISiloBuilder>();
            builderMock.Setup(b => b.UseRedisClustering(It.IsAny<Action<RedisClusteringOptions>>())).Verifiable();

            var providerBuilder = new RedisClusteringProviderBuilder();

            // Act
            providerBuilder.Configure(builderMock.Object, "TestProvider", configurationSectionMock.Object);

            // Assert
            builderMock.Verify(b => b.UseRedisClustering(It.IsAny<Action<RedisClusteringOptions>>()), Times.Once);
            optionsMock.VerifySet(o => o.ConfigurationOptions = It.Is<ConfigurationOptions>(c => c.EndPoints[0] == "localhost:6379"), Times.Once);
        }
    }
}
