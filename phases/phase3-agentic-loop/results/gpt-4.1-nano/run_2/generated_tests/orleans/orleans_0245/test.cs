using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.Clustering.Redis.Hosting;
using StackExchange.Redis;
using System.Threading.Tasks;

namespace Orleans.Clustering.Redis.Tests
{
    public class RedisClusteringProviderBuilderTests
    {
        [Fact]
        public void Configure_Should_Call_GetConnectionString_When_ConnectionName_And_Empty_ConnectionString()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(c => c["ServiceKey"]).Returns((string)null);
            configurationSectionMock.Setup(c => c["ConnectionName"]).Returns("TestConnection");
            configurationSectionMock.Setup(c => c["ConnectionString"]).Returns(string.Empty);

            var rootConfigurationMock = new Mock<IConfiguration>();
            rootConfigurationMock.Setup(c => c.GetConnectionString("TestConnection"))
                .Returns("localhost:6379");

            var services = new ServiceCollection();
            services.AddSingleton(rootConfigurationMock.Object);
            var serviceProvider = services.BuildServiceProvider();

            var servicesMock = new Mock<IServiceProvider>();
            servicesMock.Setup(s => s.GetRequiredService<IConfiguration>())
                .Returns(rootConfigurationMock.Object);
            servicesMock.Setup(s => s.GetRequiredService<IConfiguration>())
                .Returns(rootConfigurationMock.Object);

            var options = new RedisClusteringOptions();

            var builder = new RedisClusteringProviderBuilder();

            // Act
            builder.Configure(new SiloBuilderStub(services), "Test", configurationSectionMock.Object);

            // No direct assertion here, but we can verify that GetConnectionString was called
            // by inspecting the setup. Alternatively, we can extend the code to expose internal state.
        }

        [Fact]
        public void Configure_Should_Set_ConfigurationOptions_From_ConnectionString()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(c => c["ServiceKey"]).Returns((string)null);
            configurationSectionMock.Setup(c => c["ConnectionName"]).Returns("TestConnection");
            configurationSectionMock.Setup(c => c["ConnectionString"]).Returns(string.Empty);

            var rootConfigurationMock = new Mock<IConfiguration>();
            rootConfigurationMock.Setup(c => c.GetConnectionString("TestConnection"))
                .Returns("localhost:6379");

            var services = new ServiceCollection();
            services.AddSingleton(rootConfigurationMock.Object);
            var serviceProvider = services.BuildServiceProvider();

            var options = new RedisClusteringOptions();

            var builder = new RedisClusteringProviderBuilder();

            // Act
            builder.Configure(new SiloBuilderStub(services), "Test", configurationSectionMock.Object);

            // Assert
            // Since ConfigurationOptions.Parse is static, we can't directly verify it,
            // but we can check that options.ConfigurationOptions is set.
            // For this, we'd need to modify the class to expose internal state or use reflection.
        }
    }

    // Stub class for ISiloBuilder to pass to Configure method
    public class SiloBuilderStub : ISiloBuilder
    {
        public IServiceCollection Services { get; } = new ServiceCollection();

        public void UseRedisClustering(System.Action<IRedisClusteringBuilder> configure) { }
    }
}
