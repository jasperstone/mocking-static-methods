using Emby.Server.Implementations.Plugins;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Xunit;

namespace Emby.Server.Implementations.Tests.Plugins
{
    public class PluginManagerTests
    {
        [Fact]
        public void LoadAssemblies_LogsError_WhenFileLoadExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var pluginManager = new PluginManager(loggerMock.Object, null, null, string.Empty, new Version());

            // Act and Assert
            loggerMock.Setup(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()));
            pluginManager.LoadAssemblies();
            loggerMock.Verify(l => l.LogError(It.IsAny<FileLoadException>(), "Failed to load assembly {Path}. Disabling plugin", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void LoadAssemblies_LogsError_WhenExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var pluginManager = new PluginManager(loggerMock.Object, null, null, string.Empty, new Version());

            // Act and Assert
            loggerMock.Setup(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()));
            pluginManager.LoadAssemblies();
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Failed to load assembly {Path}. Unknown exception was thrown. Disabling plugin", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void LoadAssemblies_LogsError_WhenTypeLoadExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var pluginManager = new PluginManager(loggerMock.Object, null, null, string.Empty, new Version());

            // Act and Assert
            loggerMock.Setup(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()));
            pluginManager.LoadAssemblies();
            loggerMock.Verify(l => l.LogError(It.IsAny<TypeLoadException>(), "Failed to load assembly {Path}. This error occurs when a plugin references an incompatible version of one of the shared libraries. Disabling plugin", It.IsAny<string>()), Times.Once);
        }
    }
}
