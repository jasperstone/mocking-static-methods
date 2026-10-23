using Xunit;
using Microsoft.Extensions.Configuration;
using Moq;
using Orleans.Hosting;
using System;
using System.Threading.Tasks;
using Orleans.Configuration;

namespace Orleans.Tests
{
    public class RedisGrainDirectoryProviderBuilderTests
    {
        [Fact]
        public async Task Configure_GetConnectionStringCalled_WhenConnectionNameIsSet()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.SetupGet(s => s["ConnectionName"]).Returns("TestConnection");
            configurationSectionMock.SetupGet(s => s["ConnectionString"]).Returns(string.Empty);

            var servicesMock = new Mock<IServiceProvider>();
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetConnectionString("TestConnection")).Returns("TestConnectionString");

            servicesMock.Setup(s => s.GetService(typeof(IConfiguration))).Returns(configurationMock.Object);

            var builderMock = new Mock<ISiloBuilder>();

            // Act
            var providerBuilder = new RedisGrainDirectoryProviderBuilder();
            providerBuilder.Configure(builderMock.Object, "TestName", configurationSectionMock.Object);

            // Assert
            configurationMock.Verify(c => c.GetConnectionString("TestConnection"), Times.Once);
        }

        [Fact]
        public async Task Configure_GetConnectionStringNotCalled_WhenConnectionStringIsSet()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.SetupGet(s => s["ConnectionName"]).Returns("TestConnection");
            configurationSectionMock.SetupGet(s => s["ConnectionString"]).Returns("TestConnectionString");

            var servicesMock = new Mock<IServiceProvider>();
            var configurationMock = new Mock<IConfiguration>();

            servicesMock.Setup(s => s.GetService(typeof(IConfiguration))).Returns(configurationMock.Object);

            var builderMock = new Mock<ISiloBuilder>();

            // Act
            var providerBuilder = new RedisGrainDirectoryProviderBuilder();
            providerBuilder.Configure(builderMock.Object, "TestName", configurationSectionMock.Object);

            // Assert
            configurationMock.Verify(c => c.GetConnectionString("TestConnection"), Times.Never);
        }
    }
}
