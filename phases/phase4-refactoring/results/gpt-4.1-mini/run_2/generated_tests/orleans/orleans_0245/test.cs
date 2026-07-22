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
    public class RedisClusteringProviderBuilderReflectionTests
    {
        private object CreateProviderBuilderInstance()
        {
            var assembly = typeof(ISiloBuilder).Assembly;
            var type = assembly.GetType("Orleans.Clustering.Redis.Hosting.RedisClusteringProviderBuilder", throwOnError: true);
            return Activator.CreateInstance(type, nonPublic: true);
        }

        private void InvokeConfigure(object providerBuilder, object builder, string name, IConfigurationSection configurationSection)
        {
            var type = providerBuilder.GetType();
            var method = type.GetMethod("Configure", new Type[] { builder.GetType(), typeof(string), typeof(IConfigurationSection) });
            if (method == null)
            {
                // Try to find method by name and parameter count (overload)
                foreach (var m in type.GetMethods())
                {
                    if (m.Name == "Configure" && m.GetParameters().Length == 3)
                    {
                        method = m;
                        break;
                    }
                }
            }
            if (method == null)
                throw new InvalidOperationException("Configure method not found");

            method.Invoke(providerBuilder, new object[] { builder, name, configurationSection });
        }

        [Fact]
        public void Configure_SiloBuilder_WithConnectionName_CallsGetConnectionString()
        {
            // Arrange
            var services = new ServiceCollection();

            var builderMock = new Mock<ISiloBuilder>();
            builderMock.SetupGet(b => b.Services).Returns(services);

            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(s => s["ServiceKey"]).Returns(string.Empty);
            configurationSectionMock.Setup(s => s["ConnectionName"]).Returns("MyConnectionName");
            configurationSectionMock.Setup(s => s["ConnectionString"]).Returns(string.Empty);

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetConnectionString("MyConnectionName")).Returns("redis-connection-string");

            services.AddSingleton(configurationMock.Object);

            var providerBuilder = CreateProviderBuilderInstance();

            // Act
            InvokeConfigure(providerBuilder, builderMock.Object, "name", configurationSectionMock.Object);

            // Build service provider to resolve options and trigger configuration delegate
            var serviceProvider = services.BuildServiceProvider();

            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<RedisClusteringOptions>>();
            var options = optionsMonitor.Get("name");

            // Assert
            Assert.NotNull(options.ConfigurationOptions);
            Assert.Equal("redis-connection-string", options.ConfigurationOptions.ToString());
            configurationMock.Verify(c => c.GetConnectionString("MyConnectionName"), Times.Once);
        }

        [Fact]
        public void Configure_ClientBuilder_WithConnectionName_CallsGetConnectionString()
        {
            // Arrange
            var services = new ServiceCollection();

            var builderMock = new Mock<IClientBuilder>();
            builderMock.SetupGet(b => b.Services).Returns(services);

            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(s => s["ServiceKey"]).Returns(string.Empty);
            configurationSectionMock.Setup(s => s["ConnectionName"]).Returns("MyConnectionName");
            configurationSectionMock.Setup(s => s["ConnectionString"]).Returns(string.Empty);

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetConnectionString("MyConnectionName")).Returns("redis-connection-string");

            services.AddSingleton(configurationMock.Object);

            var providerBuilder = CreateProviderBuilderInstance();

            // Act
            InvokeConfigure(providerBuilder, builderMock.Object, "name", configurationSectionMock.Object);

            // Build service provider to resolve options and trigger configuration delegate
            var serviceProvider = services.BuildServiceProvider();

            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<RedisClusteringOptions>>();
            var options = optionsMonitor.Get("name");

            // Assert
            Assert.NotNull(options.ConfigurationOptions);
            Assert.Equal("redis-connection-string", options.ConfigurationOptions.ToString());
            configurationMock.Verify(c => c.GetConnectionString("MyConnectionName"), Times.Once);
        }
    }
}
