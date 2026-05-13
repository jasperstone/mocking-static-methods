using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Emby.Server.Implementations;
using System;
using System.Collections.Generic;
using MediaBrowser.Common.Plugins;

namespace ApplicationHostTests
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
            applicationHostMock.Object._creatingInstances = new List<Type> { type };

            // Act
            var exception = Assert.Throws<TypeLoadException>(() => applicationHostMock.Object.CreateInstanceSafe(type));

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

            Assert.Equal("DI Loop detected", exception.Message);
        }
    }
}
