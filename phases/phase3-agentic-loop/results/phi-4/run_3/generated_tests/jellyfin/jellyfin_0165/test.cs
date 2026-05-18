using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using Xunit;

namespace Emby.Server.Implementations.Plugins.Tests
{
    // Mock classes for missing dependencies
    public interface IServerApplicationHost { }
    public class ServerConfiguration { }

    // Assuming LocalPlugin is a class within the same namespace
    public class LocalPlugin
    {
        public bool IsEnabledAndSupported { get; set; }
        public string Path { get; set; }
        public List<string> DllFiles { get; set; }
    }

    public class PluginManagerTests
    {
        [Fact]
        public void LoadAssemblies_LogsError_WhenExceptionThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var plugin = new LocalPlugin
            {
                IsEnabledAndSupported = true,
                Path = "dummyPath",
                DllFiles = new List<string> { "dummy.dll" }
            };

            var appHostMock = new Mock<IServerApplicationHost>();
            var configMock = new Mock<ServerConfiguration>();

            var pluginManager = new PluginManager(
                loggerMock.Object,
                appHostMock.Object,
                configMock.Object,
                "dummyPath",
                new Version(1, 0, 0)
            );

            // Act
            var assemblies = pluginManager.LoadAssemblies();

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    "Failed to load assembly {Path}. Unknown exception was thrown. Disabling plugin",
                    It.Is<string>(s => s == "dummy.dll")
                ),
                Times.Once
            );
        }
    }
}
