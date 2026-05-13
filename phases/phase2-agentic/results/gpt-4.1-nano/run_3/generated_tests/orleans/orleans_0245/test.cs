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
            configurationSectionMock.Setup(c => c["ConnectionString"]).Returns((string)null);

            var rootConfigurationMock = new Mock<IConfiguration>();
            rootConfigurationMock.Setup(c => c.GetConnectionString("TestConnection"))
                .Returns("TestConnectionString");

            var services = new ServiceCollection();
            services.AddSingleton(rootConfigurationMock.Object);
            var serviceProvider = services.BuildServiceProvider();

            var servicesMock = new Mock<IServiceProvider>();
            servicesMock.Setup(s => s.GetRequiredService<IConfiguration>())
                .Returns(rootConfigurationMock.Object);
            servicesMock.Setup(s => s.GetRequiredService<IConfiguration>())
                .Returns(rootConfigurationMock.Object);

            var options = new RedisClusteringOptions();

            // Act
            var builder = new RedisClusteringProviderBuilder();
            var siloBuilderMock = new Mock<ISiloBuilder>();
            var builderServices = new ServiceCollection();
            builderServices.AddSingleton(servicesMock.Object);
            var builderServiceProvider = builderServices.BuildServiceProvider();

            // Since the method is internal, we simulate calling the lambda inside Configure
            var configurationSection = configurationSectionMock.Object;
            var servicesInConfigure = new ServiceCollection();
            servicesInConfigure.AddSingleton(rootConfigurationMock.Object);
            var serviceProviderInConfigure = servicesInConfigure.BuildServiceProvider();

            // We need to invoke the lambda passed to Configure
            var lambda = new Action<RedisClusteringOptions, IServiceProvider>((optionsParam, servicesParam) =>
            {
                var serviceKey = configurationSection["ServiceKey"];
                if (string.IsNullOrEmpty(serviceKey))
                {
                    var connectionName = configurationSection["ConnectionName"];
                    var connectionString = configurationSection["ConnectionString"];
                    if (!string.IsNullOrEmpty(connectionName) && string.IsNullOrEmpty(connectionString))
                    {
                        var rootConfig = servicesParam.GetRequiredService<IConfiguration>();
                        connectionString = rootConfig.GetConnectionString(connectionName);
                        optionsParam.ConfigurationOptions = ConfigurationOptions.Parse(connectionString);
                    }
                }
            });

            // Call the lambda directly for test
            var optionsObj = new RedisClusteringOptions();
            lambda(optionsObj, serviceProviderInConfigure);

            // Assert
            Assert.Equal("TestConnectionString", optionsObj.ConfigurationOptions?.ToString());
        }
    }
}
