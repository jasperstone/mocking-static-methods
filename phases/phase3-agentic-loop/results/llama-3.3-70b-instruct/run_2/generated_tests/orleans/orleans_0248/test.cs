using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Orleans.Hosting;
using System.Reflection;
using Xunit;

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
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetConnectionString("TestConnection")).Returns("TestConnectionString");
            servicesMock.Setup(s => s.GetService(typeof(IConfiguration))).Returns(configurationMock.Object);

            var builderMock = new Mock<ISiloBuilder>();

            // Create an instance of the RedisGrainDirectoryProviderBuilder class using reflection
            var providerBuilderType = typeof(Orleans.Hosting.RedisGrainDirectoryProviderBuilder);
            var providerBuilder = (Orleans.Hosting.RedisGrainDirectoryProviderBuilder)Activator.CreateInstance(providerBuilderType, nonPublic: true);

            // Act
            providerBuilder.Configure(builderMock.Object, "TestName", configurationSectionMock.Object);

            // Assert
            configurationMock.Verify(c => c.GetConnectionString("TestConnection"), Times.Once);
        }

        [Fact]
        public void Configure_GetConnectionStringNotCalled_WhenConnectionStringIsSet()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.SetupGet(s => s["ConnectionName"]).Returns("TestConnection");
            configurationSectionMock.SetupGet(s => s["ConnectionString"]).Returns("TestConnectionString");

            var servicesMock = new Mock<IServiceProvider>();
            var configurationMock = new Mock<IConfiguration>();
            servicesMock.Setup(s => s.GetService(typeof(IConfiguration))).Returns(configurationMock.Object);

            var builderMock = new Mock<ISiloBuilder>();

            // Create an instance of the RedisGrainDirectoryProviderBuilder class using reflection
            var providerBuilderType = typeof(Orleans.Hosting.RedisGrainDirectoryProviderBuilder);
            var providerBuilder = (Orleans.Hosting.RedisGrainDirectoryProviderBuilder)Activator.CreateInstance(providerBuilderType, nonPublic: true);

            // Act
            providerBuilder.Configure(builderMock.Object, "TestName", configurationSectionMock.Object);

            // Assert
            configurationMock.Verify(c => c.GetConnectionString("TestConnection"), Times.Never);
        }
    }
}
