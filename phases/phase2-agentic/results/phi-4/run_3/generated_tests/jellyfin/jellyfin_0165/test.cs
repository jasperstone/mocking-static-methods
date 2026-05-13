using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using Emby.Server.Implementations.Plugins;

public class PluginManagerTests
{
    [Fact]
    public void LoadAssemblies_LogsError_WhenUnknownExceptionThrown()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<PluginManager>>();
        var plugin = new Plugin("path", new PluginManifest(), new List<string> { "assembly.dll" });
        var pluginManager = new PluginManager(loggerMock.Object, null, null, "pluginsPath", new Version(1, 0, 0));

        // Act
        var assemblies = pluginManager.LoadAssemblies();

        // Assert
        loggerMock.Verify(
            x => x.LogError(
                It.IsAny<Exception>(),
                "Failed to load assembly {Path}. Unknown exception was thrown. Disabling plugin",
                "assembly.dll"),
            Times.Once);
    }
}
