using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Emby.Server.Implementations.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MediaBrowser.Model.Plugins;

namespace Emby.Server.Implementations.Plugins.Tests
{
    public class PluginManagerTests
    {
        [Fact]
        public void LoadAssemblies_UnknownException_LogsErrorAndDisablesPlugin()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var pluginManager = new PluginManager(loggerMock.Object, null, null, "pluginsPath", new Version(1, 0, 0));

            var pluginMock = new Mock<LocalPlugin>();
            pluginMock.Setup(p => p.IsEnabledAndSupported).Returns(true);
            pluginMock.Setup(p => p.Path).Returns("pluginPath");
            pluginMock.Setup(p => p.DllFiles).Returns(new List<string> { "file.dll" });

            var assemblyLoadContextMock = new Mock<PluginLoadContext>(pluginMock.Object.Path);
            assemblyLoadContextMock.Setup(x => x.LoadFromAssemblyPath(It.IsAny<string>())).Throws(new Exception("Unknown exception"));

            // Act
            var assemblies = pluginManager.LoadAssemblies();

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object>()),
                Times.Once);

            Assert.Empty(assemblies);
        }
    }
}
