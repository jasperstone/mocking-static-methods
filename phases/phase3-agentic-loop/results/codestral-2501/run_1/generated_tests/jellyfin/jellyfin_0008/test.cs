using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using Emby.Server.Implementations;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;

namespace ApplicationHostTests
{
    public class ApplicationHostTests
    {
        [Fact]
        public void CreateInstanceSafe_ExceptionLogged()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var pluginManagerMock = new Mock<PluginManager>(MockBehavior.Strict, loggerMock.Object, null, null, null, null);
            var applicationHostMock = new Mock<ApplicationHost>(MockBehavior.Strict, null, null, null, null);
            applicationHostMock.SetupGet(x => x.Logger).Returns(loggerMock.Object);
            applicationHostMock.SetupGet(x => x.PluginManager).Returns(pluginManagerMock.Object);

            var type = typeof(ApplicationHost);

            // Act
            var result = applicationHostMock.Object.CreateInstanceSafe(type);

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    It.Is<string>(s => s.Contains("Error creating {Type}")),
                    It.Is<object[]>(o => o[0] == type)),
                Times.Once);

            pluginManagerMock.Verify(
                x => x.FailPlugin(It.IsAny<Assembly>()),
                Times.Once);

            Assert.Null(result);
        }
    }
}
