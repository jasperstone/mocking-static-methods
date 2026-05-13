using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests
{
    public class ApplicationHostTests
    {
        private class TestApplicationHost : ApplicationHost
        {
            public TestApplicationHost(
                ILoggerFactory loggerFactory,
                IServiceProvider serviceProvider,
                Mock<PluginManager> pluginManagerMock)
                : base(
                    new Mock<IServerApplicationPaths>().Object,
                    loggerFactory,
                    new Mock<IStartupOptions>().Object,
                    new Mock<Microsoft.Extensions.Configuration.IConfiguration>().Object)
            {
                ServiceProvider = serviceProvider;
                _pluginManager = pluginManagerMock.Object;
                Logger = loggerFactory.CreateLogger<ApplicationHost>();
                _creatingInstances = new List<Type>();
            }

            public new object CreateInstanceSafe(Type type) => base.CreateInstanceSafe(type);

            // Expose _creatingInstances for test manipulation
            public List<Type> CreatingInstances => _creatingInstances;

            // Expose _pluginManager for verification
            public new PluginManager PluginManager => _pluginManager;

            // Expose Logger for verification
            public new ILogger<ApplicationHost> Logger { get; set; }

            // Expose ServiceProvider for test
            public new IServiceProvider ServiceProvider { get; set; }
        }

        [Fact]
        public void CreateInstanceSafe_LogsErrorAndThrows_WhenDiLoopDetected()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<ApplicationHost>()).Returns(loggerMock.Object);

            var pluginManagerMock = new Mock<PluginManager>(
                loggerFactoryMock.Object.CreateLogger<PluginManager>(),
                null,
                null,
                string.Empty,
                null);

            var serviceProviderMock = new Mock<IServiceProvider>();

            var host = new TestApplicationHost(loggerFactoryMock.Object, serviceProviderMock.Object, pluginManagerMock);

            var testType = typeof(string);
            host.CreatingInstances.Add(testType);

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
                Times.Exactly(host.CreatingInstances.Count));

            // Verify plugin fail called
            pluginManagerMock.Verify(pm => pm.FailPlugin(testType.Assembly), Times.Once);
        }

        [Fact]
        public void CreateInstanceSafe_LogsErrorAndReturnsNull_WhenExceptionThrown()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<ApplicationHost>()).Returns(loggerMock.Object);

            var pluginManagerMock = new Mock<PluginManager>(
                loggerFactoryMock.Object.CreateLogger<PluginManager>(),
                null,
                null,
                string.Empty,
                null);

            // ServiceProvider is null to force Activator.CreateInstance which will throw for an interface type
            IServiceProvider serviceProvider = null;

            var host = new TestApplicationHost(loggerFactoryMock.Object, serviceProvider, pluginManagerMock);

            var testType = typeof(IDisposable); // interface, Activator.CreateInstance will throw

            // Act
            var result = host.CreateInstanceSafe(testType);

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

            // Verify plugin fail called
            pluginManagerMock.Verify(pm => pm.FailPlugin(testType.Assembly), Times.Once);
        }

        [Fact]
        public void CreateInstanceSafe_CreatesInstanceSuccessfully()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<ApplicationHost>()).Returns(loggerMock.Object);

            var pluginManagerMock = new Mock<PluginManager>(
                loggerFactoryMock.Object.CreateLogger<PluginManager>(),
                null,
                null,
                string.Empty,
                null);

            var serviceProviderMock = new Mock<IServiceProvider>();

            var host = new TestApplicationHost(loggerFactoryMock.Object, serviceProviderMock.Object, pluginManagerMock);

            var testType = typeof(string);

            // Act
            var result = host.CreateInstanceSafe(testType);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<string>(result);

            // Verify LogDebug called for creation
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Creating instance of")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Verify no plugin fail called
            pluginManagerMock.Verify(pm => pm.FailPlugin(It.IsAny<Assembly>()), Times.Never);
        }
    }
}
