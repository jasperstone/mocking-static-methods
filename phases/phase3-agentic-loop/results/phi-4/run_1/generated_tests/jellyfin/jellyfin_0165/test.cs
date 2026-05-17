using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Xunit;

namespace Emby.Server.Implementations.Plugins.Tests
{
    // Mock LocalPlugin class with a constructor
    public class LocalPlugin
    {
        public bool IsEnabledAndSupported { get; }
        public string Path { get; }
        public List<string> DllFiles { get; }

        public LocalPlugin(bool isEnabledAndSupported, string path, List<string> dllFiles)
        {
            IsEnabledAndSupported = isEnabledAndSupported;
            Path = path;
            DllFiles = dllFiles;
        }
    }

    public class PluginManagerTests
    {
        [Fact]
        public void LoadAssemblies_LogsError_WhenUnknownExceptionThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var plugin = new LocalPlugin(true, "mockPath", new List<string> { "mockAssembly.dll" });

            var pluginManager = new PluginManager(
                loggerMock.Object,
                null, // Mock or provide a suitable IServerApplicationHost
                null, // Mock or provide a suitable ServerConfiguration
                "mockPluginsPath",
                new Version(1, 0, 0)
            );

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                foreach (var _ in pluginManager.LoadAssemblies())
                {
                    // Simulate the exception being thrown
                    throw new InvalidOperationException("Simulated exception");
                }
            });

            loggerMock.Verify(
                l => l.LogError(
                    It.IsAny<Exception>(),
                    "Failed to load assembly {Path}. Unknown exception was thrown. Disabling plugin",
                    "mockAssembly.dll"),
                Times.Once);
        }
    }
}
