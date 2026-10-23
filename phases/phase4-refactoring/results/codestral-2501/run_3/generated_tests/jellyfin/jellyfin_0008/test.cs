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
        public void CreateInstanceSafe_ExceptionLogged()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var pluginManagerMock = new Mock<PluginManager>();
            var applicationHostMock = new Mock<ApplicationHost>(Mock.Of<IServerApplicationPaths>(), Mock.Of<ILoggerFactory>(), Mock.Of<IStartupOptions>(), Mock.Of<IConfiguration>());

            applicationHostMock.SetupGet(x => x.Logger).Returns(loggerMock.Object);
            applicationHostMock.SetupGet(x => x._pluginManager).Returns(pluginManagerMock.Object);

            var type = typeof(string); // Using string as a placeholder type

            // Act
            applicationHostMock.Object.CreateInstanceSafe(type);

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object>()),
                Times.Once);
        }
    }
}
