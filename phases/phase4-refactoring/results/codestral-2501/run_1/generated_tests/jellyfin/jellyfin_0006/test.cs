using System;
using System.Collections.Generic;
using System.Reflection;
using Emby.Server.Implementations;
using Emby.Server.Implementations.Plugins;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.Extensions.Configuration;
using MediaBrowser.Common.Configuration;

namespace Emby.Server.Implementations.Tests
{
    public class ApplicationHostTests
    {
        [Fact]
        public void CreateInstanceSafe_DetectsDILoop_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var pluginManagerMock = new Mock<PluginManager>();
            var applicationHostMock = new Mock<ApplicationHost>(Mock.Of<IServerApplicationPaths>(), Mock.Of<ILoggerFactory>(), Mock.Of<IStartupOptions>(), Mock.Of<IConfiguration>());

            applicationHostMock.SetupGet(x => x.Logger).Returns(loggerMock.Object);
            applicationHostMock.SetupGet(x => x._pluginManager).Returns(pluginManagerMock.Object);

            var type = typeof(ApplicationHost);

            // Act
            applicationHostMock.Object.CreateInstanceSafe(type);

            // Assert
            loggerMock.Verify(
                x => x.LogError("DI Loop detected in the attempted creation of {Type}", type.FullName),
                Times.Once);

            loggerMock.Verify(
                x => x.LogError("Called from: {TypeName}", type.FullName),
                Times.Once);

            pluginManagerMock.Verify(
                x => x.FailPlugin(type.Assembly),
                Times.Once);
        }
    }
}
