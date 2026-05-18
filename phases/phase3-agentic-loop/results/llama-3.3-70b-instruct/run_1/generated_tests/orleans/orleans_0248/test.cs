using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Orleans;
using Orleans.Hosting;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Orleans.Tests
{
    public class RedisGrainDirectoryProviderBuilderTests
    {
        [Fact]
        public void Configure_GetConnectionStringCalled_WhenConnectionNameIsSet()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.SetupGet(s => s["ConnectionName"]).Returns("TestConnection");
            configurationSectionMock.SetupGet(s => s["ConnectionString"]).Returns(string.Empty);

            var servicesMock = new Mock<IServiceProvider>();
            var rootConfigurationMock = new Mock<IConfiguration>();
            rootConfigurationMock.Setup(c => c.GetConnectionString("TestConnection")).Returns("TestConnectionString");
            servicesMock.Setup(s => s.GetService(typeof(IConfiguration))).Returns(rootConfigurationMock.Object);

            var builderMock = new Mock<ISiloBuilder>();

            // Act
            var providerBuilder = new Orleans.Hosting.RedisGrainDirectoryProviderBuilder();
            providerBuilder.Configure(builderMock.Object, "TestName", configurationSectionMock.Object);

            // Assert
            rootConfigurationMock.Verify(c => c.GetConnectionString("TestConnection"), Times.Once);
        }

        [Fact]
        public void Configure_GetConnectionStringNotCalled_WhenConnectionStringIsSet()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.SetupGet(s => s["ConnectionName"]).Returns("TestConnection");
            configurationSectionMock.SetupGet(s => s["ConnectionString"]).Returns("TestConnectionString");

            var servicesMock = new Mock<IServiceProvider>();
            var rootConfigurationMock = new Mock<IConfiguration>();
            servicesMock.Setup(s => s.GetService(typeof(IConfiguration))).Returns(rootConfigurationMock.Object);

            var builderMock = new Mock<ISiloBuilder>();

            // Act
            var providerBuilder = new Orleans.Hosting.RedisGrainDirectoryProviderBuilder();
            providerBuilder.Configure(builderMock.Object, "TestName", configurationSectionMock.Object);

            // Assert
            rootConfigurationMock.Verify(c => c.GetConnectionString("TestConnection"), Times.Never);
        }

        [Fact]
        public void Configure_GetConnectionStringNotCalled_WhenConnectionNameIsNotSet()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.SetupGet(s => s["ConnectionName"]).Returns(string.Empty);
            configurationSectionMock.SetupGet(s => s["ConnectionString"]).Returns(string.Empty);

            var servicesMock = new Mock<IServiceProvider>();
            var rootConfigurationMock = new Mock<IConfiguration>();
            servicesMock.Setup(s => s.GetService(typeof(IConfiguration))).Returns(rootConfigurationMock.Object);

            var builderMock = new Mock<ISiloBuilder>();

            // Act
            var providerBuilder = new Orleans.Hosting.RedisGrainDirectoryProviderBuilder();
            providerBuilder.Configure(builderMock.Object, "TestName", configurationSectionMock.Object);

            // Assert
            rootConfigurationMock.Verify(c => c.GetConnectionString(It.IsAny<string>()), Times.Never);
        }
    }
}
