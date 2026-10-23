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
    // Minimal stub for LocalPlugin to use in tests
    public class StubLocalPlugin : LocalPluginBase
    {
        public StubLocalPlugin(string path, string name, Version version, List<string> dllFiles, bool isEnabledAndSupported, PluginStatus status)
            : base(path, name, version, dllFiles, status)
        {
            IsEnabledAndSupported = isEnabledAndSupported;
        }

        public override bool IsEnabledAndSupported { get; }
    }

    // Base class to simulate LocalPlugin properties used in PluginManager
    public abstract class LocalPluginBase
    {
        public LocalPluginBase(string path, string name, Version version, List<string> dllFiles, PluginStatus status)
        {
            Path = path;
            Name = name;
            Version = version;
            DllFiles = dllFiles;
            Manifest = new PluginManifest { Status = status };
        }

        public string Path { get; }
        public string Name { get; }
        public Version Version { get; }
        public List<string> DllFiles { get; }
        public PluginManifest Manifest { get; }
        public abstract bool IsEnabledAndSupported { get; }
    }

    public class PluginManifest
    {
        public PluginStatus Status { get; set; }
    }

    public enum PluginStatus
    {
        Active,
        Deleted,
        Malfunctioned,
        NotSupported
    }

    public class PluginManagerTests
    {
        [Fact]
        public void LoadAssemblies_LogsErrorOnFileLoadException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var config = new ServerConfiguration();
            var pluginsPath = Path.GetTempPath();
            var appVersion = new Version(1, 0);

            var dllFiles = new List<string> { "file1.dll" };

            var plugin = new StubLocalPlugin(pluginsPath, "TestPlugin", new Version(1, 0), dllFiles, true, PluginStatus.Active);

            var pluginManager = new PluginManager(loggerMock.Object, appHostMock.Object, config, pluginsPath, appVersion);

            // Inject the plugin manually
            var pluginsField = typeof(PluginManager).GetField("_plugins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            pluginsField.SetValue(pluginManager, new List<LocalPluginBase> { plugin });

            // Replace _assemblyLoadContexts with a custom context that throws FileLoadException
            var assemblyLoadContextsField = typeof(PluginManager).GetField("_assemblyLoadContexts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            assemblyLoadContextsField.SetValue(pluginManager, new List<AssemblyLoadContext>());

            // We cannot override LoadFromAssemblyPath, so override Load to throw instead
            var assemblyLoadContext = new ThrowingPluginLoadContext(dllFiles[0], throwFileLoadException: true);
            if (assemblyLoadContextsField.GetValue(pluginManager) is List<AssemblyLoadContext> list)
            {
                list.Add(assemblyLoadContext);
            }

            // Act
            var assemblies = pluginManager.LoadAssemblies();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to load assembly")),
                    It.IsAny<FileLoadException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LoadAssemblies_LogsErrorOnGeneralException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var config = new ServerConfiguration();
            var pluginsPath = Path.GetTempPath();
            var appVersion = new Version(1, 0);

            var dllFiles = new List<string> { "file2.dll" };

            var plugin = new StubLocalPlugin(pluginsPath, "TestPlugin", new Version(1, 0), dllFiles, true, PluginStatus.Active);

            var pluginManager = new PluginManager(loggerMock.Object, appHostMock.Object, config, pluginsPath, appVersion);

            // Inject the plugin manually
            var pluginsField = typeof(PluginManager).GetField("_plugins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            pluginsField.SetValue(pluginManager, new List<LocalPluginBase> { plugin });

            // Replace _assemblyLoadContexts with a custom context that throws general Exception
            var assemblyLoadContextsField = typeof(PluginManager).GetField("_assemblyLoadContexts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            assemblyLoadContextsField.SetValue(pluginManager, new List<AssemblyLoadContext>());

            var assemblyLoadContext = new ThrowingPluginLoadContext(dllFiles[0], throwFileLoadException: false);
            if (assemblyLoadContextsField.GetValue(pluginManager) is List<AssemblyLoadContext> list)
            {
                list.Add(assemblyLoadContext);
            }

            // Act
            var assemblies = pluginManager.LoadAssemblies();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to load assembly")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Custom PluginLoadContext that throws exceptions on Load
        private class ThrowingPluginLoadContext : AssemblyLoadContext
        {
            private readonly string _path;
            private readonly bool _throwFileLoadException;

            public ThrowingPluginLoadContext(string path, bool throwFileLoadException) : base(true)
            {
                _path = path;
                _throwFileLoadException = throwFileLoadException;
            }

            protected override Assembly Load(AssemblyName assemblyName)
            {
                if (_throwFileLoadException)
                {
                    throw new FileLoadException("Simulated FileLoadException");
                }
                else
                {
                    throw new Exception("Simulated general exception");
                }
            }
        }
    }
}
