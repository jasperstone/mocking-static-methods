using Xunit;
using Orleans.Clustering.Redis.Hosting;
using Orleans.Hosting;
using Microsoft.Extensions.Configuration;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Orleans.Clustering.Redis;
using Microsoft.Extensions.Options;

namespace Orleans.Clustering.Redis.Tests
{
    public class RedisClusteringProviderBuilderTests
    {
        [Fact]
        public void Configure_WithConnectionName_ShouldCallGetConnectionString()
        {
            // Arrange
            var mockConfigurationSection = new Mock<IConfigurationSection>();
            mockConfigurationSection.Setup(x => x["ConnectionName"]).Returns("TestConnectionName");
            mockConfigurationSection.Setup(x => x["ConnectionString"]).Returns((string)null);

            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(x => x.GetConnectionString("TestConnectionName")).Returns("TestConnectionString");

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(x => x.GetService(typeof(IConfiguration))).Returns(mockConfiguration.Object);

            var mockServiceCollection = new Mock<IServiceCollection>();
            var mockOptionsBuilder = new Mock<IOptionsBuilder<RedisClusteringOptions>>();
            mockServiceCollection.Setup(x => x.AddOptions<RedisClusteringOptions>()).Returns(mockOptionsBuilder.Object);

            var mockSiloBuilder = new Mock<ISiloBuilder>();
            mockSiloBuilder.Setup(x => x.Services).Returns(mockServiceCollection.Object);

            var builder = new RedisClusteringProviderBuilder();

            // Act
            builder.Configure(mockSiloBuilder.Object, "TestName", mockConfigurationSection.Object);

            // Assert
            mockConfiguration.Verify(x => x.GetConnectionString("TestConnectionName"), Times.Once);
        }
    }
}
