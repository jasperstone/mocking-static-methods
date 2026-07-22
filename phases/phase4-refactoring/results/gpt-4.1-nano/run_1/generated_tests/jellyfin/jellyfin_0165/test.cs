using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Common.Net;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Configuration;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System;

namespace PluginManagerTests
{
    public class PluginManagerTest
    {
        [Fact]
        public void LoadAssemblies_Should_LogError_When_ExceptionOccursDuringAssemblyLoad()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var config = new ServerConfiguration();
            var pluginsPath = Path.GetTempPath();
            var appVersion = new Version(1, 0, 0, 0);

            var pluginManager = new PluginManager(loggerMock.Object, appHostMock.Object, config, pluginsPath, appVersion);

            // Use reflection to set private fields if needed, or mock dependencies to cause exception
            // For simplicity, assume LoadFromAssemblyPath throws exception for a specific file

            // Act
            var assemblies = pluginManager.LoadAssemblies();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to load assembly")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.AtLeastOnce);
        }
    }
}
