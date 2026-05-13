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
            }

            public new IServiceProvider ServiceProvider { get; set; }

            public new ILogger<ApplicationHost> Logger { get; set; }

            public new List<Type> _creatingInstances = null;

            public new PluginManager _pluginManager;

            protected override object CreateInstanceSafe(Type type)
            {
                return base.CreateInstanceSafe(type);
            }
        }

        [Fact]
        public void CreateInstanceSafe_LogsErrorAndThrows_WhenDiLoopDetected()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var pluginManagerMock = new Mock<PluginManager>(
                loggerFactory.CreateLogger<PluginManager>(),
                null,
                null,
                string.Empty,
                null);

            var serviceProviderMock = new Mock<IServiceProvider>();

            var host = new TestApplicationHost(loggerFactory, serviceProviderMock.Object, pluginManagerMock);
            host._creatingInstances = new List<Type>();

            var testType = typeof(string);
            host._creatingInstances.Add(testType);

            // Setup logger to capture LogError calls
            var loggedMessages = new List<string>();
            loggerMock.Setup(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Callback<LogLevel, EventId, object, Exception, Func<object, Exception, string>>(
                    (level, id, state, ex, formatter) =>
                    {
                        loggedMessages.Add(formatter(state, ex));
                    });

            host.Logger = loggerMock.Object;

            // Act & Assert
            var ex = Assert.Throws<TypeLoadException>(() => host.CreateInstanceSafe(testType));
            Assert.Contains("DI Loop detected", ex.Message);

            // Verify that LogError was called with the DI loop message and the call stack
            Assert.Contains(loggedMessages, msg => msg.Contains("DI Loop detected in the attempted creation of"));
            Assert.Contains(loggedMessages, msg => msg.Contains("Called from:"));

            // Verify plugin fail called
            pluginManagerMock.Verify(pm => pm.FailPlugin(testType.Assembly), Times.Once);
        }

        [Fact]
        public void CreateInstanceSafe_LogsErrorAndReturnsNull_WhenExceptionThrown()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var pluginManagerMock = new Mock<PluginManager>(
                loggerFactory.CreateLogger<PluginManager>(),
                null,
                null,
                string.Empty,
                null);

            var serviceProviderMock = new Mock<IServiceProvider>();

            var host = new TestApplicationHost(loggerFactory, serviceProviderMock.Object, pluginManagerMock);
            host._creatingInstances = new List<Type>();

            var testType = typeof(FaultyType);

            // Setup logger to capture LogError calls
            var loggedErrors = new List<string>();
            loggerMock.Setup(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Callback<LogLevel, EventId, object, Exception, Func<object, Exception, string>>(
                    (level, id, state, ex, formatter) =>
                    {
                        loggedErrors.Add(formatter(state, ex));
                    });

            host.Logger = loggerMock.Object;

            // Act
            var result = host.CreateInstanceSafe(testType);

            // Assert
            Assert.Null(result);
            Assert.Single(loggedErrors);
            Assert.Contains("Error creating", loggedErrors[0]);
            pluginManagerMock.Verify(pm => pm.FailPlugin(testType.Assembly), Times.Once);
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
