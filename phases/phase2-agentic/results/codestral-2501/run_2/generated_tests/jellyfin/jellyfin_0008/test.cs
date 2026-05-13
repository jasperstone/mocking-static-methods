using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Emby.Server.Implementations;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Common.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Emby.Server.Tests.Implementations
{
    public class ApplicationHostTests
    {
        private readonly Mock<ILogger<ApplicationHost>> _mockLogger;
        private readonly Mock<ILoggerFactory> _mockLoggerFactory;
        private readonly Mock<IServerApplicationPaths> _mockApplicationPaths;
        private readonly Mock<IStartupOptions> _mockStartupOptions;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<PluginManager> _mockPluginManager;
        private readonly Mock<IServiceProvider> _mockServiceProvider;

        public ApplicationHostTests()
        {
            _mockLogger = new Mock<ILogger<ApplicationHost>>();
            _mockLoggerFactory = new Mock<ILoggerFactory>();
            _mockApplicationPaths = new Mock<IServerApplicationPaths>();
            _mockStartupOptions = new Mock<IStartupOptions>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockPluginManager = new Mock<PluginManager>(_mockLogger.Object, null, null, null, null);
            _mockServiceProvider = new Mock<IServiceProvider>();
        }

        [Fact]
        public void CreateInstanceSafe_ShouldLogErrorOnDILoop()
        {
            // Arrange
            var type = typeof(ApplicationHost);
            var applicationHost = new Mock<ApplicationHost>(_mockApplicationPaths.Object, _mockLoggerFactory.Object, _mockStartupOptions.Object, _mockConfiguration.Object);
            applicationHost.SetupGet(x => x.Logger).Returns(_mockLogger.Object);
            applicationHost.SetupGet(x => x.ServiceProvider).Returns(_mockServiceProvider.Object);
            applicationHost.SetupGet(x => x._pluginManager).Returns(_mockPluginManager.Object);

            // Act
            applicationHost.Object.CreateInstanceSafe(type);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("DI Loop detected")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }

        [Fact]
        public void CreateInstanceSafe_ShouldLogErrorOnException()
        {
            // Arrange
            var type = typeof(ApplicationHost);
            var applicationHost = new Mock<ApplicationHost>(_mockApplicationPaths.Object, _mockLoggerFactory.Object, _mockStartupOptions.Object, _mockConfiguration.Object);
            applicationHost.SetupGet(x => x.Logger).Returns(_mockLogger.Object);
            applicationHost.SetupGet(x => x.ServiceProvider).Returns(_mockServiceProvider.Object);
            applicationHost.SetupGet(x => x._pluginManager).Returns(_mockPluginManager.Object);
            applicationHost.Setup(x => x.ServiceProvider.GetService(It.IsAny<Type>())).Throws(new Exception("Test exception"));

            // Act
            applicationHost.Object.CreateInstanceSafe(type);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error creating")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }
}
