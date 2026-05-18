using Xunit;
using Orleans.Clustering.Redis.Hosting;
using Orleans.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using StackExchange.Redis;
using System.Threading.Tasks;

public class RedisClusteringProviderBuilderTests
{
    [Fact]
    public void Configure_WithConnectionName_ShouldCallGetConnectionString()
    {
        // Arrange
        var mockSiloBuilder = new Mock<ISiloBuilder>();
        var mockConfigurationSection = new Mock<IConfigurationSection>();
        var mockConfiguration = new Mock<IConfiguration>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockServiceCollection = new Mock<IServiceCollection>();

        mockSiloBuilder.Setup(b => b.Services).Returns(mockServiceCollection.Object);
        mockServiceCollection.Setup(s => s.Add(It.IsAny<ServiceDescriptor>())).Returns(mockServiceCollection.Object);
        mockServiceProvider.Setup(s => s.GetService(typeof(IConfiguration))).Returns(mockConfiguration.Object);

        mockConfigurationSection.Setup(c => c["ConnectionName"]).Returns("TestConnectionName");
        mockConfigurationSection.Setup(c => c["ConnectionString"]).Returns((string)null);

        mockConfiguration.Setup(c => c.GetConnectionString("TestConnectionName")).Returns("TestConnectionString");

        var builder = new RedisClusteringProviderBuilder();

        // Act
        builder.Configure(mockSiloBuilder.Object, "TestProvider", mockConfigurationSection.Object);

        // Assert
        mockConfiguration.Verify(c => c.GetConnectionString("TestConnectionName"), Times.Once);
    }
}
