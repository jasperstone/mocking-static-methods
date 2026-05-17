using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Orleans.Hosting;
using System.Threading.Tasks;
using StackExchange.Redis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;

namespace Orleans.GrainDirectory.Redis.Hosting.Tests
{
    internal class RedisGrainDirectoryProviderBuilderTests
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

            var optionsBuilderMock = new Mock<OptionsBuilder<RedisGrainDirectoryOptions>>();
            var optionsMock = new Mock<RedisGrainDirectoryOptions>();
            optionsBuilderMock.Setup(o => o.Configure(It.IsAny<IServiceProvider>())).Callback<IServiceProvider>(services => 
            {
                servicesMock.Verify(s => s.GetRequiredService<IConfiguration>(), Times.Once);
                configurationMock.Verify(c => c.GetConnectionString("TestConnection"), Times.Once);
            });

            var builderMock = new Mock<ISiloBuilder>();
            builderMock.Setup(b => b.AddRedisGrainDirectory(It.IsAny<string>(), It.IsAny<Action<OptionsBuilder<RedisGrainDirectoryOptions>>>()))
                .Callback<string, Action<OptionsBuilder<RedisGrainDirectoryOptions>>>(name, configure =>
                {
                    configure(optionsBuilderMock.Object);
                });

            var providerBuilder = new RedisGrainDirectoryProviderBuilder();

            // Act
            providerBuilder.Configure(builderMock.Object, "TestName", configurationSectionMock.Object);

            // Assert
            builderMock.Verify(b => b.AddRedisGrainDirectory("TestName", It.IsAny<Action<OptionsBuilder<RedisGrainDirectoryOptions>>>()), Times.Once);
        }
    }
}
