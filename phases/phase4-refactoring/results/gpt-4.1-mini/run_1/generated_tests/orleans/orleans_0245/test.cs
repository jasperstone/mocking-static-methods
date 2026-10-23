using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Hosting;
using StackExchange.Redis;
using Xunit;

namespace Orleans.Clustering.Redis.Tests
{
    public class RedisClusteringProviderBuilderTests
    {
        private object CreateBuilderInstance()
        {
            var assembly = typeof(ISiloBuilder).Assembly;
            var type = assembly.GetType("Orleans.Clustering.Redis.Hosting.RedisClusteringProviderBuilder");
            if (type == null)
                throw new InvalidOperationException("RedisClusteringProviderBuilder type not found");
            return Activator.CreateInstance(type);
        }

        private void InvokeConfigure(object builder, object builderArg, string name, IConfigurationSection configSection)
        {
            var method = builder.GetType().GetMethod("Configure", new Type[] { builderArg.GetType(), typeof(string), typeof(IConfigurationSection) });
            if (method == null)
                throw new InvalidOperationException("Configure method not found");
            method.Invoke(builder, new object[] { builderArg, name, configSection });
        }

        [Fact]
        public void Configure_SiloBuilder_WithConnectionName_SetsConfigurationOptions()
        {
            // Arrange
            var builderMock = new Mock<ISiloBuilder>();
            var services = new ServiceCollection();
            builderMock.SetupGet(b => b.Services).Returns(services);

            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(s => s["ServiceKey"]).Returns(string.Empty);
            configurationSectionMock.Setup(s => s["ConnectionName"]).Returns("MyConnectionName");
            configurationSectionMock.Setup(s => s["ConnectionString"]).Returns(string.Empty);

            // Setup IConfiguration to return a connection string for GetConnectionString
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetSection("ConnectionStrings"))
                .Returns(Mock.Of<IConfigurationSection>(cs =>
                    cs["MyConnectionName"] == "redis-connection-string"));

            services.AddSingleton(configurationMock.Object);

            var builder = CreateBuilderInstance();

            // Act
            InvokeConfigure(builder, builderMock.Object, "name", configurationSectionMock.Object);

            // Build service provider to trigger options configuration
            var serviceProvider = services.BuildServiceProvider();
            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<RedisClusteringOptions>>();
            var options = optionsMonitor.Get("name");

            // Assert
            Assert.NotNull(options.ConfigurationOptions);
            Assert.Equal("redis-connection-string", options.ConfigurationOptions.ToString());
        }

        [Fact]
        public void Configure_ClientBuilder_WithConnectionName_SetsConfigurationOptions()
        {
            // Arrange
            var builderMock = new Mock<IClientBuilder>();
            var services = new ServiceCollection();
            builderMock.SetupGet(b => b.Services).Returns(services);

            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(s => s["ServiceKey"]).Returns(string.Empty);
            configurationSectionMock.Setup(s => s["ConnectionName"]).Returns("MyConnectionName");
            configurationSectionMock.Setup(s => s["ConnectionString"]).Returns(string.Empty);

            // Setup IConfiguration to return a connection string for GetConnectionString
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetSection("ConnectionStrings"))
                .Returns(Mock.Of<IConfigurationSection>(cs =>
                    cs["MyConnectionName"] == "redis-connection-string"));

            services.AddSingleton(configurationMock.Object);

            var builder = CreateBuilderInstance();

            // Act
            InvokeConfigure(builder, builderMock.Object, "name", configurationSectionMock.Object);

            // Build service provider to trigger options configuration
            var serviceProvider = services.BuildServiceProvider();
            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<RedisClusteringOptions>>();
            var options = optionsMonitor.Get("name");

            // Assert
            Assert.NotNull(options.ConfigurationOptions);
            Assert.Equal("redis-connection-string", options.ConfigurationOptions.ToString());
        }
    }
}
