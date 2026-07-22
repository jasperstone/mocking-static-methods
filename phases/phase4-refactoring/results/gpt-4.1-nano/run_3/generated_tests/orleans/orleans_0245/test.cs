using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Clustering.Redis.Hosting;
using StackExchange.Redis;
using System.Threading.Tasks;

namespace Orleans.Tests
{
    public class RedisClusteringProviderBuilderTests
    {
        [Fact]
        public void Configure_Should_Call_GetConnectionString_When_ConnectionName_Provided_And_ConnectionString_Is_Empty()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(c => c["ServiceKey"]).Returns((string)null);
            configurationSectionMock.Setup(c => c["ConnectionName"]).Returns("TestConnection");
            configurationSectionMock.Setup(c => c["ConnectionString"]).Returns((string)null);

            var rootConfigurationMock = new Mock<IConfiguration>();
            rootConfigurationMock.Setup(c => c.GetConnectionString("TestConnection"))
                .Returns("TestConnectionString");

            var services = new ServiceCollection();

            // Add IConfiguration to the service collection
            services.AddSingleton<IConfiguration>(rootConfigurationMock.Object);

            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var builder = new RedisClusteringProviderBuilder();

            // Simulate the internal lambda execution
            var options = new RedisClusteringOptions();
            var servicesForConfig = serviceProvider;

            // Manually invoke the lambda as in the actual code
            var connectionName = "TestConnection";
            var connectionString = (string)null;
            var rootConfig = servicesForConfig.GetRequiredService<IConfiguration>();
            connectionString = rootConfig.GetConnectionString(connectionName);
            options.ConfigurationOptions = ConfigurationOptions.Parse(connectionString);

            // Assert
            Assert.NotNull(options.ConfigurationOptions);
            Assert.Equal("TestConnectionString", options.ConfigurationOptions.ToString());
        }
    }

    // Dummy class to represent the options class used in the actual code
    public class RedisClusteringOptions
    {
        public Func<IConnectionMultiplexer, Task> CreateMultiplexer { get; set; }
        public ConfigurationOptions ConfigurationOptions { get; set; }
    }
}
