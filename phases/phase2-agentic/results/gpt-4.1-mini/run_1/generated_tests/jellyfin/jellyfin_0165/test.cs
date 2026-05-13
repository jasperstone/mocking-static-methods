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
        private class TestPluginLoadContext : AssemblyLoadContext
        {
            private readonly Func<string, Assembly> _loadFunc;

            public TestPluginLoadContext(Func<string, Assembly> loadFunc)
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

        private class TestLocalPlugin : LocalPlugin
        {
            public TestLocalPlugin(string path, List<string> dllFiles, bool isEnabledAndSupported)
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

        [Fact]
        public void LoadAssemblies_LogsErrorOnFileLoadExceptionAndDisablesPlugin()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var config = new ServerConfiguration();
            var pluginsPath = "plugins";
            var appVersion = new Version(1, 0);

            var pluginManager = new PluginManager(loggerMock.Object, appHostMock.Object, config, pluginsPath, appVersion);

            var dllFile = "bad.dll";
            var plugin = new TestLocalPlugin("pluginPath", new List<string> { dllFile }, true);

            // Inject plugin into private _plugins list via reflection
            var pluginsField = typeof(PluginManager).GetField("_plugins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            pluginsField.SetValue(pluginManager, new List<LocalPlugin> { plugin });

            // Replace PluginLoadContext with one that throws FileLoadException
            var assemblyLoadContextsField = typeof(PluginManager).GetField("_assemblyLoadContexts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            assemblyLoadContextsField.SetValue(pluginManager, new List<AssemblyLoadContext>());

            // We need to override PluginLoadContext creation to throw FileLoadException on LoadFromAssemblyPath
            // We do this by replacing PluginLoadContext with a custom one via reflection or by mocking PluginLoadContext
            // Since PluginLoadContext is instantiated inside LoadAssemblies, we cannot easily replace it.
            // Instead, we will create a derived PluginManager with overridden LoadAssemblies method for test.

            var testPluginManager = new TestPluginManager(loggerMock.Object, appHostMock.Object, config, pluginsPath, appVersion, plugin, dllFile);

            // Act
            var assemblies = new List<Assembly>();
            foreach (var assembly in testPluginManager.LoadAssemblies())
            {
                assemblies.Add(assembly);
            }

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to load assembly")),
                    It.IsAny<FileLoadException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Plugin state should be changed to Malfunctioned
            var pluginManifest = plugin.Manifest;
            Assert.Equal(PluginStatus.Malfunctioned, pluginManifest.Status);

            Assert.Empty(assemblies);
        }

        [Fact]
        public void LoadAssemblies_LogsErrorOnGeneralExceptionAndDisablesPlugin()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var config = new ServerConfiguration();
            var pluginsPath = "plugins";
            var appVersion = new Version(1, 0);

            var pluginManager = new PluginManager(loggerMock.Object, appHostMock.Object, config, pluginsPath, appVersion);

            var dllFile = "bad.dll";
            var plugin = new TestLocalPlugin("pluginPath", new List<string> { dllFile }, true);

            var pluginsField = typeof(PluginManager).GetField("_plugins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            pluginsField.SetValue(pluginManager, new List<LocalPlugin> { plugin });

            var assemblyLoadContextsField = typeof(PluginManager).GetField("_assemblyLoadContexts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            assemblyLoadContextsField.SetValue(pluginManager, new List<AssemblyLoadContext>());

            var testPluginManager = new TestPluginManagerThrowingGeneralException(loggerMock.Object, appHostMock.Object, config, pluginsPath, appVersion, plugin, dllFile);

            // Act
            var assemblies = new List<Assembly>();
            foreach (var assembly in testPluginManager.LoadAssemblies())
            {
                assemblies.Add(assembly);
            }

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to load assembly")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            var pluginManifest = plugin.Manifest;
            Assert.Equal(PluginStatus.Malfunctioned, pluginManifest.Status);

            Assert.Empty(assemblies);
        }

        private class TestPluginManager : PluginManager
        {
            private readonly LocalPlugin _plugin;
            private readonly string _dllFile;

            public TestPluginManager(ILogger<PluginManager> logger, IServerApplicationHost appHost, ServerConfiguration config, string pluginsPath, Version appVersion, LocalPlugin plugin, string dllFile)
                : base(logger, appHost, config, pluginsPath, appVersion)
            {
                _plugin = plugin;
                _dllFile = dllFile;
                // Override _plugins list
                var pluginsField = typeof(PluginManager).GetField("_plugins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
                pluginsField.SetValue(this, new List<LocalPlugin> { _plugin });
            }

            public override IEnumerable<Assembly> LoadAssemblies()
            {
                foreach (var plugin in new List<LocalPlugin> { _plugin })
                {
                    if (plugin.IsEnabledAndSupported == false)
                    {
                        continue;
                    }

                    var assemblyLoadContext = new PluginLoadContext(plugin.Path);
                    var assemblyLoadContextsField = typeof(PluginManager).GetField("_assemblyLoadContexts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
                    var list = (List<AssemblyLoadContext>)assemblyLoadContextsField.GetValue(this)!;
                    list.Add(assemblyLoadContext);

                    var assemblies = new List<Assembly>(plugin.DllFiles.Count);
                    var loadedAll = true;

                    foreach (var file in plugin.DllFiles)
                    {
                        // Simulate FileLoadException on LoadFromAssemblyPath
                        throw new FileLoadException("Simulated file load failure");
                    }

                    if (!loadedAll)
                    {
                        continue;
                    }

                    foreach (var assembly in assemblies)
                    {
                        yield return assembly;
                    }
                }
            }
        }

        private class TestPluginManagerThrowingGeneralException : PluginManager
        {
            private readonly LocalPlugin _plugin;
            private readonly string _dllFile;

            public TestPluginManagerThrowingGeneralException(ILogger<PluginManager> logger, IServerApplicationHost appHost, ServerConfiguration config, string pluginsPath, Version appVersion, LocalPlugin plugin, string dllFile)
                : base(logger, appHost, config, pluginsPath, appVersion)
            {
                _plugin = plugin;
                _dllFile = dllFile;
                var pluginsField = typeof(PluginManager).GetField("_plugins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
                pluginsField.SetValue(this, new List<LocalPlugin> { _plugin });
            }

            public override IEnumerable<Assembly> LoadAssemblies()
            {
                foreach (var plugin in new List<LocalPlugin> { _plugin })
                {
                    if (plugin.IsEnabledAndSupported == false)
                    {
                        continue;
                    }

                    var assemblyLoadContext = new PluginLoadContext(plugin.Path);
                    var assemblyLoadContextsField = typeof(PluginManager).GetField("_assemblyLoadContexts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
                    var list = (List<AssemblyLoadContext>)assemblyLoadContextsField.GetValue(this)!;
                    list.Add(assemblyLoadContext);

                    var assemblies = new List<Assembly>(plugin.DllFiles.Count);
                    var loadedAll = true;

                    foreach (var file in plugin.DllFiles)
                    {
                        // Simulate general exception on LoadFromAssemblyPath
                        throw new Exception("Simulated general failure");
                    }

                    if (!loadedAll)
                    {
                        continue;
                    }

                    foreach (var assembly in assemblies)
                    {
                        yield return assembly;
                    }
                }
            }
        }
    }

    // Minimal stubs for dependencies to compile
    public class LocalPlugin
    {
        public virtual string Path { get; }
        public virtual List<string> DllFiles { get; }
        public virtual bool IsEnabledAndSupported { get; }
        public virtual Version Version { get; }
        public virtual string Name { get; }
        public virtual PluginManifest Manifest { get; }
    }

    public class PluginManifest
    {
        public PluginStatus Status { get; set; }
    }

    public enum PluginStatus
    {
        Enabled,
        Malfunctioned,
        NotSupported,
        Deleted
    }

    public interface IServerApplicationHost
    {
        T Resolve<T>();
        IEnumerable<T> GetExports<T>(Func<Type, T> factory);
    }

    public class ServerConfiguration { }
}
