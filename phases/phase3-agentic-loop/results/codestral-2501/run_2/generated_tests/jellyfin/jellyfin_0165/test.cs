using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Controller.Plugins;

namespace Emby.Server.Implementations.Plugins.Tests
{
    public class PluginManagerTests
    {
        [Fact]
        public void LoadAssemblies_UnknownException_LogsErrorAndDisablesPlugin()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var pluginMock = new Mock<LocalPlugin>();
            pluginMock.Setup(p => p.IsEnabledAndSupported).Returns(true);
            pluginMock.Setup(p => p.DllFiles).Returns(new List<string> { "invalid.dll" });
            pluginMock.Setup(p => p.Path).Returns("pluginPath");
            pluginMock.Setup(p => p.Name).Returns("TestPlugin");
            pluginMock.Setup(p => p.Version).Returns(new Version(1, 0));
            pluginMock.Setup(p => p.Id).Returns(Guid.NewGuid());
            pluginMock.Setup(p => p.IsBundledPlugin).Returns(false);

            var pluginManager = new PluginManager(loggerMock.Object, null, null, "pluginsPath", new Version(1, 0));

            // Act
            var assemblies = pluginManager.LoadAssemblies();

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    "Failed to load assembly {Path}. Unknown exception was thrown. Disabling plugin",
                    It.IsAny<object>()),
                Times.Once);
        }
    }
}
