using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans;
using StackExchange.Redis;
using System.Threading.Tasks;
using Orleans.Configuration;

namespace Orleans.Tests
{
    public class RedisGrainDirectoryProviderBuilderTests
    {
        [Fact]
        public void Configure_Should_Call_GetConnectionString_When_ConnectionName_And_Empty_ConnectionString()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(c => c["ServiceKey"]).Returns<string>(null);
            configurationSectionMock.Setup(c => c["ConnectionName"]).Returns("TestConnection");
            configurationSectionMock.Setup(c => c["ConnectionString"]).Returns<string>(null);

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetConnectionString("TestConnection")).Returns("localhost:6379");

            var services = new ServiceCollection()
                .AddSingleton<IConfiguration>(configurationMock.Object)
                .BuildServiceProvider();

            var builderMock = new Mock<ISiloBuilder>();
            builderMock.Setup(b => b.AddRedisGrainDirectory(It.IsAny<string>(), It.IsAny<Action<OptionsBuilder<RedisGrainDirectoryOptions>>>()))
                .Callback<string, Action<OptionsBuilder<RedisGrainDirectoryOptions>>>((name, configure) =>
                {
                    var optionsBuilder = new OptionsBuilder<RedisGrainDirectoryOptions>();
                    // Invoke the configuration action to simulate configuration
                    configure(optionsBuilder);
                });

            var providerBuilder = new RedisGrainDirectoryProviderBuilder();

            // Act
            providerBuilder.Configure(builderMock.Object, "TestName", configurationSectionMock.Object);
        }
    }
}
