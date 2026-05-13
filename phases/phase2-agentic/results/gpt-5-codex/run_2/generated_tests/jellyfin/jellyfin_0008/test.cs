using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Tests.Emby.Server.Implementations
{
    public class ApplicationHostTests
    {
        private sealed class TestApplicationHost : Emby.Server.Implementations.ApplicationHost
        {
            public TestApplicationHost(
                IServerApplicationPaths paths,
                ILoggerFactory loggerFactory,
                IStartupOptions options,
                IConfiguration configuration,
                IServiceProvider serviceProvider,
                ILogger<ApplicationHost> logger,
                PluginManager pluginManager)
                : base(paths, loggerFactory, options, configuration)
            {
                ServiceProvider = serviceProvider;
                Logger = logger;
                PluginManager = pluginManager;
            }

            protected override string Name => "TestHost";

            protected override Version Version => new Version(1, 0);

            public new object CreateInstanceSafe(Type type) => base.CreateInstanceSafe(type);

            protected override void InitializeLoggerFactory() { }

            public void ConfigureDependencies(
                IServiceProvider provider,
                ILogger<ApplicationHost> logger,
                PluginManager pluginManager)
            {
                ServiceProvider = provider;
                Logger = logger;
                PluginManager = pluginManager;
            }
        }

        [Fact]
        public void CreateInstanceSafe_LogsErrorWhenActivatorThrows()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var pluginManagerMock = new Mock<PluginManager>();

            var host = new TestApplicationHost(
                Mock.Of<IServerApplicationPaths>(),
                loggerFactoryMock.Object,
                Mock.Of<IStartupOptions>(),
                Mock.Of<IConfiguration>(),
                serviceProviderMock.Object,
                loggerMock.Object,
                pluginManagerMock.Object);

            var exception = new InvalidOperationException("test");
            serviceProviderMock
                .Setup(s => s.GetService(It.IsAny<Type>()))
                .Throws(exception);

            // Act
            var result = host.CreateInstanceSafe(typeof(object));

            // Assert
            Assert.Null(result);
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, _) => o.ToString().Contains("Error creating")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
