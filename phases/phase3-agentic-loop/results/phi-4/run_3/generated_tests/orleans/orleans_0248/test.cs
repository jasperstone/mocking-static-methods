using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Orleans.Hosting;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Orleans.GrainDirectory.Redis.Hosting.Tests")]

namespace Orleans.GrainDirectory.Redis.Hosting.Tests
{
    public class RedisGrainDirectoryProviderBuilderTests
    {
        [Fact]
        public void Configure_WhenConnectionNameProvidedAndConnectionStringNotProvided_CallsGetConnectionString()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(c => c["ServiceKey"]).Returns(string.Empty);
            configurationSectionMock.Setup(c => c["ConnectionName"]).Returns("TestConnection");
            configurationSectionMock.Setup(c => c["ConnectionString"]).Returns(string.Empty);

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetConnectionString("TestConnection")).Returns("TestConnectionString");

            var servicesMock = new Mock<IServiceProvider>();
            servicesMock.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(configurationMock.Object);

            var builderMock = new Mock<ISiloBuilder>();
            var optionsBuilderMock = new Mock<OptionsBuilder<RedisGrainDirectoryOptions>>();

            // Act
            var providerBuilder = new RedisGrainDirectoryProviderBuilder();
            providerBuilder.Configure(builderMock.Object, "TestName", configurationSectionMock.Object);

            // Assert
            configurationMock.Verify(c => c.GetConnectionString("TestConnection"), Times.Once);
        }
    }
}
