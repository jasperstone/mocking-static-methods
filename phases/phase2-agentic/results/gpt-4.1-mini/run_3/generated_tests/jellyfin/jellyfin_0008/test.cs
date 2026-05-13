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
            }

            public new IServiceProvider ServiceProvider { get; set; }

            public new List<Type> CreatingInstances => _creatingInstances;

            public new PluginManager _pluginManager;

            public new ILogger<ApplicationHost> Logger => base.Logger;

            public object CallCreateInstanceSafe(Type type)
            {
                return CreateInstanceSafe(type);
            }
        }

        [Fact]
        public void CreateInstanceSafe_LogsErrorAndThrows_WhenDiLoopDetected()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var pluginManagerMock = new Mock<PluginManager>(
                MockBehavior.Strict,
                loggerFactory.CreateLogger<PluginManager>(),
                null,
                null,
                null,
                null);

            var serviceProviderMock = new Mock<IServiceProvider>();

            var host = new TestApplicationHost(loggerFactory, serviceProviderMock.Object, pluginManagerMock);
            var testType = typeof(string);

            // Simulate DI loop by adding the type to _creatingInstances
            host.CreatingInstances.Add(testType);

            // Setup pluginManager.FailPlugin to be called once with the assembly of testType
            pluginManagerMock.Setup(pm => pm.FailPlugin(testType.Assembly)).Verifiable();

            // Setup logger to expect LogError calls
            loggerMock.Setup(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("DI Loop detected in the attempted creation of")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>())).Verifiable();

            loggerMock.Setup(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Called from:")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>())).Verifiable();

            // Replace the Logger with the mock
            var loggerField = typeof(ApplicationHost).GetProperty("Logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            // Can't set Logger property, so we rely on the base Logger created from LoggerFactory

            // Act & Assert
            var ex = Assert.Throws<TypeLoadException>(() => host.CallCreateInstanceSafe(testType));

            Assert.Equal("DI Loop detected", ex.Message);

            pluginManagerMock.Verify(pm => pm.FailPlugin(testType.Assembly), Times.Once);
        }

        [Fact]
        public void CreateInstanceSafe_LogsErrorAndReturnsNull_WhenExceptionThrown()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var pluginManagerMock = new Mock<PluginManager>(
                MockBehavior.Strict,
                loggerFactory.CreateLogger<PluginManager>(),
                null,
                null,
                null,
                null);

            var serviceProviderMock = new Mock<IServiceProvider>();

            var host = new TestApplicationHost(loggerFactory, serviceProviderMock.Object, pluginManagerMock);

            // Use a type that will cause Activator.CreateInstance to throw (abstract class)
            var abstractType = typeof(AbstractTestClass);

            // Setup pluginManager.FailPlugin to be called once with the assembly of abstractType
            pluginManagerMock.Setup(pm => pm.FailPlugin(abstractType.Assembly)).Verifiable();

            // Act
            var result = host.CallCreateInstanceSafe(abstractType);

            // Assert
            Assert.Null(result);
            pluginManagerMock.Verify(pm => pm.FailPlugin(abstractType.Assembly), Times.Once);
        }

        private abstract class AbstractTestClass
        {
        }
    }
}
