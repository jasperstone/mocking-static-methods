using Xunit;
using Microsoft.Extensions.Configuration;
using Moq;
using Orleans;
using Orleans.Hosting;
using System;
using System.Threading.Tasks;

namespace Orleans.Tests
{
    public class RedisGrainDirectoryProviderBuilderTests
    {
        [Fact]
        public async Task Configure_CallsGetConnectionString_WhenConnectionNameIsSpecified()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.SetupGet(s => s["ConnectionName"]).Returns("TestConnection");
            configurationSectionMock.SetupGet(s => s["ConnectionString"]).Returns(string.Empty);

            var servicesMock = new Mock<IServiceProvider>();
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetConnectionString("TestConnection")).Returns("TestConnectionString");
            servicesMock.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(configurationMock.Object);

            var optionsBuilderMock = new Mock<OptionsBuilder<RedisGrainDirectoryOptions>>();
            var builderMock = new Mock<ISiloBuilder>();
            builderMock.Setup(b => b.AddRedisGrainDirectory(It.IsAny<string>(), It.IsAny<Action<OptionsBuilder<RedisGrainDirectoryOptions>>>()))
                .Callback<string, Action<OptionsBuilder<RedisGrainDirectoryOptions>>>((name, configure) => configure(optionsBuilderMock.Object));

            var sut = new RedisGrainDirectoryProviderBuilder();

            // Act
            sut.Configure(builderMock.Object, "TestName", configurationSectionMock.Object);

            // Assert
            configurationMock.Verify(c => c.GetConnectionString("TestConnection"), Times.Once);
        }

        [Fact]
        public async Task Configure_DoesNotCallGetConnectionString_WhenConnectionStringIsSpecified()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.SetupGet(s => s["ConnectionName"]).Returns("TestConnection");
            configurationSectionMock.SetupGet(s => s["ConnectionString"]).Returns("TestConnectionString");

            var servicesMock = new Mock<IServiceProvider>();
            var configurationMock = new Mock<IConfiguration>();
            servicesMock.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(configurationMock.Object);

            var optionsBuilderMock = new Mock<OptionsBuilder<RedisGrainDirectoryOptions>>();
            var builderMock = new Mock<ISiloBuilder>();
            builderMock.Setup(b => b.AddRedisGrainDirectory(It.IsAny<string>(), It.IsAny<Action<OptionsBuilder<RedisGrainDirectoryOptions>>>()))
                .Callback<string, Action<OptionsBuilder<RedisGrainDirectoryOptions>>>((name, configure) => configure(optionsBuilderMock.Object));

            var sut = new RedisGrainDirectoryProviderBuilder();

            // Act
            sut.Configure(builderMock.Object, "TestName", configurationSectionMock.Object);

            // Assert
            configurationMock.Verify(c => c.GetConnectionString("TestConnection"), Times.Never);
        }
    }
}
