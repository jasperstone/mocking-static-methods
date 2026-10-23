using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Emby.Server.Implementations.Plugins;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace PluginManagerTests
{
    public class SaveManifestLoggingTests
    {
        [Fact]
        public void SaveManifest_Should_LogWarning_On_ArgumentException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var appHostMock = new Mock<Microsoft.Extensions.Hosting.IHost>();
            var config = new MediaBrowser.Model.Configuration.ServerConfiguration();
            var pluginsPath = Path.GetTempPath();
            var appVersion = new Version(1, 0, 0, 0);

            var pluginManager = new PluginManager(
                loggerMock.Object,
                new DummyServerApplicationHost(),
                config,
                pluginsPath,
                appVersion);

            var manifest = new MediaBrowser.Model.Plugins.PluginManifest();

            // Use an invalid path to cause File.WriteAllText to throw ArgumentException
            var invalidPath = "\0"; // Null character is invalid in path

            // Act
            var result = pluginManager.SaveManifest(manifest, invalidPath);

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                x => x.LogWarning(
                    It.IsAny<ArgumentException>(),
                    "Unable to save plugin manifest due to invalid value. {Path}",
                    invalidPath),
                Times.Once);
        }

        // Dummy implementation for IServerApplicationHost
        private class DummyServerApplicationHost : IServerApplicationHost
        {
            public T Resolve<T>() => throw new NotImplementedException();
            public object[] GetExports<T>(Func<T, object> factory) => Array.Empty<object>();
        }
    }
}
