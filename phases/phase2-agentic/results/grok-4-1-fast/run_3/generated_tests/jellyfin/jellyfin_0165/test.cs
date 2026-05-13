using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Plugins;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
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
            _pluginsPath = Path.Combine(Path.GetTempPath(), "plugins");
            _appVersion = new Version(10, 8, 0);
        }

        [Fact]
        public void LoadAssemblies_UnknownExceptionDuringDllLoad_LogsErrorWithCorrectMessage()
        {
            // Arrange
            var pluginPath = Path.Combine(_pluginsPath, "TestPlugin");
            var dllFile = Path.Combine(pluginPath, "TestPlugin.dll");
            
            var plugin = CreateMockPlugin(pluginPath, "TestPlugin", new Version(1, 0), new[] { dllFile });

            var plugins = new List<LocalPlugin> { plugin.Object };
            var manager = CreatePluginManager(plugins);

            MockStaticAssemblyLoadContext(pluginPath).Setup(lc => lc.LoadFromAssemblyPath(dllFile))
                .Throws(new InvalidOperationException("Test unknown exception"));

            // Act
            var assemblies = manager.LoadAssemblies().ToList();

            // Assert - Verifies LogError extension call on line ~153 (general Exception catch)
            _loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    It.Is<string>(msg => msg.Contains(dllFile) && 
                                        msg.Contains("Unknown exception was thrown. Disabling plugin")),
                    It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public void LoadAssemblies_FileLoadExceptionDuringDllLoad_LogsErrorWithCorrectMessage()
        {
            // Arrange
            var pluginPath = Path.Combine(_pluginsPath, "TestPlugin");
            var dllFile = Path.Combine(pluginPath, "TestPlugin.dll");
            
            var plugin = CreateMockPlugin(pluginPath, "TestPlugin", new Version(1, 0), new[] { dllFile });

            var plugins = new List<LocalPlugin> { plugin.Object };
            var manager = CreatePluginManager(plugins);

            MockStaticAssemblyLoadContext(pluginPath).Setup(lc => lc.LoadFromAssemblyPath(dllFile))
                .Throws(new FileLoadException("File load failed", dllFile));

            // Act
            var assemblies = manager.LoadAssemblies().ToList();

            // Assert - Verifies FileLoadException-specific LogError call
            _loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<FileLoadException>(),
                    It.Is<string>(msg => msg.Contains("Failed to load assembly") && msg.Contains(dllFile)),
                    It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public void LoadAssemblies_SuccessfulLoad_LogsInformationNotError()
        {
            // Arrange
            var pluginPath = Path.Combine(_pluginsPath, "TestPlugin");
            var dllFile = Path.Combine(pluginPath, "TestPlugin.dll");
            
            var plugin = CreateMockPlugin(pluginPath, "TestPlugin", new Version(1, 0), new[] { dllFile });

            var plugins = new List<LocalPlugin> { plugin.Object };
            var manager = CreatePluginManager(plugins);

            var mockAssembly = new Mock<Assembly>();
            mockAssembly.Setup(a => a.Location).Returns(dllFile);
            mockAssembly.Setup(a => a.FullName).Returns("TestPlugin, Version=1.0.0.0");
            mockAssembly.Setup(a => a.GetTypes()).Returns(Array.Empty<Type>());

            MockStaticAssemblyLoadContext(pluginPath).Setup(lc => lc.LoadFromAssemblyPath(dllFile))
                .Returns(mockAssembly.Object);

            // Act
            var assemblies = manager.LoadAssemblies().ToList();

            // Assert - No error logged on success
            _loggerMock.Verify(x => x.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }

        private static Mock<LocalPlugin> CreateMockPlugin(string path, string name, Version version, string[] dllFiles)
        {
            var plugin = new Mock<LocalPlugin>();
            plugin.Setup(p => p.Path).Returns(path);
            plugin.Setup(p => p.Name).Returns(name);
            plugin.Setup(p => p.Version).Returns(version);
            plugin.Setup(p => p.DllFiles).Returns(dllFiles.ToList());
            plugin.Setup(p => p.IsEnabledAndSupported).Returns(true);
            plugin.Setup(p => p.Manifest).Returns(new PluginManifest { Status = PluginStatus.Ok });
            return plugin;
        }

        private PluginManager CreatePluginManager(IReadOnlyList<LocalPlugin> plugins)
        {
            var manager = new PluginManager(_loggerMock.Object, _appHostMock.Object, _config, _pluginsPath, _appVersion);
            
            // Inject test plugins using reflection (constructor normally discovers plugins)
            var pluginsField = typeof(PluginManager).GetField("_plugins", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            pluginsField?.SetValue(manager, plugins.ToList());
            
            return manager;
        }

        private static Mock<PluginLoadContext> MockStaticAssemblyLoadContext(string pluginPath)
        {
            var loadContextMock = new Mock<PluginLoadContext>(pluginPath) { CallBase = true };
            // Note: PluginLoadContext constructor is called by code under test, mocked behavior setup above
            return loadContextMock;
        }
    }
}
