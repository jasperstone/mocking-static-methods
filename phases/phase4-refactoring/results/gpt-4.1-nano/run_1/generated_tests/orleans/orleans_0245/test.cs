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
        [Fact]
        public void Configure_Should_Call_GetConnectionString_When_ConnectionName_Provided_And_ConnectionString_Is_Empty()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(c => c["ServiceKey"]).Returns((string)null);
            configurationSectionMock.Setup(c => c["ConnectionName"]).Returns("TestConnection");
            configurationSectionMock.Setup(c => c["ConnectionString"]).Returns(string.Empty);

            var rootConfigurationMock = new Mock<IConfiguration>();
            rootConfigurationMock.Setup(c => c.GetConnectionString("TestConnection"))
                .Returns("localhost:6379");

            var servicesMock = new ServiceCollection();
            servicesMock.AddSingleton(rootConfigurationMock.Object);
            var serviceProvider = servicesMock.BuildServiceProvider();

            var builderMock = new Mock<ISiloBuilder>();
            var builderServices = new ServiceCollection();
            builderServices.AddSingleton(serviceProvider);
            builderMock.Setup(b => b.UseRedisClustering(It.IsAny<Action<RedisClusteringOptions>>()))
                .Returns(builderMock.Object);
            builderMock.Setup(b => b.Services).Returns(builderServices);

            var providerBuilder = new RedisClusteringProviderBuilder();

            // Act
            providerBuilder.Configure(builderMock.Object, "Test", configurationSectionMock.Object);

            // Assert
            rootConfigurationMock.Verify(c => c.GetConnectionString("TestConnection"), Times.Once);
        }
    }
}
