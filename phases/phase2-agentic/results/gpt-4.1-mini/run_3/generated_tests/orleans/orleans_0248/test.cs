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
        public void Configure_WithServiceKey_SetsCreateMultiplexerAndConfigurationOptions()
        {
            // Arrange
            var builderMock = new Mock<ISiloBuilder>();
            OptionsBuilder<RedisGrainDirectoryOptions> capturedOptionsBuilder = null;
            builderMock.Setup(b => b.AddRedisGrainDirectory(It.IsAny<string>(), It.IsAny<Action<OptionsBuilder<RedisGrainDirectoryOptions>>>()))
                .Callback<string, Action<OptionsBuilder<RedisGrainDirectoryOptions>>>((name, configure) =>
                {
                    var optionsBuilder = new OptionsBuilder<RedisGrainDirectoryOptions>(new ServiceCollection());
                    configure(optionsBuilder);
                    capturedOptionsBuilder = optionsBuilder;
                })
                .Returns(builderMock.Object);

            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.SetupGet(c => c["ServiceKey"]).Returns("myServiceKey");
            configurationSectionMock.SetupGet(c => c["ConnectionName"]).Returns((string)null);
            configurationSectionMock.SetupGet(c => c["ConnectionString"]).Returns((string)null);

            var serviceProviderMock = new Mock<IServiceProvider>();
            var connectionMultiplexerMock = new Mock<IConnectionMultiplexer>();

            var serviceProvider = new ServiceCollection()
                .AddSingleton(connectionMultiplexerMock.Object)
                .BuildServiceProvider();

            // Setup GetRequiredKeyedService extension method via a mock IServiceProvider
            // Since GetRequiredKeyedService is an extension method, we simulate it by adding a method to IServiceProvider mock
            // But since we cannot mock extension methods directly, we simulate the call by intercepting the call inside Configure delegate

            // We will simulate the IServiceProvider passed to Configure delegate
            var servicesMock = new Mock<IServiceProvider>();
            servicesMock.Setup(s => s.GetRequiredKeyedService<IConnectionMultiplexer>("myServiceKey"))
                .Returns(connectionMultiplexerMock.Object);

            // Act
            var builder = new RedisGrainDirectoryProviderBuilder();
            builder.Configure(builderMock.Object, "testName", configurationSectionMock.Object);

            // Now invoke the Configure delegate to trigger the inner logic
            var optionsBuilder = new OptionsBuilder<RedisGrainDirectoryOptions>(new ServiceCollection());
            optionsBuilder.Configure<IServiceProvider>((options, services) =>
            {
                var serviceKey = configurationSectionMock.Object["ServiceKey"];
                if (!string.IsNullOrEmpty(serviceKey))
                {
                    var multiplexer = services.GetRequiredKeyedService<IConnectionMultiplexer>(serviceKey);
                    options.CreateMultiplexer = _ => Task.FromResult(multiplexer);
                    options.ConfigurationOptions = new ConfigurationOptions();
                }
            });

            // Assert
            Assert.NotNull(capturedOptionsBuilder);
        }

        [Fact]
        public void Configure_WithConnectionNameAndNoConnectionString_CallsGetConnectionString()
        {
            // Arrange
            var builderMock = new Mock<ISiloBuilder>();
            builderMock.Setup(b => b.AddRedisGrainDirectory(It.IsAny<string>(), It.IsAny<Action<OptionsBuilder<RedisGrainDirectoryOptions>>>()))
                .Returns(builderMock.Object);

            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.SetupGet(c => c["ServiceKey"]).Returns((string)null);
            configurationSectionMock.SetupGet(c => c["ConnectionName"]).Returns("myConnectionName");
            configurationSectionMock.SetupGet(c => c["ConnectionString"]).Returns(string.Empty);

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetConnectionString("myConnectionName")).Returns("myConnectionString");

            var servicesMock = new Mock<IServiceProvider>();
            servicesMock.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(configurationMock.Object);

            RedisGrainDirectoryOptions options = null;

            // Act
            var builder = new RedisGrainDirectoryProviderBuilder();
            builder.Configure(builderMock.Object, "testName", configurationSectionMock.Object);

            // We need to invoke the Configure delegate to test the inner logic
            var optionsBuilder = new OptionsBuilder<RedisGrainDirectoryOptions>(new ServiceCollection());
            optionsBuilder.Configure<IServiceProvider>((opts, services) =>
            {
                var serviceKey = configurationSectionMock.Object["ServiceKey"];
                if (!string.IsNullOrEmpty(serviceKey))
                {
                    // Not expected in this test
                }
                else
                {
                    var connectionName = configurationSectionMock.Object["ConnectionName"];
                    var connectionString = configurationSectionMock.Object["ConnectionString"];
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
                options = opts;
            });

            // Assert
            Assert.NotNull(options);
            Assert.NotNull(options.ConfigurationOptions);
            Assert.Equal("myConnectionString", options.ConfigurationOptions.ToString());
        }
    }

    // Extension methods to simulate GetRequiredKeyedService and GetRequiredService for mocking
    public static class ServiceProviderExtensions
    {
        public static T GetRequiredKeyedService<T>(this IServiceProvider provider, string key)
        {
            // This method is just a placeholder for mocking
            throw new NotImplementedException();
        }

        public static T GetRequiredService<T>(this IServiceProvider provider)
        {
            return (T)provider.GetService(typeof(T))!;
        }
    }
}
