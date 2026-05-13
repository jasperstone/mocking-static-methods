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
            var pluginsPath = "somepath";
            var appVersion = new Version(1, 0);

            var pluginManager = new PluginManager(loggerMock.Object, appHostMock.Object, config, pluginsPath, appVersion);

            var dllFile = "bad.dll";
            var plugin = new TestLocalPlugin("pluginPath", new List<string> { dllFile }, true);

            // Inject plugin manually
            var pluginsField = typeof(PluginManager).GetField("_plugins", BindingFlags.NonPublic | BindingFlags.Instance)!;
            pluginsField.SetValue(pluginManager, new List<LocalPlugin> { plugin });

            // Replace PluginLoadContext with one that throws FileLoadException
            var assemblyLoadContextsField = typeof(PluginManager).GetField("_assemblyLoadContexts", BindingFlags.NonPublic | BindingFlags.Instance)!;
            assemblyLoadContextsField.SetValue(pluginManager, new List<AssemblyLoadContext>());

            // We need to intercept the creation of PluginLoadContext, so we use a derived class with override
            // But since PluginLoadContext is instantiated directly in LoadAssemblies, we cannot replace it easily.
            // So we simulate by replacing the LoadFromAssemblyPath method via reflection or by mocking PluginLoadContext.
            // Instead, we will simulate by creating a derived PluginLoadContext that throws on LoadFromAssemblyPath.

            var loadContext = new TestPluginLoadContext(path =>
            {
                throw new FileLoadException("Failed to load assembly");
            });

            // Replace _assemblyLoadContexts with our test load context
            var assemblyLoadContexts = new List<AssemblyLoadContext> { loadContext };
            assemblyLoadContextsField.SetValue(pluginManager, assemblyLoadContexts);

            // We will patch the PluginLoadContext constructor call by replacing the field _assemblyLoadContexts and plugin.DllFiles to cause the exception.

            // Act
            var enumerator = pluginManager.LoadAssemblies().GetEnumerator();
            var hasAny = enumerator.MoveNext();

            // Assert
            Assert.False(hasAny);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to load assembly")),
                    It.IsAny<FileLoadException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Plugin state should be changed to Malfunctioned
            var plugins = (List<LocalPlugin>)pluginsField.GetValue(pluginManager)!;
            Assert.Equal(PluginStatus.Malfunctioned, plugins[0].Manifest.Status);
        }

        [Fact]
        public void LoadAssemblies_LogsErrorOnUnknownExceptionAndDisablesPlugin()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var config = new ServerConfiguration();
            var pluginsPath = "somepath";
            var appVersion = new Version(1, 0);

            var pluginManager = new PluginManager(loggerMock.Object, appHostMock.Object, config, pluginsPath, appVersion);

            var dllFile = "bad.dll";
            var plugin = new TestLocalPlugin("pluginPath", new List<string> { dllFile }, true);

            var pluginsField = typeof(PluginManager).GetField("_plugins", BindingFlags.NonPublic | BindingFlags.Instance)!;
            pluginsField.SetValue(pluginManager, new List<LocalPlugin> { plugin });

            var assemblyLoadContextsField = typeof(PluginManager).GetField("_assemblyLoadContexts", BindingFlags.NonPublic | BindingFlags.Instance)!;
            assemblyLoadContextsField.SetValue(pluginManager, new List<AssemblyLoadContext>());

            var loadContext = new TestPluginLoadContext(path =>
            {
                throw new Exception("Unknown exception");
            });

            var assemblyLoadContexts = new List<AssemblyLoadContext> { loadContext };
            assemblyLoadContextsField.SetValue(pluginManager, assemblyLoadContexts);

            // Act
            var enumerator = pluginManager.LoadAssemblies().GetEnumerator();
            var hasAny = enumerator.MoveNext();

            // Assert
            Assert.False(hasAny);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unknown exception was thrown")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            var plugins = (List<LocalPlugin>)pluginsField.GetValue(pluginManager)!;
            Assert.Equal(PluginStatus.Malfunctioned, plugins[0].Manifest.Status);
        }

        [Fact]
        public void LoadAssemblies_LogsErrorOnTypeLoadExceptionAndDisablesPlugin()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var config = new ServerConfiguration();
            var pluginsPath = "somepath";
            var appVersion = new Version(1, 0);

            var pluginManager = new PluginManager(loggerMock.Object, appHostMock.Object, config, pluginsPath, appVersion);

            var dllFile = "good.dll";
            var plugin = new TestLocalPlugin("pluginPath", new List<string> { dllFile }, true);

            var pluginsField = typeof(PluginManager).GetField("_plugins", BindingFlags.NonPublic | BindingFlags.Instance)!;
            pluginsField.SetValue(pluginManager, new List<LocalPlugin> { plugin });

            var assemblyLoadContextsField = typeof(PluginManager).GetField("_assemblyLoadContexts", BindingFlags.NonPublic | BindingFlags.Instance)!;
            assemblyLoadContextsField.SetValue(pluginManager, new List<AssemblyLoadContext>());

            var assembly = new Mock<Assembly>();
            assembly.Setup(a => a.Location).Returns("assemblyLocation");
            assembly.Setup(a => a.GetTypes()).Throws(new TypeLoadException("Type load failure"));

            var loadContext = new TestPluginLoadContext(path => assembly.Object);
            var assemblyLoadContexts = new List<AssemblyLoadContext> { loadContext };
            assemblyLoadContextsField.SetValue(pluginManager, assemblyLoadContexts);

            // Act
            var enumerator = pluginManager.LoadAssemblies().GetEnumerator();
            var hasAny = enumerator.MoveNext();

            // Assert
            Assert.False(hasAny);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to load assembly")),
                    It.IsAny<TypeLoadException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            var plugins = (List<LocalPlugin>)pluginsField.GetValue(pluginManager)!;
            Assert.Equal(PluginStatus.NotSupported, plugins[0].Manifest.Status);
        }

        [Fact]
        public void LoadAssemblies_LogsErrorOnUnknownExceptionDuringGetTypesAndDisablesPlugin()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var config = new ServerConfiguration();
            var pluginsPath = "somepath";
            var appVersion = new Version(1, 0);

            var pluginManager = new PluginManager(loggerMock.Object, appHostMock.Object, config, pluginsPath, appVersion);

            var dllFile = "good.dll";
            var plugin = new TestLocalPlugin("pluginPath", new List<string> { dllFile }, true);

            var pluginsField = typeof(PluginManager).GetField("_plugins", BindingFlags.NonPublic | BindingFlags.Instance)!;
            pluginsField.SetValue(pluginManager, new List<LocalPlugin> { plugin });

            var assemblyLoadContextsField = typeof(PluginManager).GetField("_assemblyLoadContexts", BindingFlags.NonPublic | BindingFlags.Instance)!;
            assemblyLoadContextsField.SetValue(pluginManager, new List<AssemblyLoadContext>());

            var assembly = new Mock<Assembly>();
            assembly.Setup(a => a.Location).Returns("assemblyLocation");
            assembly.Setup(a => a.GetTypes()).Throws(new Exception("Unknown exception"));

            var loadContext = new TestPluginLoadContext(path => assembly.Object);
            var assemblyLoadContexts = new List<AssemblyLoadContext> { loadContext };
            assemblyLoadContextsField.SetValue(pluginManager, assemblyLoadContexts);

            // Act
            var enumerator = pluginManager.LoadAssemblies().GetEnumerator();
            var hasAny = enumerator.MoveNext();

            // Assert
            Assert.False(hasAny);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unknown exception was thrown")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            var plugins = (List<LocalPlugin>)pluginsField.GetValue(pluginManager)!;
            Assert.Equal(PluginStatus.Malfunctioned, plugins[0].Manifest.Status);
        }
    }
}
