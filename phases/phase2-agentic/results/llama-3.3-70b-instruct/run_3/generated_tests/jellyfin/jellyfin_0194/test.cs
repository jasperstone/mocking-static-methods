using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Emby.Server.Implementations.Plugins
{
    public class PluginManagerTests
    {
        [Fact]
        public void ProcessAlternative_LogsError_WhenChangePluginStateFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var pluginManager = new PluginManager(loggerMock.Object, null, null, null, null);
            var plugin = new LocalPlugin
            {
                Id = "id",
                Version = "version",
                Name = "name"
            };
            var previousVersion = new LocalPlugin
            {
                Id = "id",
                Version = "previousVersion",
                Name = "name"
            };
            pluginManager._plugins.Add(plugin);
            pluginManager._plugins.Add(previousVersion);

            // Act
            pluginManager.ProcessAlternative(plugin);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
