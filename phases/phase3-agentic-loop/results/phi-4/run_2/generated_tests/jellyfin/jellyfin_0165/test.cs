using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
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
            var pluginMock = new Mock<PluginManager.LocalPlugin>();
            var assemblyLoadContextMock = new Mock<AssemblyLoadContext>();

            pluginMock.Setup(p => p.IsEnabledAndSupported).Returns(true);
            pluginMock.Setup(p => p.Path).Returns("mockPath");
            pluginMock.Setup(p => p.DllFiles).Returns(new List<string> { "mockAssembly.dll" });

            var pluginManager = new PluginManager(
                loggerMock.Object,
                null, // Mock or provide a suitable IServerApplicationHost
                null, // Mock or provide a suitable ServerConfiguration
                "mockPluginsPath",
                new Version(1, 0, 0)
            );

            // Act
            var exception = new InvalidOperationException("Test exception");
            try
            {
                pluginManager.LoadAssemblies();
            }
            catch (Exception)
            {
                // Expected exception
            }

            // Assert
            loggerMock.Verify(
                l => l.LogError(
                    It.IsAny<Exception>(),
                    "Failed to load assembly {Path}. Unknown exception was thrown. Disabling plugin",
                    "mockAssembly.dll"),
                Times.Once);
        }

        [Fact]
        public void LoadAssemblies_LogsError_WhenFileLoadExceptionThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var pluginMock = new Mock<PluginManager.LocalPlugin>();
            var assemblyLoadContextMock = new Mock<AssemblyLoadContext>();

            pluginMock.Setup(p => p.IsEnabledAndSupported).Returns(true);
            pluginMock.Setup(p => p.Path).Returns("mockPath");
            pluginMock.Setup(p => p.DllFiles).Returns(new List<string> { "mockAssembly.dll" });

            var pluginManager = new PluginManager(
                loggerMock.Object,
                null, // Mock or provide a suitable IServerApplicationHost
                null, // Mock or provide a suitable ServerConfiguration
                "mockPluginsPath",
                new Version(1, 0, 0)
            );

            // Act
            var fileLoadException = new FileLoadException("Test FileLoadException", null, null);
            try
            {
                pluginManager.LoadAssemblies();
            }
            catch (Exception)
            {
                // Expected exception
            }

            // Assert
            loggerMock.Verify(
                l => l.LogError(
                    It.Is<FileLoadException>(ex => ex.Message == "Test FileLoadException"),
                    "Failed to load assembly {Path}. Disabling plugin",
                    "mockAssembly.dll"),
                Times.Once);
        }

        [Fact]
        public void LoadAssemblies_LogsError_WhenTypeLoadExceptionThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var pluginMock = new Mock<PluginManager.LocalPlugin>();
            var assemblyLoadContextMock = new Mock<AssemblyLoadContext>();

            pluginMock.Setup(p => p.IsEnabledAndSupported).Returns(true);
            pluginMock.Setup(p => p.Path).Returns("mockPath");
            pluginMock.Setup(p => p.DllFiles).Returns(new List<string> { "mockAssembly.dll" });

            var pluginManager = new PluginManager(
                loggerMock.Object,
                null, // Mock or provide a suitable IServerApplicationHost
                null, // Mock or provide a suitable ServerConfiguration
                "mockPluginsPath",
                new Version(1, 0, 0)
            );

            // Act
            var typeLoadException = new TypeLoadException("Test TypeLoadException");
            try
            {
                pluginManager.LoadAssemblies();
            }
            catch (Exception)
            {
                // Expected exception
            }

            // Assert
            loggerMock.Verify(
                l => l.LogError(
                    It.Is<TypeLoadException>(ex => ex.Message == "Test TypeLoadException"),
                    "Failed to load assembly {Path}. This error occurs when a plugin references an incompatible version of one of the shared libraries. Disabling plugin",
                    "mockAssembly.dll"),
                Times.Once);
        }

        [Fact]
        public void LoadAssemblies_SkipsDisabledPlugins()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var pluginMock = new Mock<PluginManager.LocalPlugin>();

            pluginMock.Setup(p => p.IsEnabledAndSupported).Returns(false);
            pluginMock.Setup(p => p.Version).Returns(new Version(1, 0, 0));
            pluginMock.Setup(p => p.Name).Returns("TestPlugin");

            var pluginManager = new PluginManager(
                loggerMock.Object,
                null, // Mock or provide a suitable IServerApplicationHost
                null, // Mock or provide a suitable ServerConfiguration
                "mockPluginsPath",
                new Version(1, 0, 0)
            );

            pluginManager._plugins.Add(pluginMock.Object);

            // Act
            pluginManager.LoadAssemblies();

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(
                    "Skipping disabled plugin {Version} of {Name} ",
                    It.IsAny<Version>(),
                    It.IsAny<string>()),
                Times.Once);
        }
    }
}
