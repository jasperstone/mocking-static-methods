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

namespace Orleans.GrainDirectory.Redis.Tests.Hosting
{
    public class RedisGrainDirectoryProviderBuilderTests
    {
        private class TestSiloBuilder : ISiloBuilder
        {
            public Action<string, Action<OptionsBuilder<RedisGrainDirectoryOptions>>>? CapturedAction { get; private set; }

            public ISiloBuilder AddRedisGrainDirectory(string name, Action<OptionsBuilder<RedisGrainDirectoryOptions>> configureOptions)
            {
                CapturedAction = (name, configureOptions);
                return this;
            }

            // The remaining ISiloBuilder members are not required for these tests.
            public IServiceCollection Services => throw new NotImplementedException();
            public ISiloBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate) => throw new NotImplementedException();
            public ISiloBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate, bool catchExceptions) => throw new NotImplementedException();
            public ISiloBuilder ConfigureServices(Action<IServiceCollection> configureDelegate) => throw new NotImplementedException();
            public ISiloBuilder ConfigureServices(Action<IServiceCollection> configureDelegate, bool catchExceptions) => throw new NotImplementedException();
            public ISiloBuilder ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate) => throw new NotImplementedException();
            public ISiloBuilder ConfigureApplicationParts(Action<IApplicationPartManager> configure) => throw new NotImplementedException();
            public ISiloBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate) => throw new NotImplementedException();
            public ISiloBuilder UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory) => throw new NotImplementedException();
            public ISiloBuilder UseServiceProviderFactory<TContainerBuilder>(Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory) => throw new NotImplementedException();
        }

        private static IConfigurationSection CreateConfigurationSection(string? serviceKey, string? connectionName, string? connectionString)
        {
            var configMock = new Mock<IConfigurationSection>();
            configMock.Setup(c => c["ServiceKey"]).Returns(serviceKey);
            configMock.Setup(c => c["ConnectionName"]).Returns(connectionName);
            configMock.Setup(c => c["ConnectionString"]).Returns(connectionString);
            return configMock.Object;
        }

        [Fact]
        public void Configure_WhenConnectionNameProvidedAndConnectionStringMissing_UsesRootConfigurationConnectionString()
        {
            // Arrange
            var connectionName = "MyRedis";
            var resolvedConnectionString = "localhost:6379";
            var configurationSection = CreateConfigurationSection(serviceKey: null, connectionName: connectionName, connectionString: null);

            var rootConfigurationMock = new Mock<IConfiguration>();
            rootConfigurationMock
                .Setup(c => c.GetConnectionString(connectionName))
                .Returns(resolvedConnectionString);

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(rootConfigurationMock.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var optionsBuilder = new OptionsBuilder<RedisGrainDirectoryOptions>("TestOptions");
            optionsBuilder.Configure<IServiceProvider>((options, _) => { });

            var siloBuilder = new TestSiloBuilder();
            var builder = new RedisGrainDirectoryProviderBuilder();

            // Act
            builder.Configure(siloBuilder, "RedisDirectory", configurationSection);
            Assert.NotNull(siloBuilder.CapturedAction);

            var options = new RedisGrainDirectoryOptions();
            squaredAddConfigure(siloBuilder.CapturedAction.Value, serviceProvider, options);

            // Assert
            rootConfigurationMock.Verify(c => c.GetConnectionString(connectionName), Times.Once);
            Assert.NotNull(options.ConfigurationOptions);
            Assert.Equal(resolvedConnectionString, options.ConfigurationOptions.EndPoints[0].ToString());
        }

        [Fact]
        public void Configure_WhenConnectionStringProvided_DoesNotQueryRootConfiguration()
        {
            // Arrange
            var connectionName = "MyRedis";
            var connectionString = "localhost:6380";
            var configurationSection = CreateConfigurationSection(serviceKey: null, connectionName: connectionName, connectionString: connectionString);

            var rootConfigurationMock = new Mock<IConfiguration>();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(rootConfigurationMock.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var optionsBuilder = new OptionsBuilder<RedisGrainDirectoryOptions>("TestOptions");
            optionsBuilder.Configure<IServiceProvider>((options, _) => { });

            var siloBuilder = new TestSiloBuilder();
            var builder = new RedisGrainDirectoryProviderBuilder();

            // Act
            builder.Configure(siloBuilder, "RedisDirectory", configurationSection);
            Assert.NotNull(siloBuilder.CapturedAction);

            var options = new RedisGrainDirectoryOptions();
            squaredAddConfigure(siloBuilder.CapturedAction.Value, serviceProvider, options);

            // Assert
            rootConfigurationMock.Verify(c => c.GetConnectionString(It.IsAny<string>()), Times.Never);
            Assert.NotNull(options.ConfigurationOptions);
            Assert.Equal(connectionString, options.ConfigurationOptions.EndPoints[0].ToString());
        }

        private static void squaredAddConfigure(
            (string Name, Action<OptionsBuilder<RedisGrainDirectoryOptions>> Configure) capturedAction,
            IServiceProvider serviceProvider,
            RedisGrainDirectoryOptions options)
        {
            var optionsBuilder = new OptionsBuilder<RedisGrainDirectoryOptions>(capturedAction.Name);
            optionsBuilder.Configure<IServiceProvider>((opts, svc) =>
            {
                opts.ConfigurationOptions = options.ConfigurationOptions;
                opts.CreateMultiplexer = options.CreateMultiplexer;
            });

            capturedAction.Configure(optionsBuilder);

            foreach (var configureAction in optionsBuilder.Actions)
            {
                configureAction(options, serviceProvider);
            }
        }
    }
}
