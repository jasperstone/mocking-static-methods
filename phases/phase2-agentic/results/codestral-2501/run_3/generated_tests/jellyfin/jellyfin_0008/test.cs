using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using Emby.Server.Implementations;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Emby.Server.Implementations.Tests
{
    public class ApplicationHostTests
    {
        private readonly Mock<ILogger<ApplicationHost>> _loggerMock;
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Mock<IServerApplicationPaths> _applicationPathsMock;
        private readonly Mock<IStartupOptions> _startupOptionsMock;
        private readonly Mock<IConfiguration> _startupConfigMock;
        private readonly Mock<PluginManager> _pluginManagerMock;

        public ApplicationHostTests()
        {
            _loggerMock = new Mock<ILogger<ApplicationHost>>();
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _applicationPathsMock = new Mock<IServerApplicationPaths>();
            _startupOptionsMock = new Mock<IStartupOptions>();
            _startupConfigMock = new Mock<IConfiguration>();
            _pluginManagerMock = new Mock<PluginManager>(_loggerMock.Object, null, null, null, null);
        }

        [Fact]
        public void CreateInstanceSafe_ShouldLogError_WhenDILoopDetected()
        {
            // Arrange
            var type = typeof(ApplicationHost);
            var applicationHost = new Mock<ApplicationHost>(_applicationPathsMock.Object, _loggerFactoryMock.Object, _startupOptionsMock.Object, _startupConfigMock.Object)
            {
                CallBase = true
            };

            applicationHost.Object._creatingInstances = new List<Type> { type };

            // Act
            Assert.Throws<TypeLoadException>(() => applicationHost.Object.CreateInstanceSafe(type));

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("DI Loop detected")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void CreateInstanceSafe_ShouldLogError_WhenExceptionThrown()
        {
            // Arrange
            var type = typeof(ApplicationHost);
            var applicationHost = new Mock<ApplicationHost>(_applicationPathsMock.Object, _loggerFactoryMock.Object, _startupOptionsMock.Object, _startupConfigMock.Object)
            {
                CallBase = true
            };

            applicationHost.Object._creatingInstances = new List<Type>();

            // Act
            applicationHost.Object.CreateInstanceSafe(type);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error creating")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
