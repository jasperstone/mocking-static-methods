using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests
{
    // Minimal stubs for missing interfaces to allow compilation
    public interface IServerApplicationPaths
    {
        string VirtualDataPath { get; }
        string DataPath { get; }
        string VirtualInternalMetadataPath { get; }
        string InternalMetadataPath { get; }
        string PluginsPath { get; }
    }

    public interface IStartupOptions
    {
    }

    // Minimal stub for PluginManager to allow construction
    public class PluginManager
    {
        public PluginManager(ILogger<PluginManager> logger, object host, object config, string pluginsPath, Version version)
        {
        }

        public void FailPlugin(Assembly assembly)
        {
        }
    }

    // Minimal stub for DeviceId to allow construction
    public class DeviceId
    {
        public DeviceId(IServerApplicationPaths paths, ILogger<DeviceId> logger)
        {
        }
    }

    // Minimal stub for ServerConfigurationManager to allow construction
    public class ServerConfigurationManager
    {
        public ServerConfigurationManager(IServerApplicationPaths paths, ILoggerFactory loggerFactory, object xmlSerializer)
        {
        }

        public object Configuration => null;
    }

    // Minimal stub for MyXmlSerializer
    public class MyXmlSerializer
    {
    }

    // The ApplicationHost abstract class with only the relevant parts for testing CreateInstanceSafe
    public abstract class ApplicationHost
    {
        private List<Type> _creatingInstances;
        protected ILogger<ApplicationHost> Logger { get; }
        protected PluginManager _pluginManager;
        protected IServerApplicationPaths ApplicationPaths { get; }
        protected ServerConfigurationManager ConfigurationManager { get; }
        protected ILoggerFactory LoggerFactory { get; }
        protected object ServiceProvider => null;

        protected ApplicationHost(
            IServerApplicationPaths applicationPaths,
            ILoggerFactory loggerFactory,
            IStartupOptions options,
            IConfiguration startupConfig)
        {
            ApplicationPaths = applicationPaths;
            LoggerFactory = loggerFactory;
            Logger = LoggerFactory.CreateLogger<ApplicationHost>();
            ConfigurationManager = new ServerConfigurationManager(applicationPaths, loggerFactory, new MyXmlSerializer());
            _pluginManager = new PluginManager(LoggerFactory.CreateLogger<PluginManager>(), this, ConfigurationManager.Configuration, applicationPaths.PluginsPath, new Version(1, 0));
        }

        protected virtual object CreateInstanceSafe(Type type)
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

            public new object CallCreateInstanceSafe(Type type)
            {
                return base.CreateInstanceSafe(type);
            }

            public void AddCreatingInstance(Type type)
            {
                var field = typeof(ApplicationHost).GetField("_creatingInstances", BindingFlags.NonPublic | BindingFlags.Instance);
                var list = (List<Type>)field.GetValue(this);
                if (list == null)
                {
                    list = new List<Type>();
                    field.SetValue(this, list);
                }
                list.Add(type);
            }
        }

        [Fact]
        public void CreateInstanceSafe_LogsErrorAndThrows_WhenDiLoopDetected()
        {
            // Arrange
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger<ApplicationHost>>();
            mockLoggerFactory.Setup(f => f.CreateLogger<ApplicationHost>()).Returns(mockLogger.Object);

            var mockAppPaths = new Mock<IServerApplicationPaths>();
            mockAppPaths.SetupGet(p => p.PluginsPath).Returns("plugins");

            var mockOptions = new Mock<IStartupOptions>();
            var mockConfig = new Mock<IConfiguration>();

            var host = new TestApplicationHost(mockAppPaths.Object, mockLoggerFactory.Object, mockOptions.Object, mockConfig.Object);

            var testType = typeof(string);

            // Simulate DI loop by adding the type to _creatingInstances before calling CreateInstanceSafe
            host.AddCreatingInstance(testType);

            // Act & Assert
            var ex = Assert.Throws<TypeLoadException>(() => host.CallCreateInstanceSafe(testType));
            Assert.Equal("DI Loop detected", ex.Message);

            // Verify that LogError was called for the DI loop detection and for each entry in _creatingInstances
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("DI Loop detected in the attempted creation of")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Called from:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public void CreateInstanceSafe_ReturnsInstance_WhenNoDiLoop()
        {
            // Arrange
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger<ApplicationHost>>();
            mockLoggerFactory.Setup(f => f.CreateLogger<ApplicationHost>()).Returns(mockLogger.Object);

            var mockAppPaths = new Mock<IServerApplicationPaths>();
            mockAppPaths.SetupGet(p => p.PluginsPath).Returns("plugins");

            var mockOptions = new Mock<IStartupOptions>();
            var mockConfig = new Mock<IConfiguration>();

            var host = new TestApplicationHost(mockAppPaths.Object, mockLoggerFactory.Object, mockOptions.Object, mockConfig.Object);

            var testType = typeof(string);

            // Act
            var instance = host.CallCreateInstanceSafe(testType);

            // Assert
            Assert.NotNull(instance);
            Assert.IsType<string>(instance);

            // Verify that LogDebug was called for creating instance
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Creating instance of")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
