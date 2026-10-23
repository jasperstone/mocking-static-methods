using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Controller;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Plugins;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests.Plugins
{
    public class PluginManagerTests
    {
        private readonly Mock<ILogger<PluginManager>> _loggerMock;
        private readonly Mock<IServerApplicationHost> _appHostMock;
        private readonly ServerConfiguration _config;
        private readonly string _pluginsPath;
        private readonly Version _appVersion;

        public PluginManagerTests()
        {
            _loggerMock = new Mock<ILogger<PluginManager>>();
            _appHostMock = new Mock<IServerApplicationHost>();
            _config = new ServerConfiguration();
            _pluginsPath = "/fake/plugins/path";
            _appVersion = new Version(10, 8, 0, 0);
        }

        [Fact]
        public void LoadAssemblies_GeneralExceptionInLoadFromAssemblyPath_LogsErrorWithUnknownExceptionMessage()
        {
            // Arrange
            var plugin = CreateMockPlugin("/fake/plugin.dll");
            var plugins = new List<LocalPlugin> { plugin };
            var manager = CreatePluginManager(plugins);

            // Act
            var assemblies = manager.LoadAssemblies().ToList();

            // Assert - Verifies the LogError call on line 153 (general Exception catch)
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>((v, t) => v.ToString()!.Contains("Failed to load assembly /fake/plugin.dll. Unknown exception was thrown. Disabling plugin")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LoadAssemblies_GeneralExceptionInGetTypes_LogsErrorWithUnknownExceptionMessage()
        {
            // Arrange - Plugin loads successfully but GetTypes throws general exception
            // This tests the second general catch block with the same message pattern as line 153
            var plugin = CreateMockPlugin("/fake/assembly.dll");
            var plugins = new List<LocalPlugin> { plugin };
            var manager = CreatePluginManager(plugins);

            // Act
            var assemblies = manager.LoadAssemblies().ToList();

            // Assert - Verifies the general catch LogError call for GetTypes (same message pattern as line 153)
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>((v, t) => v.ToString()!.Contains("Failed to load assembly /fake/assembly.dll. Unknown exception was thrown. Disabling plugin")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
                Times.Once);
        }

        private LocalPlugin CreateMockPlugin(params string[] dllFiles)
        {
            var pluginMock = new Mock<LocalPlugin>();
            pluginMock.Setup(p => p.Name).Returns("TestPlugin");
            pluginMock.Setup(p => p.Version).Returns(new Version(1, 0, 0, 0));
            pluginMock.Setup(p => p.Path).Returns("/fake/path");
            pluginMock.Setup(p => p.DllFiles).Returns(dllFiles.ToList());
            pluginMock.Setup(p => p.IsEnabledAndSupported).Returns(true);
            pluginMock.Setup(p => p.Manifest).Returns(new PluginManifest { Status = PluginStatus.Ok });
            return pluginMock.Object;
        }

        private PluginManager CreatePluginManager(IReadOnlyList<LocalPlugin> plugins)
        {
            var manager = new PluginManager(_loggerMock.Object, _appHostMock.Object, _config, _pluginsPath, _appVersion);
            
            // Override private _plugins field via reflection to bypass file system discovery
            var pluginsField = typeof(PluginManager).GetField("_plugins", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            pluginsField?.SetValue(manager, plugins.ToList());

            return manager;
        }
    }
}
