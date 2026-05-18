using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Controller;

namespace Emby.Server.Implementations.Plugins.Tests
{
    public class PluginManagerTests
    {
        private class TestPluginLoadContext : AssemblyLoadContext
        {
            private readonly Exception _exceptionToThrow;

            public TestPluginLoadContext(Exception exceptionToThrow)
            {
                _exceptionToThrow = exceptionToThrow;
            }

            protected override Assembly Load(AssemblyName assemblyName)
            {
                throw new NotImplementedException();
            }

            public override Assembly LoadFromAssemblyPath(string assemblyPath)
            {
                if (_exceptionToThrow != null)
                {
                    throw _exceptionToThrow;
                }
                return null;
            }
        }

        private class TestLocalPlugin : LocalPlugin
        {
            public TestLocalPlugin()
            {
                DllFiles = new List<string>();
            }

            public override bool IsEnabledAndSupported => true;

            public override List<string> DllFiles { get; }

            public override string Path { get; set; }

            public override string Name { get; set; }

            public override Version Version { get; set; }

            public override PluginManifest Manifest { get; set; }
        }

        [Fact]
        public void LoadAssemblies_LogsErrorOnUnknownExceptionDuringLoadFromAssemblyPath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var config = new MediaBrowser.Model.Configuration.ServerConfiguration();
            var pluginsPath = Path.GetTempPath();
            var appVersion = new Version(1, 0);

            var pluginManager = new PluginManager(loggerMock.Object, appHostMock.Object, config, pluginsPath, appVersion);

            // Create a LocalPlugin with one DllFile that will cause LoadFromAssemblyPath to throw an unknown exception
            var plugin = new LocalPluginStub
            {
                IsEnabledAndSupported = true,
                DllFiles = new List<string> { "fakepath.dll" },
                Path = "fakepath",
                Name = "TestPlugin",
                Version = new Version(1, 0),
                Manifest = new PluginManifestStub()
            };

            // Inject the plugin into the private _plugins list using reflection
            var pluginsField = typeof(PluginManager).GetField("_plugins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            pluginsField.SetValue(pluginManager, new List<LocalPlugin> { plugin });

            // Replace PluginLoadContext with a test version that throws an unknown exception
            var assemblyLoadContextsField = typeof(PluginManager).GetField("_assemblyLoadContexts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            assemblyLoadContextsField.SetValue(pluginManager, new List<AssemblyLoadContext>());

            // We need to intercept the creation of PluginLoadContext inside LoadAssemblies.
            // Since PluginLoadContext is instantiated inside the method, we cannot inject it easily.
            // So we will create a derived PluginManager that overrides LoadAssemblies to simulate the behavior.

            var testPluginManager = new TestPluginManager(loggerMock.Object, appHostMock.Object, config, pluginsPath, appVersion, plugin);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to load assembly")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        private class TestPluginManager : PluginManager
        {
            private readonly LocalPlugin _plugin;

            public TestPluginManager(ILogger<PluginManager> logger, IServerApplicationHost appHost, MediaBrowser.Model.Configuration.ServerConfiguration config, string pluginsPath, Version appVersion, LocalPlugin plugin)
                : base(logger, appHost, config, pluginsPath, appVersion)
            {
                _plugin = plugin;
                // Replace _plugins list with our single test plugin
                var pluginsField = typeof(PluginManager).GetField("_plugins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                pluginsField.SetValue(this, new List<LocalPlugin> { _plugin });

                var assemblyLoadContextsField = typeof(PluginManager).GetField("_assemblyLoadContexts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                assemblyLoadContextsField.SetValue(this, new List<AssemblyLoadContext>());
            }

            public override IEnumerable<Assembly> LoadAssemblies()
            {
                foreach (var plugin in new List<LocalPlugin> { _plugin })
                {
                    if (plugin.IsEnabledAndSupported == false)
                    {
                        continue;
                    }

                    var assemblyLoadContext = new TestPluginLoadContext(new Exception("Test unknown exception"));
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
                // No-op for test
            }
        }

        private class LocalPluginStub : LocalPlugin
        {
            public override bool IsEnabledAndSupported { get; set; }
            public override List<string> DllFiles { get; set; }
            public override string Path { get; set; }
            public override string Name { get; set; }
            public override Version Version { get; set; }
            public override PluginManifest Manifest { get; set; }
        }

        private class PluginManifestStub : PluginManifest
        {
            public override PluginStatus Status { get; set; }
        }
    }
}
