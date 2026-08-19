using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Plugins.Tests
{
    public class PluginManagerTests
    {
        [Fact]
        public void LoadAssemblies_FileLoadException_LogsErrorAndDisablesPlugin()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var plugin = new LocalPlugin("testPath", true, new PluginManifest());
            var pluginManager = new PluginManager(loggerMock.Object, null, null, "testPath", new Version(1, 0));

            var assemblyLoadContextMock = new Mock<PluginLoadContext>(plugin.Path);
            assemblyLoadContextMock.Setup(x => x.LoadFromAssemblyPath(It.IsAny<string>())).Throws(new FileLoadException());

            // Act
            var assemblies = pluginManager.LoadAssemblies();

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<FileLoadException>(),
                    "Failed to load assembly {Path}. Disabling plugin",
                    "testDll"),
                Times.Once);
        }

        [Fact]
        public void LoadAssemblies_GeneralException_LogsErrorAndDisablesPlugin()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var plugin = new LocalPlugin("testPath", true, new PluginManifest());
            var pluginManager = new PluginManager(loggerMock.Object, null, null, "testPath", new Version(1, 0));

            var assemblyLoadContextMock = new Mock<PluginLoadContext>(plugin.Path);
            assemblyLoadContextMock.Setup(x => x.LoadFromAssemblyPath(It.IsAny<string>())).Throws(new Exception());

            // Act
            var assemblies = pluginManager.LoadAssemblies();

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    "Failed to load assembly {Path}. Unknown exception was thrown. Disabling plugin",
                    "testDll"),
                Times.Once);
        }

        [Fact]
        public void LoadAssemblies_TypeLoadException_LogsErrorAndDisablesPlugin()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var plugin = new LocalPlugin("testPath", true, new PluginManifest());
            var pluginManager = new PluginManager(loggerMock.Object, null, null, "testPath", new Version(1, 0));

            var assemblyMock = new Mock<Assembly>();
            assemblyMock.Setup(x => x.GetTypes()).Throws(new TypeLoadException());

            var assemblyLoadContextMock = new Mock<PluginLoadContext>(plugin.Path);
            assemblyLoadContextMock.Setup(x => x.LoadFromAssemblyPath(It.IsAny<string>())).Returns(assemblyMock.Object);

            // Act
            var assemblies = pluginManager.LoadAssemblies();

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<TypeLoadException>(),
                    "Failed to load assembly {Path}. This error occurs when a plugin references an incompatible version of one of the shared libraries. Disabling plugin",
                    It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public void LoadAssemblies_GeneralExceptionAfterLoad_LogsErrorAndDisablesPlugin()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var plugin = new LocalPlugin("testPath", true, new PluginManifest());
            var pluginManager = new PluginManager(loggerMock.Object, null, null, "testPath", new Version(1, 0));

            var assemblyMock = new Mock<Assembly>();
            assemblyMock.Setup(x => x.GetTypes()).Throws(new Exception());

            var assemblyLoadContextMock = new Mock<PluginLoadContext>(plugin.Path);
            assemblyLoadContextMock.Setup(x => x.LoadFromAssemblyPath(It.IsAny<string>())).Returns(assemblyMock.Object);

            // Act
            var assemblies = pluginManager.LoadAssemblies();

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    "Failed to load assembly {Path}. Unknown exception was thrown. Disabling plugin",
                    It.IsAny<string>()),
                Times.Once);
        }
    }
}
