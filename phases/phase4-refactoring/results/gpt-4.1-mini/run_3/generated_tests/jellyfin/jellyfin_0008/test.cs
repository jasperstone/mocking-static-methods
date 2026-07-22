using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations;
using MediaBrowser.Common;

namespace Emby.Server.Implementations.Tests
{
    public class ApplicationHostTests
    {
        private class TestApplicationHost : ApplicationHost
        {
            public TestApplicationHost(
                IServerApplicationPaths applicationPaths,
                ILoggerFactory loggerFactory,
                PluginManager pluginManager,
                ILogger<ApplicationHost> logger)
                : base(applicationPaths, loggerFactory, options: null, startupConfig: null)
            {
                // Override the Logger and PluginManager with mocks
                Logger = logger;
                _pluginManager = pluginManager;
            }

            public new ILogger<ApplicationHost> Logger { get; set; }
            public new PluginManager _pluginManager { get; set; }

            // Expose CreateInstanceSafe for testing
            public new object CreateInstanceSafe(Type type) => base.CreateInstanceSafe(type);

            // Implement abstract method
            protected override IEnumerable<Assembly> GetAssembliesWithPartsInternal()
            {
                return Array.Empty<Assembly>();
            }
        }

        private class DummyApplicationPaths : IServerApplicationPaths
        {
            public string CachePath => "cache";
            public string DataPath => "data";
            public string LogPath => "log";
            public string PluginsPath => "plugins";
            public string TempPath => "temp";
            public string AppPath => "app";
            public string SystemPath => "system";
            public string ConfigPath => "config";
            public string MetadataPath => "metadata";
            public string UserDataPath => "userdata";
            public string TranscodingTempPath => "transcodingtemp";
            public string SystemTempPath => "systemtemp";
            public string SystemTempPath2 => "systemtemp2";
            public string SystemTempPath3 => "systemtemp3";
            public string SystemTempPath4 => "systemtemp4";
            public string SystemTempPath5 => "systemtemp5";
            public string SystemTempPath6 => "systemtemp6";
            public string SystemTempPath7 => "systemtemp7";
            public string SystemTempPath8 => "systemtemp8";
            public string SystemTempPath9 => "systemtemp9";
        }

        [Fact]
        public void CreateInstanceSafe_LogsErrorAndThrows_WhenDiLoopDetected()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var pluginManagerMock = new Mock<PluginManager>(
                Mock.Of<ILogger<PluginManager>>(),
                null,
                null,
                null,
                null);

            var appPaths = new DummyApplicationPaths();
            var loggerFactory = LoggerFactory.Create(builder => { });

            var host = new TestApplicationHost(appPaths, loggerFactory, pluginManagerMock.Object, loggerMock.Object);

            var testType = typeof(string);

            // Use reflection to set the private _creatingInstances field to simulate DI loop
            var creatingInstancesField = typeof(ApplicationHost).GetField("_creatingInstances", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            creatingInstancesField.SetValue(host, new List<Type> { testType });

            // Act & Assert
            var ex = Assert.Throws<TypeLoadException>(() => host.CreateInstanceSafe(testType));

            Assert.Equal("DI Loop detected", ex.Message);

            // Verify LogError called for DI loop detection
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("DI Loop detected in the attempted creation of")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Verify LogError called for each entry in _creatingInstances
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Called from:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

            // Verify pluginManager.FailPlugin called
            pluginManagerMock.Verify(pm => pm.FailPlugin(testType.Assembly), Times.Once);
        }

        [Fact]
        public void CreateInstanceSafe_LogsErrorAndReturnsNull_WhenExceptionThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var pluginManagerMock = new Mock<PluginManager>(
                Mock.Of<ILogger<PluginManager>>(),
                null,
                null,
                null,
                null);

            var appPaths = new DummyApplicationPaths();
            var loggerFactory = LoggerFactory.Create(builder => { });

            var host = new TestApplicationHost(appPaths, loggerFactory, pluginManagerMock.Object, loggerMock.Object);

            // Use a type that will cause Activator.CreateInstance to throw (abstract class)
            var abstractType = typeof(AbstractTestClass);

            // Act
            var result = host.CreateInstanceSafe(abstractType);

            // Assert
            Assert.Null(result);

            // Verify LogError called with exception
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error creating")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Verify pluginManager.FailPlugin called
            pluginManagerMock.Verify(pm => pm.FailPlugin(abstractType.Assembly), Times.Once);
        }

        private abstract class AbstractTestClass
        {
        }
    }
}
