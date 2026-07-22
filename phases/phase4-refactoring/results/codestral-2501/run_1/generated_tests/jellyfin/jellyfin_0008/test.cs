using System;
using System.Collections.Generic;
using Emby.Server.Implementations;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Common.Plugins;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common;

namespace Emby.Server.Implementations.Tests
{
    public class ApplicationHostTests
    {
        [Fact]
        public void CreateInstanceSafe_ShouldLogError_WhenExceptionIsThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var pluginManagerMock = new Mock<PluginManager>(MockBehavior.Strict, loggerMock.Object, null, null, null, null);

            var applicationHost = new Mock<ApplicationHost>(null, null, null, null);
            applicationHost.SetupGet(x => x.Logger).Returns(loggerMock.Object);
            applicationHost.SetupGet(x => x._pluginManager).Returns(pluginManagerMock.Object);

            var type = typeof(string); // Using string as a placeholder type

            // Act
            applicationHost.Object.CreateInstanceSafe(type);

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    It.Is<string>(s => s.Contains("Error creating {Type}")),
                    It.Is<object[]>(o => o[0] == type)),
                Times.Once);
        }
    }
}
