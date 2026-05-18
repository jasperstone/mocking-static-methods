using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations;

namespace Emby.Server.Implementations.Tests
{
    public class ApplicationHostTests
    {
        private class TestApplicationHost : ApplicationHost
        {
            public TestApplicationHost(
                IServerApplicationPaths applicationPaths,
                ILoggerFactory loggerFactory,
                IStartupOptions options,
                Microsoft.Extensions.Configuration.IConfiguration startupConfig)
                : base(applicationPaths, loggerFactory, options, startupConfig)
            {
            }

            public new object CreateInstanceSafe(Type type)
            {
                return base.CreateInstanceSafe(type);
            }

            public List<Type> CreatingInstances
            {
                get => _creatingInstances;
                set => _creatingInstances = value;
            }

            public PluginManager PluginManager => _pluginManager;
        }

        private class DummyTypeA { }
        private class DummyTypeB { }

        [Fact]
        public void CreateInstanceSafe_LogsErrorAndThrows_WhenDiLoopDetected()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ApplicationHost>>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(f => f.CreateLogger<ApplicationHost>()).Returns(mockLogger.Object);
            mockLoggerFactory.Setup(f => f.CreateLogger<DeviceId>()).Returns(Mock.Of<ILogger<DeviceId>>());
            var mockAppPaths = new Mock<IServerApplicationPaths>();
            var mockOptions = new Mock<IStartupOptions>();
            var mockConfig = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();

            var host = new TestApplicationHost(mockAppPaths.Object, mockLoggerFactory.Object, mockOptions.Object, mockConfig.Object);

            // Setup _creatingInstances to simulate a DI loop
            var typeA = typeof(DummyTypeA);
            var typeB = typeof(DummyTypeB);
            host.CreatingInstances = new List<Type> { typeA, typeB };

            // Add typeA again to simulate loop detection
            host.CreatingInstances.Add(typeA);

            // We need to access the private _pluginManager to verify FailPlugin is called
            var mockPluginManager = new Mock<PluginManager>(
                Mock.Of<ILogger<PluginManager>>(),
                host,
                null,
                null,
                null);
            // Replace the private _pluginManager field with our mock
            var pluginManagerField = typeof(ApplicationHost).GetField("_pluginManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            pluginManagerField.SetValue(host, mockPluginManager.Object);

            // Act & Assert
            var ex = Assert.Throws<TypeLoadException>(() => host.CreateInstanceSafe(typeA));

            Assert.Equal("DI Loop detected", ex.Message);

            // Verify LogError called for loop detection message
            mockLogger.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("DI Loop detected in the attempted creation of")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Verify LogError called for each entry in _creatingInstances
            mockLogger.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Called from:")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(host.CreatingInstances.Count - 1)); // Because the last add is the duplicate

            // Verify FailPlugin called on plugin manager
            mockPluginManager.Verify(pm => pm.FailPlugin(typeA.Assembly), Times.Once);
        }
    }
}
