using Xunit;
using Orleans.Clustering.Redis.Hosting;
using Microsoft.Extensions.Configuration;
using Orleans.Hosting;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Orleans.Clustering.Redis.Tests
{
    public class RedisClusteringProviderBuilderWrapper
    {
        private readonly RedisClusteringProviderBuilder _builder;

        public RedisClusteringProviderBuilderWrapper()
        {
            _builder = new RedisClusteringProviderBuilder();
        }

        public void Configure(ISiloBuilder builder, string name, IConfigurationSection configurationSection)
        {
            _builder.Configure(builder, name, configurationSection);
        }
    }

    public class RedisClusteringProviderBuilderTests
    {
        [Fact]
        public void Configure_WithConnectionName_ShouldSetConnectionString()
        {
            // Arrange
            var mockConfigurationSection = new Mock<IConfigurationSection>();
            mockConfigurationSection.Setup(x => x["ConnectionName"]).Returns("TestConnection");
            mockConfigurationSection.Setup(x => x["ConnectionString"]).Returns((string)null);

            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(x => x.GetConnectionString("TestConnection")).Returns("TestConnectionString");

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(x => x.GetService(typeof(IConfiguration))).Returns(mockConfiguration.Object);

            var mockServiceCollection = new Mock<IServiceCollection>();
            mockServiceCollection.Setup(x => x.BuildServiceProvider()).Returns(mockServiceProvider.Object);

            var mockSiloBuilder = new Mock<ISiloBuilder>();
            mockSiloBuilder.Setup(x => x.Services).Returns(mockServiceCollection.Object);

            var builder = new RedisClusteringProviderBuilderWrapper();

            // Act
            builder.Configure(mockSiloBuilder.Object, "Test", mockConfigurationSection.Object);

            // Assert
            mockConfiguration.Verify(x => x.GetConnectionString("TestConnection"), Times.Once);
        }
    }
}
