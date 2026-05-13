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

            var multiplexerMock = new Mock<IConnectionMultiplexer>();

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredKeyedService<IConnectionMultiplexer>("myServiceKey"))
                .Returns(multiplexerMock.Object);

            // Act
            var builder = new RedisGrainDirectoryProviderBuilder();
            builder.Configure(builderMock.Object, "name", configurationSectionMock.Object);

            // Now invoke the Configure<IServiceProvider> delegate to test the inner logic
            var options = new RedisGrainDirectoryOptions();
            var services = new TestServiceProvider(serviceProviderMock.Object);

            // The Configure delegate is the last registered on capturedOptionsBuilder
            var configureDelegate = capturedOptionsBuilder.OptionsActions[0];
            configureDelegate(options, services);

            // Assert
            Assert.NotNull(options.CreateMultiplexer);
            var task = options.CreateMultiplexer(null);
            Assert.Same(multiplexerMock.Object, task.Result);
            Assert.NotNull(options.ConfigurationOptions);
        }

        [Fact]
        public void Configure_WithConnectionNameAndNoConnectionString_UsesGetConnectionString()
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
            configurationSectionMock.SetupGet(c => c["ServiceKey"]).Returns((string)null);
            configurationSectionMock.SetupGet(c => c["ConnectionName"]).Returns("myConnectionName");
            configurationSectionMock.SetupGet(c => c["ConnectionString"]).Returns(string.Empty);

            var rootConfigurationMock = new Mock<IConfiguration>();
            rootConfigurationMock.Setup(c => c.GetConnectionString("myConnectionName")).Returns("myConnectionString");

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IConfiguration>())
                .Returns(rootConfigurationMock.Object);

            var services = new TestServiceProvider(serviceProviderMock.Object);

            // Act
            var builder = new RedisGrainDirectoryProviderBuilder();
            builder.Configure(builderMock.Object, "name", configurationSectionMock.Object);

            var options = new RedisGrainDirectoryOptions();

            var configureDelegate = capturedOptionsBuilder.OptionsActions[0];
            configureDelegate(options, services);

            // Assert
            Assert.NotNull(options.ConfigurationOptions);
            Assert.Equal("myConnectionString", options.ConfigurationOptions.ToString());
        }

        [Fact]
        public void Configure_WithConnectionString_ParsesConfigurationOptions()
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
            configurationSectionMock.SetupGet(c => c["ServiceKey"]).Returns((string)null);
            configurationSectionMock.SetupGet(c => c["ConnectionName"]).Returns(string.Empty);
            configurationSectionMock.SetupGet(c => c["ConnectionString"]).Returns("myConnectionString");

            var serviceProviderMock = new Mock<IServiceProvider>();

            var services = new TestServiceProvider(serviceProviderMock.Object);

            // Act
            var builder = new RedisGrainDirectoryProviderBuilder();
            builder.Configure(builderMock.Object, "name", configurationSectionMock.Object);

            var options = new RedisGrainDirectoryOptions();

            var configureDelegate = capturedOptionsBuilder.OptionsActions[0];
            configureDelegate(options, services);

            // Assert
            Assert.NotNull(options.ConfigurationOptions);
            Assert.Equal("myConnectionString", options.ConfigurationOptions.ToString());
        }

        private class TestServiceProvider : IServiceProvider
        {
            private readonly IServiceProvider _inner;

            public TestServiceProvider(IServiceProvider inner)
            {
                _inner = inner;
            }

            public object GetService(Type serviceType)
            {
                // Support GetRequiredService<T> extension method calls
                if (serviceType == typeof(IConfiguration))
                {
                    return _inner.GetService(serviceType);
                }

                // Support GetRequiredKeyedService<T> extension method calls
                if (serviceType == typeof(IConnectionMultiplexer))
                {
                    return _inner.GetService(serviceType);
                }

                return null;
            }
        }
    }
}
