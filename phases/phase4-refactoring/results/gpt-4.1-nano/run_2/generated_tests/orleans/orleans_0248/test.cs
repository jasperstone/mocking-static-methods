using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.Providers;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Orleans.Tests
{
    public class RedisGrainDirectoryProviderBuilderTests
    {
        [Fact]
        public void Configure_Should_Call_GetConnectionString_When_ConnectionName_IsSet_And_ConnectionString_IsEmpty()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(s => s["ServiceKey"]).Returns((string)null);
            configurationSectionMock.Setup(s => s["ConnectionName"]).Returns("TestConnection");
            configurationSectionMock.Setup(s => s["ConnectionString"]).Returns((string)null);

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetConnectionString("TestConnection")).Returns("localhost:6379");

            var servicesMock = new Mock<IServiceProvider>();
            servicesMock.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(configurationMock.Object);

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configurationMock.Object);
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var builder = new Mock<ISiloBuilder>();
            builder.Setup(b => b.AddRedisGrainDirectory(It.IsAny<string>(), It.IsAny<Action<OptionsBuilder<RedisGrainDirectoryOptions>>>()))
                .Verifiable();

            var providerBuilder = new RedisGrainDirectoryProviderBuilder();

            // Call the method
            providerBuilder.Configure(builder.Object, "TestName", configurationSectionMock.Object);

            // Assert
            var connectionString = configurationMock.Object.GetConnectionString("TestConnection");
            Assert.Equal("localhost:6379", connectionString);
        }
    }
}
