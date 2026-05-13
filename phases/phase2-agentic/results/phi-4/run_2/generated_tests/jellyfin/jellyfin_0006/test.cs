using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Emby.Server.Implementations.Tests
{
    public class ApplicationHostTests
    {
        [Fact]
        public void CreateInstanceSafe_LogsErrorOnDILoop()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var pluginManagerMock = new Mock<PluginManager>();
            var applicationPathsMock = new Mock<IServerApplicationPaths>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var startupOptionsMock = new Mock<IStartupOptions>();
            var startupConfigMock = new Mock<IConfiguration>();

            loggerFactoryMock.Setup(lf => lf.CreateLogger<ApplicationHost>()).Returns(loggerMock.Object);

            var applicationHost = new ApplicationHost(
                applicationPathsMock.Object,
                loggerFactoryMock.Object,
                startupOptionsMock.Object,
                startupConfigMock.Object)
            {
                _pluginManager = pluginManagerMock.Object
            };

            var type = typeof(object);
            applicationHost._creatingInstances = new List<Type> { type };

            // Act
            applicationHost.CreateInstanceSafe(type);

            // Assert
            loggerMock.Verify(
                l => l.LogError(
                    It.Is<string>(s => s.Contains("DI Loop detected in the attempted creation of")),
                    It.Is<object>(o => o == type.FullName)),
                Times.Once);

            loggerMock.Verify(
                l => l.LogError(
                    It.Is<string>(s => s.Contains("Called from:")),
                    It.IsAny<object>()),
                Times.AtLeastOnce);
        }
    }
}
