using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Emby.Server.Implementations.Plugins;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests.Plugins
{
    public class PluginManagerTests
    {
        private readonly Mock<ILogger<PluginManager>> _loggerMock;
        private readonly Mock<object> _appHostMock;
        private readonly object _config;
        private readonly string _pluginsPath;
        private readonly Version _appVersion;

        public PluginManagerTests()
        {
            _loggerMock = new Mock<ILogger<PluginManager>>();
            _appHostMock = new Mock<object>();
            _config = new object();
            _pluginsPath = "/fake/plugins/path";
            _appVersion = new Version(10, 8, 0);
        }

        [Fact]
        public void LoadAssemblies_WhenGeneralExceptionThrownInLoadFromAssemblyPath_LogsErrorWithCorrectMessage()
        {
            // Arrange
            var plugin = CreateMockPlugin("/fake/plugin", new[] { "/fake/plugin/MyPlugin.dll" });
            var plugins = new List<object> { plugin };
            var manager = CreatePluginManager(plugins);

            _loggerMock
                .Setup(x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to load assembly /fake/plugin/MyPlugin.dll. Unknown exception was thrown. Disabling plugin")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Verifiable();

            // Act
            var assemblies = manager.LoadAssemblies().ToList();

            // Assert
            _loggerMock.Verify();
        }

        [Fact]
        public void LoadAssemblies_WhenGeneralExceptionThrownInAssemblyGetTypes_LogsErrorWithCorrectMessage()
        {
            // Arrange
            var plugin = CreateMockPlugin("/fake/plugin", new[] { "/fake/assembly.dll" });
            var plugins = new List<object> { plugin };
            var manager = CreatePluginManager(plugins);

            _loggerMock
                .Setup(x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to load assembly /fake/assembly.dll. Unknown exception was thrown. Disabling plugin")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Verifiable();

            // Act
            var assemblies = manager.LoadAssemblies().ToList();

            // Assert
            _loggerMock.Verify();
        }

        private PluginManager CreatePluginManager(List<object> plugins)
        {
            var manager = new PluginManager(
                _loggerMock.Object,
                _appHostMock.Object,
                _config,
                _pluginsPath,
                _appVersion);

            var field = typeof(PluginManager).GetField("_plugins", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            field.SetValue(manager, plugins);

            return manager;
        }

        private static object CreateMockPlugin(string path, string[] dllFiles)
        {
            var pluginMock = new Mock<object>();
            pluginMock.Setup(p => p.IsEnabledAndSupported).Returns(true);
            pluginMock.Setup(p => p.Path).Returns(path);
            pluginMock.Setup(p => p.DllFiles).Returns((IReadOnlyList<string>)dllFiles);
            return pluginMock.Object;
        }
    }
}
