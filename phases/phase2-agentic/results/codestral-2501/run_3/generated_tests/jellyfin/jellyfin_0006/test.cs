using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Emby.Server.Implementations;
using Emby.Server.Implementations.Plugins;
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
            var pluginManagerMock = new Mock<PluginManager>(MockBehavior.Strict, null, null, null, null, null);
            var applicationHostMock = new Mock<ApplicationHost>(MockBehavior.Strict, null, null, null, null);
            applicationHostMock.SetupGet(x => x.Logger).Returns(loggerMock.Object);
            applicationHostMock.SetupGet(x => x._pluginManager).Returns(pluginManagerMock.Object);

            var type = typeof(ApplicationHost);
            applicationHostMock.Object._creatingInstances = new List<Type> { type };

            // Act
            Assert.Throws<TypeLoadException>(() => applicationHostMock.Object.CreateInstanceSafe(type));

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
