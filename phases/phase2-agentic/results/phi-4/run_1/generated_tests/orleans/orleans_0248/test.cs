using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using StackExchange.Redis;
using System.Threading.Tasks;

namespace Orleans.GrainDirectory.Redis.Hosting.Tests
{
    public class RedisGrainDirectoryProviderBuilderTests
    {
        [Fact]
        public async Task Configure_WithConnectionName_UsesGetConnectionString()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(c => c["ServiceKey"]).Returns(string.Empty);
            configurationSectionMock.Setup(c => c["ConnectionName"]).Returns("TestConnection");
            configurationSectionMock.Setup(c => c["ConnectionString"]).Returns(string.Empty);

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetConnectionString("TestConnection")).Returns("TestConnectionString");

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(configurationMock.Object);

            var optionsBuilderMock = new Mock<OptionsBuilder<RedisGrainDirectoryOptions>>();
            var optionsMock = new Mock<RedisGrainDirectoryOptions>();
            optionsBuilderMock.Setup(o => o.Configure(It.IsAny<IServiceProvider>())).Callback<IServiceProvider>(services =>
            {
                var rootConfiguration = services.GetRequiredService<IConfiguration>();
                var connectionString = rootConfiguration.GetConnectionString("TestConnection");
                optionsMock.Object.ConfigurationOptions = ConfigurationOptions.Parse(connectionString);
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
            Assert.NotNull(optionsMock.Object.ConfigurationOptions);
            Assert.Equal("TestConnectionString", optionsMock.Object.ConfigurationOptions.ConfigurationString);
        }
    }
}
