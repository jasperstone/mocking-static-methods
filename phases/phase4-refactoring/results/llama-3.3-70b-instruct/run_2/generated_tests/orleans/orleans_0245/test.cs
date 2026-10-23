using Microsoft.Extensions.Configuration;
using Orleans.Clustering.Redis.Hosting;
using Orleans;
using Xunit;
using Moq;

namespace Orleans.Clustering.Redis.Tests
{
    public class RedisClusteringProviderBuilderTests
    {
        [Fact]
        public void Configure_GetConnectionStringCalled()
        {
            // Arrange
            var configurationSection = new ConfigurationSection(new ConfigurationBuilder().Build(), "section");
            configurationSection["ConnectionName"] = "connection_name";
            var configuration = new Mock<IConfiguration>();
            var services = new Mock<IServiceProvider>();
            services.Setup(s => s.GetService(typeof(IConfiguration))).Returns(configuration.Object);
            var builder = new Mock<Orleans.ISiloBuilder>();
            var providerBuilder = new Orleans.Clustering.Redis.Hosting.RedisClusteringProviderBuilder();

            configuration.Setup(c => c.GetConnectionString(It.IsAny<string>())).Returns("connection_string");

            // Act
            providerBuilder.Configure(builder.Object, "name", configurationSection);

            // Assert
            configuration.Verify(c => c.GetConnectionString(It.IsAny<string>()), Times.Once);
        }
    }
}
