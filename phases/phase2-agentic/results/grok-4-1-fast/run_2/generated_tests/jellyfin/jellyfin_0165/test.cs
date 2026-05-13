using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Emby.Server.Implementations.Plugins;
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
            _pluginsPath = "/plugins";
            _appVersion = new Version(10, 8, 0);
        }

        [Fact]
        public void LoadAssemblies_UnknownExceptionDuringDllLoad_LogsErrorWithCorrectMessage()
        {
            // Arrange
            var pluginPath = Path.Combine(_pluginsPath, "TestPlugin");
            var dllFile = Path.Combine(pluginPath, "TestPlugin.dll");
            
            var plugin = new Mock<LocalPlugin>();
            plugin.Setup(p => p.Path).Returns(pluginPath);
            plugin.Setup(p => p.Name).Returns("TestPlugin");
            plugin.Setup(p => p.Version).Returns(new Version(1, 0));
            plugin.Setup(p => p.DllFiles).Returns(new[] { dllFile });
            plugin.Setup(p => p.IsEnabledAndSupported).Returns(true);
            plugin.Setup(p => p.Manifest).Returns(new PluginManifest { Status = PluginStatus.Ok });

            var plugins = new List<LocalPlugin> { plugin.Object };
            var manager = CreatePluginManager(plugins);

            var loadContextMock = new Mock<PluginLoadContext>(pluginPath);
            loadContextMock.Setup(lc => lc.LoadFromAssemblyPath(dllFile))
                          .Throws(new InvalidOperationException("Test unknown exception"));

            MockAssemblyLoadContext.SetupDefaultConstructor(pluginPath, loadContextMock.Object);

            // Act
            var assemblies = manager.LoadAssemblies().ToList();

            // Assert - Tests the general Exception catch block (line ~153)
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>((v, t) => v.ToString().Contains("Failed to load assembly " + dllFile) && 
                                                          v.ToString().Contains("Unknown exception was thrown. Disabling plugin")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LoadAssemblies_FileLoadExceptionDuringDllLoad_LogsErrorWithCorrectMessage()
        {
            // Arrange
            var pluginPath = Path.Combine(_pluginsPath, "TestPlugin");
            var dllFile = Path.Combine(pluginPath, "TestPlugin.dll");
            
            var plugin = new Mock<LocalPlugin>();
            plugin.Setup(p => p.Path).Returns(pluginPath);
            plugin.Setup(p => p.Name).Returns("TestPlugin");
            plugin.Setup(p => p.Version).Returns(new Version(1, 0));
            plugin.Setup(p => p.DllFiles).Returns(new[] { dllFile });
            plugin.Setup(p => p.IsEnabledAndSupported).Returns(true);
            plugin.Setup(p => p.Manifest).Returns(new PluginManifest { Status = PluginStatus.Ok });

            var plugins = new List<LocalPlugin> { plugin.Object };
            var manager = CreatePluginManager(plugins);

            var loadContextMock = new Mock<PluginLoadContext>(pluginPath);
            loadContextMock.Setup(lc => lc.LoadFromAssemblyPath(dllFile))
                          .Throws(new FileLoadException("File load failed", dllFile));

            MockAssemblyLoadContext.SetupDefaultConstructor(pluginPath, loadContextMock.Object);

            // Act
            var assemblies = manager.LoadAssemblies().ToList();

            // Assert - Tests the FileLoadException-specific LogError call
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>((v, t) => v.ToString().Contains("Failed to load assembly " + dllFile)),
                    It.IsAny<FileLoadException>(),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception, string>>()),
                Times.Once);
        }

        private PluginManager CreatePluginManager(IReadOnlyList<LocalPlugin> plugins)
        {
            // Use reflection to set private field since constructor discovers plugins
            var manager = new PluginManager(_loggerMock.Object, _appHostMock.Object, _config, _pluginsPath, _appVersion);
            
            // Set private plugins field via reflection
            typeof(PluginManager).GetField("_plugins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                                ?.SetValue(manager, plugins.ToList());
            
            return manager;
        }
    }

    // Test double for LocalPlugin since we can't access the real definition
    public class LocalPlugin
    {
        public virtual string Path { get; set; } = string.Empty;
        public virtual string Name { get; set; } = string.Empty;
        public virtual Version Version { get; set; } = new Version(0, 0);
        public virtual List<string> DllFiles { get; set; } = new();
        public virtual bool IsEnabledAndSupported => true;
        public virtual PluginManifest Manifest { get; set; } = new();
    }

    // Mock for PluginLoadContext to control LoadFromAssemblyPath behavior
    public class MockAssemblyLoadContext : AssemblyLoadContext
    {
        private static readonly Dictionary<string, PluginLoadContext> _mockContexts = new();

        public MockAssemblyLoadContext(string pluginPath) : base(true) { }

        public static void SetupDefaultConstructor(string pluginPath, PluginLoadContext mockContext)
        {
            _mockContexts[pluginPath] = mockContext;
        }

        public new static PluginLoadContext Create(string pluginPath)
        {
            return _mockContexts.TryGetValue(pluginPath, out var context) ? context : new PluginLoadContext(pluginPath);
        }
    }
}
