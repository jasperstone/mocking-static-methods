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
        public void Configure_WithConnectionName_CallsGetConnectionString()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.SetupGet(s => s["ServiceKey"]).Returns(string.Empty);
            configurationSectionMock.SetupGet(s => s["ConnectionName"]).Returns("TestConnection");
            configurationSectionMock.SetupGet(s => s["ConnectionString"]).Returns(string.Empty);

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetConnectionString("TestConnection")).Returns("mocked-connection-string");

            var servicesMock = new Mock<IServiceProvider>();
            servicesMock.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(configurationMock.Object);

            var builderMock = new Mock<ISiloBuilder>();
            var providerBuilder = new RedisClusteringProviderBuilder();

            // Act
            providerBuilder.Configure(builderMock.Object, "TestProvider", configurationSectionMock.Object);

            // Assert
            configurationMock.Verify(c => c.GetConnectionString("TestConnection"), Times.Once);
        }
    }
}
