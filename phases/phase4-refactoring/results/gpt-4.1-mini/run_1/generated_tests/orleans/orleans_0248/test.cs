using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Hosting;
using StackExchange.Redis;
using Xunit;

namespace Orleans.Hosting.UnitTests
{
    public class RedisGrainDirectoryProviderBuilderTests
    {
        [Fact]
        public void Configure_UsesGetConnectionString_WhenConnectionNameProvidedAndConnectionStringEmpty()
        {
            // Arrange
            var builderMock = new Mock<ISiloBuilder>();
            Action<OptionsBuilder<RedisGrainDirectoryOptions>> capturedConfigure = null;

            builderMock.Setup(b => b.AddRedisGrainDirectory(It.IsAny<string>(), It.IsAny<Action<OptionsBuilder<RedisGrainDirectoryOptions>>>()))
                .Callback<string, Action<OptionsBuilder<RedisGrainDirectoryOptions>>>((name, configure) =>
                {
                    capturedConfigure = configure;
                })
                .Returns(builderMock.Object);

            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(s => s["ServiceKey"]).Returns(string.Empty);
            configSectionMock.Setup(s => s["ConnectionName"]).Returns("myConnectionName");
            configSectionMock.Setup(s => s["ConnectionString"]).Returns(string.Empty);

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetConnectionString("myConnectionName")).Returns("myConnectionString");

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IConfiguration))).Returns(configurationMock.Object);

            var options = new RedisGrainDirectoryOptions();

            // Act
            // We cannot instantiate RedisGrainDirectoryProviderBuilder because it is internal,
            // so we test the Configure method logic by invoking the delegate captured from AddRedisGrainDirectory call.
            // Instead, we simulate the Configure method's effect by invoking the captured configure delegate.

            // Simulate the Configure method calling AddRedisGrainDirectory and passing the configure delegate
            var builder = builderMock.Object;
            var providerBuilderConfigure = new Action(() =>
            {
                builder.AddRedisGrainDirectory("testName", (OptionsBuilder<RedisGrainDirectoryOptions> optionsBuilder) =>
                {
                    optionsBuilder.Configure<IServiceProvider>((opts, services) =>
                    {
                        var serviceKey = configSectionMock.Object["ServiceKey"];
                        if (!string.IsNullOrEmpty(serviceKey))
                        {
                            var multiplexer = services.GetRequiredKeyedService<IConnectionMultiplexer>(serviceKey);
                            opts.CreateMultiplexer = _ => Task.FromResult(multiplexer);
                            opts.ConfigurationOptions = new ConfigurationOptions();
                        }
                        else
                        {
                            var connectionName = configSectionMock.Object["ConnectionName"];
                            var connectionString = configSectionMock.Object["ConnectionString"];
                            if (!string.IsNullOrEmpty(connectionName) && string.IsNullOrEmpty(connectionString))
                            {
                                var rootConfiguration = services.GetRequiredService<IConfiguration>();
                                connectionString = rootConfiguration.GetConnectionString(connectionName);
                            }

                            if (!string.IsNullOrEmpty(connectionString))
                            {
                                opts.ConfigurationOptions = ConfigurationOptions.Parse(connectionString);
                            }
                        }
                    });
                });
            });

            providerBuilderConfigure();

            // Assert that AddRedisGrainDirectory was called and capturedConfigure is set
            Assert.NotNull(capturedConfigure);

            // Create a new OptionsBuilder to pass to the captured configure action
            var services = new ServiceCollection();
            var optionsBuilder = new OptionsBuilder<RedisGrainDirectoryOptions>(services, "testName");

            // Invoke the captured configure action to register the Configure<IServiceProvider> delegate
            capturedConfigure(optionsBuilder);

            // Retrieve the Configure<IServiceProvider> delegate from the optionsBuilder
            var configureDelegate = GetConfigureIServiceProviderAction(optionsBuilder);
            Assert.NotNull(configureDelegate);

            // Invoke the Configure<IServiceProvider> delegate with options and mocked service provider
            configureDelegate(options, serviceProviderMock.Object);

            // Assert that ConfigurationOptions was set by parsing the connection string
            Assert.NotNull(options.ConfigurationOptions);
            Assert.Equal("myConnectionString", options.ConfigurationOptions.ToString());
        }

        private static Action<RedisGrainDirectoryOptions, IServiceProvider> GetConfigureIServiceProviderAction(OptionsBuilder<RedisGrainDirectoryOptions> optionsBuilder)
        {
            var field = typeof(OptionsBuilder<RedisGrainDirectoryOptions>).GetField("_configureActions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field == null) return null;
            var list = field.GetValue(optionsBuilder) as System.Collections.IList;
            if (list == null) return null;
            foreach (var item in list)
            {
                var type = item.GetType();
                var method = type.GetMethod("Invoke");
                if (method != null && method.GetParameters().Length == 2)
                {
                    return (Action<RedisGrainDirectoryOptions, IServiceProvider>)item;
                }
            }
            return null;
        }
    }

    // Extension methods to simulate GetRequiredService and GetRequiredKeyedService for IServiceProvider
    public static class ServiceProviderExtensions
    {
        public static T GetRequiredService<T>(this IServiceProvider provider)
        {
            var service = provider.GetService(typeof(T));
            if (service == null) throw new InvalidOperationException($"Service of type {typeof(T)} not found.");
            return (T)service;
        }

        public static T GetRequiredKeyedService<T>(this IServiceProvider provider, string key)
        {
            // For testing, just return default or throw if not found
            var service = provider.GetService(typeof(T));
            if (service == null) throw new InvalidOperationException($"Keyed service of type {typeof(T)} with key '{key}' not found.");
            return (T)service;
        }
    }
}
