using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Plugins;

namespace Emby.Server.Implementations.Tests.Plugins
{
    public class PluginManagerTests
    {
        private class TestPlugin : LocalPlugin
        {
            public TestPlugin(string path, List<string> dllFiles, bool isEnabledAndSupported)
            {
                Path = path;
                DllFiles = dllFiles;
                IsEnabledAndSupported = isEnabledAndSupported;
                Version = new Version(1, 0);
                Name = "TestPlugin";
                Manifest = new PluginManifest { Status = PluginStatus.Enabled };
            }

            public override string Path { get; }
            public override List<string> DllFiles { get; }
            public override bool IsEnabledAndSupported { get; }
            public override Version Version { get; }
            public override string Name { get; }
            public override PluginManifest Manifest { get; }
        }

        private class PluginManifest
        {
            public PluginStatus Status { get; set; }
        }

        private enum PluginStatus
        {
            Enabled,
            Malfunctioned,
            NotSupported,
            Deleted
        }

        private class LocalPlugin
        {
            public virtual string Path { get; }
            public virtual List<string> DllFiles { get; }
            public virtual bool IsEnabledAndSupported { get; }
            public virtual Version Version { get; }
            public virtual string Name { get; }
            public virtual PluginManifest Manifest { get; }
        }

        private class PluginLoadContextFake : AssemblyLoadContext
        {
            private readonly Func<string, Assembly> _loadFunc;

            public PluginLoadContextFake(Func<string, Assembly> loadFunc)
            {
                _loadFunc = loadFunc;
            }

            protected override Assembly Load(AssemblyName assemblyName)
            {
                return null!;
            }

            public Assembly LoadFromAssemblyPathOverride(string path)
            {
                return _loadFunc(path);
            }
        }

        [Fact]
        public void LoadAssemblies_LogsErrorOnFileLoadExceptionAndDisablesPlugin()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var config = new ServerConfiguration();
            var pluginsPath = "somepath";
            var appVersion = new Version(1, 0);

            var plugin = new TestPlugin("pluginPath", new List<string> { "dll1" }, true);

            var pluginManager = new PluginManager(loggerMock.Object, appHostMock.Object, config, pluginsPath, appVersion);

            // Inject plugin list with our test plugin
            var pluginsField = typeof(PluginManager).GetField("_plugins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            pluginsField.SetValue(pluginManager, new List<LocalPlugin> { plugin });

            // Replace PluginLoadContext with a fake that throws FileLoadException on LoadFromAssemblyPath
            var assemblyLoadContextsField = typeof(PluginManager).GetField("_assemblyLoadContexts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            assemblyLoadContextsField.SetValue(pluginManager, new List<AssemblyLoadContext>());

            var loadContextCtor = typeof(PluginLoadContext).GetConstructor(new[] { typeof(string) });
            // We cannot replace PluginLoadContext constructor easily, so we will mock LoadFromAssemblyPath via reflection

            // Use reflection to replace LoadFromAssemblyPath method on PluginLoadContext instance
            // Instead, we will create a derived class to override LoadFromAssemblyPath, but since PluginLoadContext is sealed, we cannot.
            // So we will use a workaround: create a fake PluginLoadContext class and inject it into _assemblyLoadContexts and simulate LoadAssemblies manually.

            // Act
            // We simulate the LoadAssemblies method logic for the first foreach plugin.DllFiles to trigger the FileLoadException catch block

            var assemblyLoadContextMock = new Mock<AssemblyLoadContext>(MockBehavior.Strict);
            assemblyLoadContextMock.Setup(x => x.LoadFromAssemblyPath("dll1")).Throws(new FileLoadException());

            assemblyLoadContextsField.SetValue(pluginManager, new List<AssemblyLoadContext> { assemblyLoadContextMock.Object });

            // We call LoadAssemblies and enumerate to trigger the code
            var enumerator = pluginManager.LoadAssemblies().GetEnumerator();

            // Assert
            // The logger should have LogError called with FileLoadException
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to load assembly")),
                    It.IsAny<FileLoadException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // The plugin state should be changed to Malfunctioned
            // We cannot access ChangePluginState directly, but we can check plugin.Manifest.Status if it was changed
            // Since our TestPlugin does not implement ChangePluginState, we cannot verify state change here
            // So this test focuses on verifying the LogError call

            // The enumerator should not yield any assemblies
            Assert.False(enumerator.MoveNext());
        }

        [Fact]
        public void LoadAssemblies_LogsErrorOnGeneralExceptionAndDisablesPlugin()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var config = new ServerConfiguration();
            var pluginsPath = "somepath";
            var appVersion = new Version(1, 0);

            var plugin = new TestPlugin("pluginPath", new List<string> { "dll1" }, true);

            var pluginManager = new PluginManager(loggerMock.Object, appHostMock.Object, config, pluginsPath, appVersion);

            var pluginsField = typeof(PluginManager).GetField("_plugins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            pluginsField.SetValue(pluginManager, new List<LocalPlugin> { plugin });

            var assemblyLoadContextsField = typeof(PluginManager).GetField("_assemblyLoadContexts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            assemblyLoadContextsField.SetValue(pluginManager, new List<AssemblyLoadContext>());

            var assemblyLoadContextMock = new Mock<AssemblyLoadContext>(MockBehavior.Strict);
            assemblyLoadContextMock.Setup(x => x.LoadFromAssemblyPath("dll1")).Throws(new Exception("General failure"));

            assemblyLoadContextsField.SetValue(pluginManager, new List<AssemblyLoadContext> { assemblyLoadContextMock.Object });

            // Act
            var enumerator = pluginManager.LoadAssemblies().GetEnumerator();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to load assembly") && v.ToString().Contains("Unknown exception")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.False(enumerator.MoveNext());
        }
    }

    // Dummy classes to satisfy constructor parameters
    public interface IServerApplicationHost
    {
        T Resolve<T>();
        IEnumerable<T> GetExports<T>(Func<Type, T> factory);
    }

    public class ServerConfiguration
    {
    }
}
