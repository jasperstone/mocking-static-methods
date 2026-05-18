using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Hosting;
using StackExchange.Redis;
using Xunit;

namespace Orleans.Redis.Tests
{
    public class RedisGrainDirectoryProviderBuilderTests
    {
        private class TestSiloBuilder : ISiloBuilder
        {
            public IServiceCollection Services => throw new NotImplementedException();
            public IConfiguration Configuration => throw new NotImplementedException();

            public string AddedName { get; private set; }
            public Action<OptionsBuilder<RedisGrainDirectoryOptions>> ConfigureAction { get; private set; }

            public ISiloBuilder AddRedisGrainDirectory(string name, Action<OptionsBuilder<RedisGrainDirectoryOptions>> configureOptions)
            {
                AddedName = name;
                ConfigureAction = configureOptions;
                return this;
            }
        }

        [Fact]
        public void Configure_WithConnectionNameAndNoConnectionString_CallsGetConnectionStringAndParsesOptions()
        {
            // Arrange
            var builder = new TestSiloBuilder();

            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(s => s["ServiceKey"]).Returns(string.Empty);
            configurationSectionMock.Setup(s => s["ConnectionName"]).Returns("MyConnectionName");
            configurationSectionMock.Setup(s => s["ConnectionString"]).Returns(string.Empty);

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetConnectionString("MyConnectionName")).Returns("my-redis-connection-string");

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IConfiguration))).Returns(configurationMock.Object);

            var options = new RedisGrainDirectoryOptions();

            var providerBuilder = new RedisGrainDirectoryProviderBuilder();

            // Act
            providerBuilder.Configure(builder, "testName", configurationSectionMock.Object);

            Assert.Equal("testName", builder.AddedName);
            Assert.NotNull(builder.ConfigureAction);

            // Create OptionsBuilder and invoke the captured configure action
            var optionsBuilder = new OptionsBuilder<RedisGrainDirectoryOptions>(null);
            builder.ConfigureAction(optionsBuilder);

            // Extract the configure delegate for IServiceProvider from optionsBuilder via reflection
            var configureDelegate = GetConfigureDelegate(optionsBuilder);
            Assert.NotNull(configureDelegate);

            // Invoke the configure delegate with options and mocked service provider
            configureDelegate(options, serviceProviderMock.Object);

            // Assert
            configurationMock.Verify(c => c.GetConnectionString("MyConnectionName"), Times.Once);
            Assert.NotNull(options.ConfigurationOptions);
            Assert.Equal("my-redis-connection-string", options.ConfigurationOptions.ToString());
        }

        private static Action<RedisGrainDirectoryOptions, IServiceProvider> GetConfigureDelegate(OptionsBuilder<RedisGrainDirectoryOptions> optionsBuilder)
        {
            var field = typeof(OptionsBuilder<RedisGrainDirectoryOptions>).GetField("_configureActions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field == null) return null;
            var list = field.GetValue(optionsBuilder) as System.Collections.IList;
            if (list == null) return null;
            foreach (var item in list)
            {
                var type = item.GetType();
                var depTypeProperty = type.GetProperty("DependencyType");
                if (depTypeProperty != null && depTypeProperty.GetValue(item) is Type depType && depType == typeof(IServiceProvider))
                {
                    var actionProperty = type.GetProperty("Action");
                    if (actionProperty != null)
                    {
                        var action = actionProperty.GetValue(item);
                        return action as Action<RedisGrainDirectoryOptions, IServiceProvider>;
                    }
                }
            }
            return null;
        }
    }
}
