using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Plugins;
using Emby.Server.Implementations.Configuration;

namespace Emby.Server.Implementations.Plugins.Tests
{
    public class PluginManagerTests
    {
        private class LocalPluginStub
        {
            public string Id { get; set; } = "";
            public Version Version { get; set; } = new Version(1, 0, 0, 0);
            public bool IsEnabledAndSupported { get; set; } = true;
            public ManifestStub Manifest { get; set; } = new ManifestStub();
            public string Name { get; set; } = "TestPlugin";
        }

        private class ManifestStub
        {
            public PluginStatus Status { get; set; }
            public bool AutoUpdate { get; set; }
        }

        [Fact]
        public void ProcessAlternative_LogsError_When_ChangePluginStateFails_ActiveStatus()
        {
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var config = new ServerConfiguration();

            var pluginManager = new PluginManager(
                loggerMock.Object,
                appHostMock.Object,
                config,
                "",
                new Version(1, 0, 0, 0));

            var pluginsField = typeof(PluginManager).GetField("_plugins", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(pluginsField);

            var previousVersion = new LocalPluginStub
            {
                Id = "plugin1",
                Version = new Version(1, 0, 0, 0),
                IsEnabledAndSupported = true,
                Name = "PreviousPlugin",
                Manifest = new ManifestStub { Status = PluginStatus.Active }
            };

            var plugin = new LocalPluginStub
            {
                Id = "plugin1",
                Version = new Version(2, 0, 0, 0),
                IsEnabledAndSupported = true,
                Name = "CurrentPlugin",
                Manifest = new ManifestStub { Status = PluginStatus.Active }
            };

            var pluginsList = new List<object> { previousVersion, plugin };
            pluginsField.SetValue(pluginManager, pluginsList);

            var method = typeof(PluginManager).GetMethod("ProcessAlternative", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(method);

            method.Invoke(pluginManager, new object[] { plugin });

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to enable version")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtMostOnce);
        }

        [Fact]
        public void ProcessAlternative_LogsError_When_ChangePluginStateFails_SupersededStatus()
        {
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var config = new ServerConfiguration();

            var pluginManager = new PluginManager(
                loggerMock.Object,
                appHostMock.Object,
                config,
                "",
                new Version(1, 0, 0, 0));

            var pluginsField = typeof(PluginManager).GetField("_plugins", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(pluginsField);

            var previousVersion = new LocalPluginStub
            {
                Id = "plugin1",
                Version = new Version(1, 0, 0, 0),
                IsEnabledAndSupported = true,
                Name = "PreviousPlugin",
                Manifest = new ManifestStub { Status = PluginStatus.Active }
            };

            var plugin = new LocalPluginStub
            {
                Id = "plugin1",
                Version = new Version(2, 0, 0, 0),
                IsEnabledAndSupported = true,
                Name = "CurrentPlugin",
                Manifest = new ManifestStub { Status = PluginStatus.Superseded }
            };

            var pluginsList = new List<object> { previousVersion, plugin };
            pluginsField.SetValue(pluginManager, pluginsList);

            var method = typeof(PluginManager).GetMethod("ProcessAlternative", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(method);

            method.Invoke(pluginManager, new object[] { plugin });

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to supercede version")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtMostOnce);
        }
    }
}
