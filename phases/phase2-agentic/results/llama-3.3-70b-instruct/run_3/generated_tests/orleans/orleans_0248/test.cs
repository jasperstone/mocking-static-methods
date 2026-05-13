using Xunit;
using Microsoft.Extensions.Configuration;
using Moq;
using Orleans.Hosting;
using System;
using System.Threading.Tasks;

namespace Orleans.Tests
{
    public class RedisGrainDirectoryProviderBuilderTests
    {
        [Fact]
        public void Configure_GetConnectionStringCalled_WhenConnectionNameIsSet()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.SetupGet(cs => cs["ConnectionName"]).Returns("TestConnection");
            configurationSectionMock.SetupGet(cs => cs["ConnectionString"]).Returns(string.Empty);

            var servicesMock = new Mock<IServiceProvider>();
            var rootConfigurationMock = new Mock<IConfiguration>();
            rootConfigurationMock.Setup(rc => rc.GetConnectionString("TestConnection")).Returns("TestConnectionString");
            servicesMock.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(rootConfigurationMock.Object);

            var builderMock = new Mock<ISiloBuilder>();
            var optionsBuilderMock = new Mock<OptionsBuilder<RedisGrainDirectoryOptions>>();

            // Act
            var providerBuilder = new RedisGrainDirectoryProviderBuilder();
            providerBuilder.Configure(builderMock.Object, "TestName", configurationSectionMock.Object);

            // Assert
            rootConfigurationMock.Verify(rc => rc.GetConnectionString("TestConnection"), Times.Once);
        }

        [Fact]
        public void Configure_GetConnectionStringNotCalled_WhenConnectionStringIsSet()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.SetupGet(cs => cs["ConnectionName"]).Returns("TestConnection");
            configurationSectionMock.SetupGet(cs => cs["ConnectionString"]).Returns("TestConnectionString");

            var servicesMock = new Mock<IServiceProvider>();
            var rootConfigurationMock = new Mock<IConfiguration>();
            servicesMock.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(rootConfigurationMock.Object);

            var builderMock = new Mock<ISiloBuilder>();
            var optionsBuilderMock = new Mock<OptionsBuilder<RedisGrainDirectoryOptions>>();

            // Act
            var providerBuilder = new RedisGrainDirectoryProviderBuilder();
            providerBuilder.Configure(builderMock.Object, "TestName", configurationSectionMock.Object);

            // Assert
            rootConfigurationMock.Verify(rc => rc.GetConnectionString("TestConnection"), Times.Never);
        }

        [Fact]
        public void Configure_GetConnectionStringNotCalled_WhenConnectionNameIsNotSet()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.SetupGet(cs => cs["ConnectionName"]).Returns(string.Empty);
            configurationSectionMock.SetupGet(cs => cs["ConnectionString"]).Returns(string.Empty);

            var servicesMock = new Mock<IServiceProvider>();
            var rootConfigurationMock = new Mock<IConfiguration>();
            servicesMock.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(rootConfigurationMock.Object);

            var builderMock = new Mock<ISiloBuilder>();
            var optionsBuilderMock = new Mock<OptionsBuilder<RedisGrainDirectoryOptions>>();

            // Act
            var providerBuilder = new RedisGrainDirectoryProviderBuilder();
            providerBuilder.Configure(builderMock.Object, "TestName", configurationSectionMock.Object);

            // Assert
            rootConfigurationMock.Verify(rc => rc.GetConnectionString(It.IsAny<string>()), Times.Never);
        }
    }
}
