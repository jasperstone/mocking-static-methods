using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Plugins.Tests
{
    public class PluginManagerTests
    {
        [Fact]
        public void LoadAssemblies_LogsError_WhenUnknownExceptionThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var plugin = new LocalPlugin(
                path: "dummyPath",
                manifest: new PluginManifest { IsEnabled = true, Status = PluginStatus.Enabled },
                dllFiles: new List<string> { "dummy.dll" });

            var pluginManager = new PluginManager(
                logger: loggerMock.Object,
                appHost: null, // Mock or replace with a suitable object
                config: null,  // Mock or replace with a suitable object
                pluginsPath: "dummyPath",
                appVersion: new Version(1, 0, 0));

            // Act
            var assemblies = pluginManager.LoadAssemblies();

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    "Failed to load assembly {Path}. Unknown exception was thrown. Disabling plugin",
                    "dummy.dll"),
                Times.Once);
        }
    }
}
