using Xunit;
using Orleans.Hosting;
using Microsoft.Extensions.Configuration;
using Moq;
using Orleans;
using Orleans.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using StackExchange.Redis;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Orleans.GrainDirectory.Redis.Tests")]

namespace Orleans.GrainDirectory.Redis.Tests
{
    public class RedisGrainDirectoryProviderBuilderTests
    {
        [Fact]
        public void Configure_ShouldSetConnectionStringFromRootConfiguration()
        {
            // Arrange
            var mockSiloBuilder = new Mock<ISiloBuilder>();
            var mockConfigurationSection = new Mock<IConfigurationSection>();
            var mockRootConfiguration = new Mock<IConfiguration>();
            var mockServiceProvider = new Mock<IServiceProvider>();

            var connectionName = "TestConnection";
            var connectionString = "TestConnectionString";

            mockConfigurationSection.Setup(c => c["ConnectionName"]).Returns(connectionName);
            mockConfigurationSection.Setup(c => c["ConnectionString"]).Returns((string)null);
            mockRootConfiguration.Setup(c => c.GetConnectionString(connectionName)).Returns(connectionString);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IConfiguration))).Returns(mockRootConfiguration.Object);

            var builder = new RedisGrainDirectoryProviderBuilder();

            // Act
            builder.Configure(mockSiloBuilder.Object, "TestName", mockConfigurationSection.Object);

            // Assert
            mockSiloBuilder.Verify(
                sb => sb.AddRedisGrainDirectory(
                    "TestName",
                    It.IsAny<Action<OptionsBuilder<RedisGrainDirectoryOptions>>>()),
                Times.Once);

            // Verify that the connection string is set from the root configuration
            mockRootConfiguration.Verify(c => c.GetConnectionString(connectionName), Times.Once);
        }
    }
}
