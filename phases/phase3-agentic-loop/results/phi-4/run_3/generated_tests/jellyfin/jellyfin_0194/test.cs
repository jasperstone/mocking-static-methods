using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Emby.Server.Implementations.Plugins.Tests
{
    public class PluginManagerTests : IDisposable
    {
        private readonly Mock<ILogger<PluginManager>> _loggerMock;
        private readonly PluginManager _pluginManager;
        private readonly Func<LocalPlugin, PluginStatus, bool> _changePluginStateDelegate;

        public PluginManagerTests()
        {
            _loggerMock = new Mock<ILogger<PluginManager>>();
            _pluginManager = new PluginManager(_loggerMock.Object, null, null, "", new Version(1, 0, 0));
            _changePluginStateDelegate = (plugin, status) => true; // Default to true for successful state change

            // Use reflection to replace the private method with our delegate
            var changePluginStateMethod = typeof(PluginManager)
                .GetMethod("ChangePluginState", BindingFlags.NonPublic | BindingFlags.Instance);

            var originalMethod = changePluginStateMethod.CreateDelegate(typeof(Func<LocalPlugin, PluginStatus, bool>), _pluginManager);
            var dynamicMethod = new DynamicMethod(
                "ChangePluginState",
                typeof(bool),
                new Type[] { typeof(LocalPlugin), typeof(PluginStatus) },
                typeof(PluginManagerTests));

            var ilGenerator = dynamicMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Ldarg_2);
            ilGenerator.EmitCall(OpCodes.Call, originalMethod, null);
            ilGenerator.Emit(OpCodes.Ret);

            var newDelegate = (Func<LocalPlugin, PluginStatus, bool>)dynamicMethod.CreateDelegate(typeof(Func<LocalPlugin, PluginStatus, bool>));
            var changePluginStateField = typeof(PluginManager)
                .GetField("_changePluginState", BindingFlags.NonPublic | BindingFlags.Instance);
            changePluginStateField.SetValue(_pluginManager, newDelegate);
        }

        [Fact]
        public void ProcessAlternative_LogsError_WhenUnableToEnablePreviousVersion()
        {
            // Arrange
            var plugin = new LocalPlugin
            {
                Id = "plugin1",
                Version = new Version(2, 0, 0),
                Manifest = new PluginManifest { Status = PluginStatus.Active },
                Name = "Test Plugin"
            };

            var previousVersion = new LocalPlugin
            {
                Id = "plugin1",
                Version = new Version(1, 0, 0),
                IsEnabledAndSupported = true,
                Name = "Test Plugin"
            };

            var pluginsField = typeof(PluginManager).GetField("_plugins", BindingFlags.NonPublic | BindingFlags.Instance);
            pluginsField.SetValue(_pluginManager, new List<LocalPlugin> { previousVersion });

            _changePluginStateDelegate = (p, s) => false; // Simulate failure

            // Act
            typeof(PluginManager)
                .GetMethod("ProcessAlternative", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(_pluginManager, new object[] { plugin });

            // Assert
            _loggerMock.Verify(
                logger => logger.LogError(
                    It.Is<string>(s => s.Contains("Unable to enable version {Version} of {Name}")),
                    previousVersion.Version,
                    previousVersion.Name),
                Times.Once);
        }

        [Fact]
        public void ProcessAlternative_LogsError_WhenUnableToSupersedePreviousVersion()
        {
            // Arrange
            var plugin = new LocalPlugin
            {
                Id = "plugin1",
                Version = new Version(2, 0, 0),
                Manifest = new PluginManifest { Status = PluginStatus.Superseded },
                Name = "Test Plugin"
            };

            var previousVersion = new LocalPlugin
            {
                Id = "plugin1",
                Version = new Version(1, 0, 0),
                IsEnabledAndSupported = true,
                Name = "Test Plugin"
            };

            var pluginsField = typeof(PluginManager).GetField("_plugins", BindingFlags.NonPublic | BindingFlags.Instance);
            pluginsField.SetValue(_pluginManager, new List<LocalPlugin> { previousVersion });

            _changePluginStateDelegate = (p, s) => false; // Simulate failure

            // Act
            typeof(PluginManager)
                .GetMethod("ProcessAlternative", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(_pluginManager, new object[] { plugin });

            // Assert
            _loggerMock.Verify(
                logger => logger.LogError(
                    It.Is<string>(s => s.Contains("Unable to supercede version {Version} of {Name}")),
                    previousVersion.Version,
                    previousVersion.Name),
                Times.Once);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _pluginManager?.Dispose();
            }
        }
    }

    // Mock classes to represent missing types
    public class LocalPlugin
    {
        public string Id { get; set; }
        public Version Version { get; set; }
        public bool IsEnabledAndSupported { get; set; }
        public PluginManifest Manifest { get; set; }
        public string Name { get; set; }
        public List<string> DllFiles => new List<string>();
    }

    public class PluginManifest
    {
        public PluginStatus Status { get; set; }
        public bool AutoUpdate { get; set; }
    }

    public enum PluginStatus
    {
        Active,
        Superseded,
        Restart
    }
}
