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
                Microsoft.Extensions.Configuration.IConfiguration startupConfig,
                PluginManager pluginManager)
                : base(applicationPaths, loggerFactory, options, startupConfig)
            {
                _pluginManager = pluginManager;
            }

            public new object CreateInstanceSafe(Type type)
            {
                return base.CreateInstanceSafe(type);
            }

            public List<Type> CreatingInstances => _creatingInstances;

            public PluginManager PluginManager => _pluginManager;
        }

        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Mock<ILogger<ApplicationHost>> _loggerMock;
        private readonly Mock<PluginManager> _pluginManagerMock;
        private readonly Mock<IServerApplicationPaths> _applicationPathsMock;
        private readonly Mock<IStartupOptions> _startupOptionsMock;
        private readonly Mock<Microsoft.Extensions.Configuration.IConfiguration> _configurationMock;

        public ApplicationHostTests()
        {
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerMock = new Mock<ILogger<ApplicationHost>>();
            _pluginManagerMock = new Mock<PluginManager>(
                Mock.Of<ILogger<PluginManager>>(),
                Mock.Of<IServerApplicationHost>(),
                null,
                string.Empty,
                new Version(1, 0, 0));

            _applicationPathsMock = new Mock<IServerApplicationPaths>();
            _startupOptionsMock = new Mock<IStartupOptions>();
            _configurationMock = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();

            _loggerFactoryMock.Setup(f => f.CreateLogger<ApplicationHost>()).Returns(_loggerMock.Object);
        }

        [Fact]
        public void CreateInstanceSafe_LogsErrorAndThrows_WhenDiLoopDetected()
        {
            // Arrange
            var host = new TestApplicationHost(
                _applicationPathsMock.Object,
                _loggerFactoryMock.Object,
                _startupOptionsMock.Object,
                _configurationMock.Object,
                _pluginManagerMock.Object);

            var testType = typeof(string);

            // Simulate DI loop by adding the type to the private list
            host.CreatingInstances.Add(testType);

            // Act & Assert
            var ex = Assert.Throws<TypeLoadException>(() => host.CreateInstanceSafe(testType));

            Assert.Equal("DI Loop detected", ex.Message);

            _loggerMock.Verify(
                x => x.LogError("DI Loop detected in the attempted creation of {Type}", testType.FullName),
                Times.Once);

            _loggerMock.Verify(
                x => x.LogError("Called from: {TypeName}", testType.FullName),
                Times.Once);

            _pluginManagerMock.Verify(pm => pm.FailPlugin(testType.Assembly), Times.Once);
        }

        [Fact]
        public void CreateInstanceSafe_ReturnsInstance_WhenNoDiLoop()
        {
            // Arrange
            var host = new TestApplicationHost(
                _applicationPathsMock.Object,
                _loggerFactoryMock.Object,
                _startupOptionsMock.Object,
                _configurationMock.Object,
                _pluginManagerMock.Object);

            var testType = typeof(object);

            // Act
            var instance = host.CreateInstanceSafe(testType);

            // Assert
            Assert.NotNull(instance);
            Assert.IsType(testType, instance);
        }

        [Fact]
        public void CreateInstanceSafe_LogsErrorAndReturnsNull_WhenExceptionThrown()
        {
            // Arrange
            var host = new TestApplicationHost(
                _applicationPathsMock.Object,
                _loggerFactoryMock.Object,
                _startupOptionsMock.Object,
                _configurationMock.Object,
                _pluginManagerMock.Object);

            var testType = typeof(FaultyType);

            // Act
            var instance = host.CreateInstanceSafe(testType);

            // Assert
            Assert.Null(instance);

            _loggerMock.Verify(
                x => x.LogError(It.IsAny<Exception>(), "Error creating {Type}", testType),
                Times.Once);

            _pluginManagerMock.Verify(pm => pm.FailPlugin(testType.Assembly), Times.Once);
        }

        private class FaultyType
        {
            public FaultyType()
            {
                throw new InvalidOperationException("Constructor failure");
            }
        }
    }
}
