using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Emby.Server.Implementations;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Common.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Emby.Server.Implementations.Tests
{
    public class ApplicationHostTests
    {
        private readonly Mock<ILogger<ApplicationHost>> _mockLogger;
        private readonly Mock<ILoggerFactory> _mockLoggerFactory;
        private readonly Mock<IServerApplicationPaths> _mockApplicationPaths;
        private readonly Mock<IStartupOptions> _mockStartupOptions;
        private readonly Mock<IConfiguration> _mockStartupConfig;
        private readonly Mock<PluginManager> _mockPluginManager;

        public ApplicationHostTests()
        {
            _mockLogger = new Mock<ILogger<ApplicationHost>>();
            _mockLoggerFactory = new Mock<ILoggerFactory>();
            _mockApplicationPaths = new Mock<IServerApplicationPaths>();
            _mockStartupOptions = new Mock<IStartupOptions>();
            _mockStartupConfig = new Mock<IConfiguration>();
            _mockPluginManager = new Mock<PluginManager>(_mockLogger.Object, null, null, null, null);
        }

        [Fact]
        public void CreateInstanceSafe_ShouldLogErrorAndFailPlugin_WhenDILoopDetected()
        {
            // Arrange
            var type = typeof(ApplicationHost);
            var mockServiceProvider = new Mock<IServiceProvider>();
            var applicationHost = new Mock<ApplicationHost>(_mockApplicationPaths.Object, _mockLoggerFactory.Object, _mockStartupOptions.Object, _mockStartupConfig.Object)
            {
                CallBase = true
            };
            applicationHost.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
            applicationHost.Setup(x => x.Logger).Returns(_mockLogger.Object);
            applicationHost.Setup(x => x._pluginManager).Returns(_mockPluginManager.Object);

            // Act
            var result = applicationHost.Object.CreateInstanceSafe(type);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("DI Loop detected")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);

            _mockPluginManager.Verify(x => x.FailPlugin(type.Assembly), Times.Once);

            Assert.Null(result);
        }

        [Fact]
        public void CreateInstanceSafe_ShouldLogErrorAndFailPlugin_WhenExceptionThrown()
        {
            // Arrange
            var type = typeof(ApplicationHost);
            var mockServiceProvider = new Mock<IServiceProvider>();
            var applicationHost = new Mock<ApplicationHost>(_mockApplicationPaths.Object, _mockLoggerFactory.Object, _mockStartupOptions.Object, _mockStartupConfig.Object)
            {
                CallBase = true
            };
            applicationHost.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
            applicationHost.Setup(x => x.Logger).Returns(_mockLogger.Object);
            applicationHost.Setup(x => x._pluginManager).Returns(_mockPluginManager.Object);
            applicationHost.Setup(x => x._creatingInstances).Returns(new List<Type> { type });

            // Act
            var result = applicationHost.Object.CreateInstanceSafe(type);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error creating")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);

            _mockPluginManager.Verify(x => x.FailPlugin(type.Assembly), Times.Once);

            Assert.Null(result);
        }
    }
}
