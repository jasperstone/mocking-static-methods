using Xunit;
using Orleans.Clustering.Redis.Hosting;
using Orleans.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Orleans.Clustering.Redis;
using StackExchange.Redis;

namespace Orleans.Clustering.Redis.Tests
{
    public class RedisClusteringProviderBuilderTests
    {
        [Fact]
        public void Configure_WithConnectionName_ShouldSetConnectionString()
        {
            // Arrange
            var mockSiloBuilder = new Mock<ISiloBuilder>();
            var mockConfigurationSection = new Mock<IConfigurationSection>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockServiceCollection = new Mock<IServiceCollection>();

            mockSiloBuilder.Setup(b => b.Services).Returns(mockServiceCollection.Object);
            mockServiceCollection.Setup(s => s.AddOptions<RedisClusteringOptions>()).Returns(mockServiceCollection.Object);

            mockConfigurationSection.Setup(c => c["ConnectionName"]).Returns("TestConnection");
            mockConfigurationSection.Setup(c => c["ConnectionString"]).Returns((string)null);

            mockServiceProvider.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(mockConfiguration.Object);
            mockConfiguration.Setup(c => c.GetConnectionString("TestConnection")).Returns("TestConnectionString");

            var builder = new RedisClusteringProviderBuilder();

            // Act
            builder.Configure(mockSiloBuilder.Object, "Test", mockConfigurationSection.Object);

            // Assert
            mockServiceCollection.Verify(s => s.Configure<RedisClusteringOptions>(It.IsAny<Action<RedisClusteringOptions>>()), Times.Once);
        }
    }
}
