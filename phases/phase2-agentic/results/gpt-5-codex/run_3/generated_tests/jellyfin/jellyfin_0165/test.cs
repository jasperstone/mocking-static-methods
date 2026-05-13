using System;
using System.Collections.Generic;
using System.IO;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Controller;
using MediaBrowser.Model.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests.Plugins
{
    public class PluginManagerTests
    {
        private static PluginManager CreatePluginManager(ILogger<PluginManager>? logger = null, string? pluginRoot = null)
        {
            var loggerMock = logger ?? Mock.Of<ILogger<PluginManager>>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var config = new ServerConfiguration();

            return new PluginManager(
                loggerMock,
                appHostMock.Object,
                config,
                pluginRoot ?? Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")),
                new Version(10, 0));
        }

        private static LocalPlugin CreateEnabledPlugin(string pluginPath, params string[] dllFiles)
        {
            var manifest = new PluginManifest
            {
                IsEnabled = true,
                Status = MediaBrowser.Model.Plugins.PluginStatus.Active,
                Name = "TestPlugin",
                Description = "Test",
                Version = "1.0.0.0",
                TargetAbi = "0.0.0",
                AssemblyGuid = Guid.NewGuid().ToString(),
                Changelog = "None",
                ImageUrl = string.Empty,
                Category = "General"
            };

            var plugin = new LocalPlugin(pluginPath, manifest, dllFiles);
            return plugin;
        }

        [Fact]
        public void LoadAssemblies_LogsErrorWhenAssemblyLoadThrows()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<PluginManager>>();
            var pluginRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(pluginRoot);

            var dllPath = Path.Combine(pluginRoot, "missing.dll");
            // File intentionally not created to trigger FileNotFoundException.

            var plugin = CreateEnabledPlugin(pluginRoot, dllPath);

            var manager = CreatePluginManager(mockLogger.Object, pluginRoot);

            var pluginsField = typeof(PluginManager).GetField("_plugins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(pluginsField);

            pluginsField!.SetValue(manager, new List<LocalPlugin> { plugin });

            // Act
            foreach (var _ in manager.LoadAssemblies())
            {
                // Enumerate to execute the loading logic
            }

            // Assert
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, _) => state.ToString()!.Contains("Failed to load assembly") && state.ToString()!.Contains("Unknown exception was thrown")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
                Times.Once);
        }
    }
}
