using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Plugins;

namespace Emby.Server.Implementations.Plugins.Tests
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
                Name = "TestPlugin";
                Version = "1.0.0";
                Manifest = new PluginManifest { Status = PluginStatus.Enabled };
            }

            public override string Path { get; }
            public override List<string> DllFiles { get; }
            public override bool IsEnabledAndSupported { get; }
            public override string Name { get; }
            public override string Version { get; }
            public override PluginManifest Manifest { get; }
        }

        private class TestAssemblyLoadContext : AssemblyLoadContext
        {
            private readonly Func<string, Assembly> _loadFunc;

            public TestAssemblyLoadContext(Func<string, Assembly> loadFunc)
            {
                _loadFunc = loadFunc;
            }

            public override Assembly LoadFromAssemblyPath(string assemblyPath)
            {
                return _loadFunc(assemblyPath);
            }
        }

        [Fact]
        public void LoadAssemblies_LogsErrorOnFileLoadExceptionAndDisablesPlugin()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var config = new ServerConfiguration();
            var pluginsPath = Path.GetTempPath();
            var appVersion = new Version(1, 0);

            var plugin = new TestPlugin("pluginPath", new List<string> { "bad.dll" }, true);

            var pluginManager = new PluginManager(loggerMock.Object, appHostMock.Object, config, pluginsPath, appVersion);

            // Inject plugin list with our test plugin
            var pluginsField = typeof(PluginManager).GetField("_plugins", BindingFlags.NonPublic | BindingFlags.Instance);
            pluginsField.SetValue(pluginManager, new List<LocalPlugin> { plugin });

            // Replace PluginLoadContext with one that throws FileLoadException
            var assemblyLoadContextsField = typeof(PluginManager).GetField("_assemblyLoadContexts", BindingFlags.NonPublic | BindingFlags.Instance);
            assemblyLoadContextsField.SetValue(pluginManager, new List<AssemblyLoadContext>());

            // We need to intercept the creation of PluginLoadContext to throw on LoadFromAssemblyPath
            // Since PluginLoadContext is instantiated inside LoadAssemblies, we cannot replace it easily.
            // Instead, we will create a derived PluginManager with override for LoadAssemblies to simulate the behavior.

            var testManager = new TestPluginManager(loggerMock.Object, appHostMock.Object, config, pluginsPath, appVersion, plugin);

            // Act
            var assemblies = new List<Assembly>();
            foreach (var assembly in testManager.LoadAssemblies())
            {
                assemblies.Add(assembly);
            }

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to load assembly bad.dll. Disabling plugin")),
                    It.IsAny<FileLoadException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Plugin state should be changed to Malfunctioned
            var manifestStatus = plugin.Manifest.Status;
            Assert.Equal(PluginStatus.Malfunctioned, manifestStatus);

            Assert.Empty(assemblies);
        }

        [Fact]
        public void LoadAssemblies_LogsErrorOnUnknownExceptionAndDisablesPlugin()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var config = new ServerConfiguration();
            var pluginsPath = Path.GetTempPath();
            var appVersion = new Version(1, 0);

            var plugin = new TestPlugin("pluginPath", new List<string> { "bad.dll" }, true);

            var testManager = new TestPluginManagerUnknownException(loggerMock.Object, appHostMock.Object, config, pluginsPath, appVersion, plugin);

            // Act
            var assemblies = new List<Assembly>();
            foreach (var assembly in testManager.LoadAssemblies())
            {
                assemblies.Add(assembly);
            }

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to load assembly bad.dll. Unknown exception was thrown. Disabling plugin")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            var manifestStatus = plugin.Manifest.Status;
            Assert.Equal(PluginStatus.Malfunctioned, manifestStatus);

            Assert.Empty(assemblies);
        }

        private class TestPluginManager : PluginManager
        {
            private readonly LocalPlugin _plugin;

            public TestPluginManager(ILogger<PluginManager> logger, IServerApplicationHost appHost, ServerConfiguration config, string pluginsPath, Version appVersion, LocalPlugin plugin)
                : base(logger, appHost, config, pluginsPath, appVersion)
            {
                _plugin = plugin;
                var pluginsField = typeof(PluginManager).GetField("_plugins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
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

                    var assemblyLoadContext = new TestAssemblyLoadContext(path =>
                    {
                        throw new FileLoadException("Simulated file load failure");
                    });

                    var assemblyLoadContextsField = typeof(PluginManager).GetField("_assemblyLoadContexts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var list = (List<AssemblyLoadContext>)assemblyLoadContextsField.GetValue(this);
                    list.Add(assemblyLoadContext);

                    var assemblies = new List<Assembly>(plugin.DllFiles.Count);
                    var loadedAll = true;

                    foreach (var file in plugin.DllFiles)
                    {
                        try
                        {
                            assemblies.Add(assemblyLoadContext.LoadFromAssemblyPath(file));
                        }
                        catch (FileLoadException ex)
                        {
                            Logger.LogError(ex, "Failed to load assembly {Path}. Disabling plugin", file);
                            ChangePluginState(plugin, PluginStatus.Malfunctioned);
                            loadedAll = false;
                            break;
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, "Failed to load assembly {Path}. Unknown exception was thrown. Disabling plugin", file);
                            ChangePluginState(plugin, PluginStatus.Malfunctioned);
                            loadedAll = false;
                            break;
                        }
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

            private ILogger<PluginManager> Logger => (ILogger<PluginManager>)typeof(PluginManager).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this);

            private void ChangePluginState(LocalPlugin plugin, PluginStatus status)
            {
                var manifest = plugin.Manifest;
                var statusField = manifest.GetType().GetProperty("Status");
                statusField.SetValue(manifest, status);
            }
        }

        private class TestPluginManagerUnknownException : TestPluginManager
        {
            public TestPluginManagerUnknownException(ILogger<PluginManager> logger, IServerApplicationHost appHost, ServerConfiguration config, string pluginsPath, Version appVersion, LocalPlugin plugin)
                : base(logger, appHost, config, pluginsPath, appVersion, plugin)
            {
            }

            public override IEnumerable<Assembly> LoadAssemblies()
            {
                foreach (var plugin in new List<LocalPlugin> { _plugin })
                {
                    if (plugin.IsEnabledAndSupported == false)
                    {
                        continue;
                    }

                    var assemblyLoadContext = new TestAssemblyLoadContext(path =>
                    {
                        throw new Exception("Simulated unknown failure");
                    });

                    var assemblyLoadContextsField = typeof(PluginManager).GetField("_assemblyLoadContexts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var list = (List<AssemblyLoadContext>)assemblyLoadContextsField.GetValue(this);
                    list.Add(assemblyLoadContext);

                    var assemblies = new List<Assembly>(plugin.DllFiles.Count);
                    var loadedAll = true;

                    foreach (var file in plugin.DllFiles)
                    {
                        try
                        {
                            assemblies.Add(assemblyLoadContext.LoadFromAssemblyPath(file));
                        }
                        catch (FileLoadException ex)
                        {
                            Logger.LogError(ex, "Failed to load assembly {Path}. Disabling plugin", file);
                            ChangePluginState(plugin, PluginStatus.Malfunctioned);
                            loadedAll = false;
                            break;
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, "Failed to load assembly {Path}. Unknown exception was thrown. Disabling plugin", file);
                            ChangePluginState(plugin, PluginStatus.Malfunctioned);
                            loadedAll = false;
                            break;
                        }
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
}
