using System;
using System.Collections.Generic;
using Emby.Server.Implementations;
using Emby.Server.Implementations.Plugins;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests
{
    public class ApplicationHostTests
    {
        [Fact]
        public void CreateInstanceSafe_ExceptionThrown_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var pluginManagerMock = new Mock<PluginManager>();

            var applicationHostMock = new Mock<ApplicationHost>(
                Mock.Of<IServerApplicationPaths>(),
                Mock.Of<ILoggerFactory>(),
                Mock.Of<IStartupOptions>(),
                Mock.Of<IConfiguration>());

            applicationHostMock.Setup(x => x.Logger).Returns(loggerMock.Object);
            applicationHostMock.Setup(x => x._pluginManager).Returns(pluginManagerMock.Object);

            var type = typeof(object); // Any type will do for this test

            // Act
            var result = applicationHostMock.Object.CreateInstanceSafe(type);

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object>()),
                Times.Once);

            pluginManagerMock.Verify(
                x => x.FailPlugin(It.IsAny<System.Reflection.Assembly>()),
                Times.Once);

            Assert.Null(result);
        }
    }
}
