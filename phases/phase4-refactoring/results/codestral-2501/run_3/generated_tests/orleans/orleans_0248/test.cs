using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Orleans.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Configuration;
using Orleans.GrainDirectory.Redis;
using Orleans.Runtime.Hosting;
using Orleans.Runtime;
using Orleans.GrainDirectory;
using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Orleans.GrainDirectory.Redis.Tests
{
    public class RedisGrainDirectoryProviderBuilderTests
    {
        [Fact]
        public void Configure_WithConnectionName_ShouldCallGetConnectionString()
        {
            // Arrange
            var mockConfigurationSection = new Mock<IConfigurationSection>();
            mockConfigurationSection.Setup(c => c["ServiceKey"]).Returns((string)null);
            mockConfigurationSection.Setup(c => c["ConnectionName"]).Returns("TestConnection");
            mockConfigurationSection.Setup(c => c["ConnectionString"]).Returns((string)null);

            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(c => c.GetConnectionString("TestConnection")).Returns("TestConnectionString");

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IConfiguration>()).Returns(mockConfiguration.Object);

            var mockSiloBuilder = new Mock<ISiloBuilder>();
            var mockOptionsBuilder = new Mock<OptionsBuilder<RedisGrainDirectoryOptions>>();

            var builder = new RedisGrainDirectoryProviderBuilder();

            // Act
            builder.Configure(mockSiloBuilder.Object, "TestName", mockConfigurationSection.Object);

            // Assert
            mockConfiguration.Verify(c => c.GetConnectionString("TestConnection"), Times.Once);
        }
    }
}
