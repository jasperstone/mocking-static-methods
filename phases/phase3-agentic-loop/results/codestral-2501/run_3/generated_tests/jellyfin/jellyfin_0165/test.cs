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
using MediaBrowser.Model.Configuration;
using Emby.Server.Implementations;

namespace Emby.Server.Implementations.Plugins.Tests
{
    public class PluginManagerTests
    {
        [Fact]
        public void LoadAssemblies_UnknownException_LogsErrorAndDisablesPlugin()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var config = new ServerConfiguration();
            var pluginsPath = "path/to/plugins";
            var appVersion = new Version(1, 0, 0, 0);

            var pluginManager = new PluginManager(loggerMock.Object, appHostMock.Object, config, pluginsPath, appVersion);

            var plugin = new LocalPlugin
            {
                IsEnabledAndSupported = true,
                Path = "path/to/plugin",
                DllFiles = new List<string> { "path/to/plugin.dll" },
                Manifest = new PluginManifest()
            };

            var pluginLoadContextMock = new Mock<PluginLoadContext>(plugin.Path);
            pluginLoadContextMock.Setup(x => x.LoadFromAssemblyPath(It.IsAny<string>())).Throws(new InvalidOperationException("Unknown exception"));

            // Act
            var assemblies = pluginManager.LoadAssemblies();

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    "Failed to load assembly {Path}. Unknown exception was thrown. Disabling plugin",
                    It.IsAny<object>()),
                Times.Once);

            Assert.Empty(assemblies);
        }
    }
}
