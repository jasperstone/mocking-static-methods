using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations;
using Microsoft.Extensions.Configuration;

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
                IConfiguration startupConfig)
                : base(applicationPaths, loggerFactory, options, startupConfig)
            {
            }

            // Implement abstract method with minimal implementation
            protected override Assembly[] GetAssembliesWithPartsInternal()
            {
                return Array.Empty<Assembly>();
            }

            // Expose the protected CreateInstanceSafe for testing
            public new object CreateInstanceSafe(Type type)
            {
                return base.CreateInstanceSafe(type);
            }

            // Expose the private _creatingInstances list for test setup
            public List<Type> CreatingInstances
            {
                get => (List<Type>)typeof(ApplicationHost)
                    .GetField("_creatingInstances", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(this);
                set => typeof(ApplicationHost)
                    .GetField("_creatingInstances", BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(this, value);
            }

            // Expose the private _pluginManager for mocking
            public PluginManager PluginManager
            {
                get => (PluginManager)typeof(ApplicationHost)
                    .GetField("_pluginManager", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(this);
                set => typeof(ApplicationHost)
                    .GetField("_pluginManager", BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(this, value);
            }
        }

        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Mock<ILogger<ApplicationHost>> _loggerMock;
        private readonly Mock<IServerApplicationPaths> _applicationPathsMock;
        private readonly Mock<IStartupOptions> _startupOptionsMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly TestApplicationHost _host;

        public ApplicationHostTests()
        {
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerMock = new Mock<ILogger<ApplicationHost>>();
            _applicationPathsMock = new Mock<IServerApplicationPaths>();
            _startupOptionsMock = new Mock<IStartupOptions>();
            _configurationMock = new Mock<IConfiguration>();

            _loggerFactoryMock.Setup(f => f.CreateLogger<ApplicationHost>()).Returns(_loggerMock.Object);
            _loggerFactoryMock.Setup(f => f.CreateLogger<PluginManager>()).Returns(Mock.Of<ILogger<PluginManager>>());
            _loggerFactoryMock.Setup(f => f.CreateLogger<DeviceId>()).Returns(Mock.Of<ILogger<DeviceId>>());

            _host = new TestApplicationHost(
                _applicationPathsMock.Object,
                _loggerFactoryMock.Object,
                _startupOptionsMock.Object,
                _configurationMock.Object);
        }

        [Fact]
        public void CreateInstanceSafe_LogsErrorAndThrows_WhenDiLoopDetected()
        {
            // Arrange
            var type = typeof(string);
            var secondType = typeof(int);

            // Setup _creatingInstances to simulate DI loop
            _host.CreatingInstances = new List<Type> { type, secondType };

            // Mock PluginManager and replace private field
            var pluginManagerMock = new Mock<PluginManager>(
                Mock.Of<ILogger<PluginManager>>(),
                _host,
                null,
                null,
                null);
            _host.PluginManager = pluginManagerMock.Object;

            // Act & Assert
            var ex = Assert.Throws<TypeLoadException>(() => _host.CreateInstanceSafe(type));
            Assert.Equal("DI Loop detected", ex.Message);

            // Verify LogError called for DI loop detection message
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("DI Loop detected in the attempted creation of")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Verify LogError called for each entry in _creatingInstances
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Called from:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(_host.CreatingInstances.Count));

            // Verify FailPlugin called on plugin manager
            pluginManagerMock.Verify(pm => pm.FailPlugin(type.Assembly), Times.Once);
        }
    }
}
