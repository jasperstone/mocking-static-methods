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
        public void Configure_GetConnectionStringCalled_WhenConnectionNameIsSetAndConnectionStringIsEmpty()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.SetupGet(cs => cs["ConnectionName"]).Returns("TestConnection");
            configurationSectionMock.SetupGet(cs => cs["ConnectionString"]).Returns(string.Empty);

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
        public void Configure_GetConnectionStringNotCalled_WhenConnectionNameIsEmpty()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.SetupGet(cs => cs["ConnectionName"]).Returns(string.Empty);
            configurationSectionMock.SetupGet(cs => cs["ConnectionString"]).Returns(string.Empty);

            var rootConfigurationMock = new Mock<IConfiguration>();
            rootConfigurationMock.Setup(rc => rc.GetConnectionString("TestConnection")).Returns("TestConnectionString");

            var servicesMock = new Mock<IServiceProvider>();
            servicesMock.Setup(sp => sp.GetRequiredService<IConfiguration>()).Returns(rootConfigurationMock.Object);

            var builderMock = new Mock<ISiloBuilder>();
            var providerBuilder = new RedisClusteringProviderBuilder();

            // Act
            providerBuilder.Configure(builderMock.Object, "TestProvider", configurationSectionMock.Object);

            // Assert
            rootConfigurationMock.Verify(rc => rc.GetConnectionString(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Configure_GetConnectionStringNotCalled_WhenConnectionStringIsSet()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.SetupGet(cs => cs["ConnectionName"]).Returns("TestConnection");
            configurationSectionMock.SetupGet(cs => cs["ConnectionString"]).Returns("TestConnectionString");

            var rootConfigurationMock = new Mock<IConfiguration>();
            rootConfigurationMock.Setup(rc => rc.GetConnectionString("TestConnection")).Returns("TestConnectionString");

            var servicesMock = new Mock<IServiceProvider>();
            servicesMock.Setup(sp => sp.GetRequiredService<IConfiguration>()).Returns(rootConfigurationMock.Object);

            var builderMock = new Mock<ISiloBuilder>();
            var providerBuilder = new RedisClusteringProviderBuilder();

            // Act
            providerBuilder.Configure(builderMock.Object, "TestProvider", configurationSectionMock.Object);

            // Assert
            rootConfigurationMock.Verify(rc => rc.GetConnectionString(It.IsAny<string>()), Times.Never);
        }
    }
}
