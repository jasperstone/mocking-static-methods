using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.Extensions.Configuration;

namespace Emby.Server.Implementations.Tests
{
    // Minimal interface definitions to allow compilation of the test
    public interface IServerApplicationPaths
    {
        string VirtualDataPath { get; }
        string DataPath { get; }
        string VirtualInternalMetadataPath { get; }
        string InternalMetadataPath { get; }
        string PluginsPath { get; }
    }

    public interface IStartupOptions { }

    // Minimal stub for PluginManager to allow compilation
    public class PluginManager : IDisposable
    {
        public PluginManager(ILogger<PluginManager> logger, object host, IConfiguration config, string pluginsPath, Version version) { }
        public void FailPlugin(Assembly assembly) { }
        public void Dispose() { }
    }

    // Minimal stub for DeviceId to allow compilation
    public class DeviceId
    {
        public DeviceId(IServerApplicationPaths paths, ILogger<DeviceId> logger) { }
    }

    // Minimal stub for MyXmlSerializer to allow compilation
    public class MyXmlSerializer : IXmlSerializer
    {
        public T Deserialize<T>(string input) => default;
        public string Serialize<T>(T obj) => string.Empty;
    }

    // Minimal stub for ServerConfigurationManager to allow compilation
    public class ServerConfigurationManager
    {
        public ServerConfigurationManager(IServerApplicationPaths paths, ILoggerFactory loggerFactory, IXmlSerializer serializer) { }
        public IConfiguration Configuration => new ConfigurationBuilder().Build();
    }

    // Minimal stub for IXmlSerializer to allow compilation
    public interface IXmlSerializer
    {
        T Deserialize<T>(string input);
        string Serialize<T>(T obj);
    }

    // We replicate the ApplicationHost class with only the relevant parts for testing
    public abstract class ApplicationHost
    {
        protected List<Type> _creatingInstances;
        protected PluginManager _pluginManager;
        protected ILogger<ApplicationHost> Logger;
        protected IServerApplicationPaths ApplicationPaths;
        protected ILoggerFactory LoggerFactory;
        protected ServerConfigurationManager ConfigurationManager;

        protected ApplicationHost(
            IServerApplicationPaths applicationPaths,
            ILoggerFactory loggerFactory,
            IStartupOptions options,
            IConfiguration startupConfig)
        {
            ApplicationPaths = applicationPaths;
            LoggerFactory = loggerFactory;
            Logger = LoggerFactory.CreateLogger<ApplicationHost>();
            ConfigurationManager = new ServerConfigurationManager(ApplicationPaths, LoggerFactory, new MyXmlSerializer());
            _pluginManager = new PluginManager(LoggerFactory.CreateLogger<PluginManager>(), this, ConfigurationManager.Configuration, ApplicationPaths.PluginsPath, new Version(1, 0));
        }

        protected abstract IEnumerable<Assembly> GetAssembliesWithPartsInternal();

        public object CreateInstanceSafe(Type type)
        {
            _creatingInstances ??= new List<Type>();

            if (_creatingInstances.Contains(type))
            {
                Logger.LogError("DI Loop detected in the attempted creation of {Type}", type.FullName);
                foreach (var entry in _creatingInstances)
                {
                    Logger.LogError("Called from: {TypeName}", entry.FullName);
                }

                _pluginManager.FailPlugin(type.Assembly);

                throw new TypeLoadException("DI Loop detected");
            }

            try
            {
                _creatingInstances.Add(type);
                Logger.LogDebug("Creating instance of {Type}", type);
                return Activator.CreateInstance(type);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error creating {Type}", type);
                _pluginManager.FailPlugin(type.Assembly);
                return null;
            }
            finally
            {
                _creatingInstances.Remove(type);
            }
        }
    }

    public class ApplicationHostTests
    {
        private class TestApplicationHost : ApplicationHost
        {
            public TestApplicationHost(
                IServerApplicationPaths applicationPaths,
                ILoggerFactory loggerFactory,
                IStartupOptions options,
                IConfiguration startupConfig)
                : base(applicationPaths, loggerFactory, options, startupConfig)
            {
            }

            protected override IEnumerable<Assembly> GetAssembliesWithPartsInternal()
            {
                return Array.Empty<Assembly>();
            }

            public List<Type> CreatingInstances
            {
                get => _creatingInstances;
                set => _creatingInstances = value;
            }
        }

        [Fact]
        public void CreateInstanceSafe_LogsErrorAndThrows_WhenDiLoopDetected()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ApplicationHost>>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(f => f.CreateLogger<ApplicationHost>()).Returns(mockLogger.Object);
            mockLoggerFactory.Setup(f => f.CreateLogger<DeviceId>()).Returns(Mock.Of<ILogger<DeviceId>>());
            mockLoggerFactory.Setup(f => f.CreateLogger<PluginManager>()).Returns(Mock.Of<ILogger<PluginManager>>());

            var mockAppPaths = new Mock<IServerApplicationPaths>();
            mockAppPaths.SetupGet(p => p.VirtualDataPath).Returns("virtualData");
            mockAppPaths.SetupGet(p => p.DataPath).Returns("data");
            mockAppPaths.SetupGet(p => p.VirtualInternalMetadataPath).Returns("virtualInternal");
            mockAppPaths.SetupGet(p => p.InternalMetadataPath).Returns("internal");
            mockAppPaths.SetupGet(p => p.PluginsPath).Returns("pluginsPath");

            var mockStartupOptions = new Mock<IStartupOptions>();
            var mockConfig = new Mock<IConfiguration>();

            var host = new TestApplicationHost(mockAppPaths.Object, mockLoggerFactory.Object, mockStartupOptions.Object, mockConfig.Object);

            var type = typeof(string); // any type
            host.CreatingInstances = new List<Type> { type };

            // Act & Assert
            var ex = Assert.Throws<TypeLoadException>(() => host.CreateInstanceSafe(type));

            // Verify that LogError was called for the DI loop detection message
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("DI Loop detected in the attempted creation of")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Verify that LogError was called for each entry in _creatingInstances
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Called from:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

            Assert.Equal("DI Loop detected", ex.Message);
        }
    }
}
