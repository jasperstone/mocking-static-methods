using Emby.Server.Implementations.Plugins;
using MediaBrowser.Model.Plugins;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Emby.Server.Implementations.Tests
{
    public class PluginManagerTests
    {
        [Fact]
        public void ProcessAlternative_LogsError_WhenChangePluginStateFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var pluginManager = new PluginManager(loggerMock.Object, null, null, string.Empty, new Version(1, 0, 0, 0));
            var plugin = new LocalPlugin { Id = "id", Version = new Version(1, 0, 0, 0), Name = "name", Manifest = new PluginManifest { Status = PluginStatus.Active } };
            var previousVersion = new LocalPlugin { Id = "id", Version = new Version(1, 0, 0, 0), Name = "name", Manifest = new PluginManifest { Status = PluginStatus.Active } };

            var fieldInfo = typeof(PluginManager).GetField("_plugins", BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfo.SetValue(pluginManager, new List<LocalPlugin> { plugin, previousVersion });

            // Act
            var methodInfo = typeof(PluginManager).GetMethod("ProcessAlternative", BindingFlags.NonPublic | BindingFlags.Instance);
            methodInfo.Invoke(pluginManager, new object[] { plugin });

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }

    public class LocalPlugin
    {
        public string Id { get; set; }
        public Version Version { get; set; }
        public string Name { get; set; }
        public PluginManifest Manifest { get; set; }
    }

    public class PluginManifest
    {
        public PluginStatus Status { get; set; }
    }

    public enum PluginStatus
    {
        Active,
        Disabled,
        Malfunctioned,
        NotSupported,
        Deleted,
        Superseded,
        Restart
    }
}
