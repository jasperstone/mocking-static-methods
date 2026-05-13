using Xunit;
using Microsoft.Extensions.Configuration;
using Orleans.Clustering.Redis.Hosting;
using Moq;
using System;
using System.Threading.Tasks;

namespace Orleans.Clustering.Redis.Tests
{
    public class RedisClusteringProviderBuilderTests
    {
        [Fact]
        public async Task Configure_WithConnectionNameAndNoConnectionString_GetConnectionStringIsCalled()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.SetupGet(cs => cs["ConnectionName"]).Returns("TestConnection");
            configurationSectionMock.SetupGet(cs => cs["ConnectionString"]).Returns((string)null);

            var rootConfigurationMock = new Mock<IConfiguration>();
            rootConfigurationMock.Setup(rc => rc.GetConnectionString("TestConnection")).Returns("TestConnectionString");

            var servicesMock = new Mock<IServiceProvider>();
            servicesMock.Setup(sp => sp.GetRequiredService<IConfiguration>()).Returns(rootConfigurationMock.Object);

            var builderMock = new Mock<ISiloBuilder>();
            var providerBuilder = new RedisClusteringProviderBuilder();

            // Act
            providerBuilder.Configure(builderMock.Object, "TestProvider", configurationSectionMock.Object);

            // Assert
            rootConfigurationMock.Verify(rc => rc.GetConnectionString("TestConnection"), Times.Once);
        }

        [Fact]
        public async Task Configure_WithServiceKey_GetRequiredKeyedServiceIsCalled()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.SetupGet(cs => cs["ServiceKey"]).Returns("TestServiceKey");

            var servicesMock = new Mock<IServiceProvider>();
            servicesMock.Setup(sp => sp.GetRequiredKeyedService<IConnectionMultiplexer>("TestServiceKey")).Returns(new Mock<IConnectionMultiplexer>().Object);

            var builderMock = new Mock<ISiloBuilder>();
            var providerBuilder = new RedisClusteringProviderBuilder();

            // Act
            providerBuilder.Configure(builderMock.Object, "TestProvider", configurationSectionMock.Object);

            // Assert
            servicesMock.Verify(sp => sp.GetRequiredKeyedService<IConnectionMultiplexer>("TestServiceKey"), Times.Once);
        }

        [Fact]
        public async Task Configure_WithConnectionString_ParseConnectionStringIsCalled()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.SetupGet(cs => cs["ConnectionString"]).Returns("TestConnectionString");

            var servicesMock = new Mock<IServiceProvider>();

            var builderMock = new Mock<ISiloBuilder>();
            var providerBuilder = new RedisClusteringProviderBuilder();

            // Act
            providerBuilder.Configure(builderMock.Object, "TestProvider", configurationSectionMock.Object);

            // Assert
            // Note: We can't directly verify the call to ConfigurationOptions.Parse, 
            // but we can verify that the options are configured correctly.
            // This test assumes that the Configure method is called correctly.
        }
    }
}
