using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Hosting;
using StackExchange.Redis;
using Xunit;

namespace Orleans.GrainDirectory.Redis.Tests
{
    public class RedisGrainDirectoryProviderBuilderTests
    {
        private class TestSiloBuilder : ISiloBuilder
        {
            public string AddedName;
            public Action<OptionsBuilder<RedisGrainDirectoryOptions>> AddedConfigure;

            public IServiceCollection Services { get; } = new ServiceCollection();

            // Minimal implementation to satisfy ISiloBuilder interface
            public ISiloBuilder AddRedisGrainDirectory(string name, Action<OptionsBuilder<RedisGrainDirectoryOptions>> configureOptions)
            {
                AddedName = name;
                AddedConfigure = configureOptions;
                return this;
            }

            // Stub properties and methods to satisfy interface
            public IConfiguration Configuration => throw new NotImplementedException();
            public IServiceProvider BuildServiceProvider() => throw new NotImplementedException();
            public ISiloBuilder ConfigureServices(Action<IServiceCollection> configureServices) => throw new NotImplementedException();
            public ISiloBuilder ConfigureAppConfiguration(Action<ISiloBuilderContext, IConfigurationBuilder> configureDelegate) => throw new NotImplementedException();
            public ISiloBuilder ConfigureLogging(Action<ISiloBuilderContext, ILoggingBuilder> configureLogging) => throw new NotImplementedException();
            public ISiloBuilder UseDashboard(Action<DashboardOptions> configureOptions) => throw new NotImplementedException();
            public ISiloBuilder UseDashboard() => throw new NotImplementedException();
            public ISiloBuilder UseStaticFiles() => throw new NotImplementedException();
            public ISiloBuilder UseStaticFiles(Action<StaticFileOptions> configureOptions) => throw new NotImplementedException();
            public ISiloBuilder UseStaticFiles(string requestPath) => throw new NotImplementedException();
            public ISiloBuilder UseStaticFiles(string requestPath, Action<StaticFileOptions> configureOptions) => throw new NotImplementedException();
            public ISiloBuilder UseStaticFiles(string requestPath, string fileProvider) => throw new NotImplementedException();
            public ISiloBuilder UseStaticFiles(string requestPath, string fileProvider, Action<StaticFileOptions> configureOptions) => throw new NotImplementedException();
        }

        [Fact]
        public void Configure_WithConnectionNameAndNoConnectionString_UsesGetConnectionString()
        {
            // Arrange
            var builder = new TestSiloBuilder();

            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.SetupGet(c => c["ServiceKey"]).Returns(string.Empty);
            configurationSectionMock.SetupGet(c => c["ConnectionName"]).Returns("MyConnectionName");
            configurationSectionMock.SetupGet(c => c["ConnectionString"]).Returns(string.Empty);

            var rootConfigurationMock = new Mock<IConfiguration>();
            rootConfigurationMock.Setup(c => c.GetConnectionString("MyConnectionName")).Returns("my-connection-string");

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IConfiguration))).Returns(rootConfigurationMock.Object);

            var serviceProvider = serviceProviderMock.Object;

            var providerBuilder = new RedisGrainDirectoryProviderBuilder();

            // Act
            providerBuilder.Configure(builder, "testName", configurationSectionMock.Object);

            // The Configure method should have called AddRedisGrainDirectory on builder
            Assert.Equal("testName", builder.AddedName);
            Assert.NotNull(builder.AddedConfigure);

            // Now invoke the options configuration delegate to simulate the IServiceProvider usage
            var services = new ServiceCollection();
            var optionsBuilder = new OptionsBuilder<RedisGrainDirectoryOptions>(services);
            builder.AddedConfigure(optionsBuilder);

            var options = new RedisGrainDirectoryOptions();

            // The Configure<IServiceProvider> extension method adds an IConfigureOptions<TOptions> service
            // We simulate calling the configure delegate by invoking the delegate directly from the optionsBuilder
            var configureDelegate = optionsBuilder.OptionsActions[0];
            configureDelegate(options, serviceProvider);

            // Assert
            Assert.NotNull(options.ConfigurationOptions);
            Assert.Equal("my-connection-string", options.ConfigurationOptions.ToString());
        }
    }
}
