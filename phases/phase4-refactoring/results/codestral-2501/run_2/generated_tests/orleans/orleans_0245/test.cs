using Xunit;
using Orleans.Clustering.Redis.Hosting;
using Orleans.Hosting;
using Microsoft.Extensions.Configuration;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System.Threading.Tasks;

public class RedisClusteringProviderBuilderTests
{
    [Fact]
    public void Configure_WithConnectionName_ShouldCallGetConnectionString()
    {
        // Arrange
        var mockConfigurationSection = new Mock<IConfigurationSection>();
        mockConfigurationSection.Setup(c => c["ServiceKey"]).Returns((string)null);
        mockConfigurationSection.Setup(c => c["ConnectionName"]).Returns("TestConnection");
        mockConfigurationSection.Setup(c => c["ConnectionString"]).Returns((string)null);

        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c.GetConnectionString("TestConnection")).Returns("TestConnectionString");

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(sp => sp.GetService(typeof(IConfiguration))).Returns(mockConfiguration.Object);

        var mockServiceCollection = new Mock<IServiceCollection>();
        mockServiceCollection.Setup(sc => sc.BuildServiceProvider()).Returns(mockServiceProvider.Object);

        var mockSiloBuilder = new Mock<ISiloBuilder>();
        mockSiloBuilder.Setup(sb => sb.Services).Returns(mockServiceCollection.Object);

        var builder = new RedisClusteringProviderBuilder();

        // Act
        builder.Configure(mockSiloBuilder.Object, "Test", mockConfigurationSection.Object);

        // Assert
        mockConfiguration.Verify(c => c.GetConnectionString("TestConnection"), Times.Once);
    }
}
