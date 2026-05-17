using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations;
using Emby.Server.Implementations.Plugins;

namespace Emby.Server.Implementations.Tests
{
    public class ApplicationHostTests
    {
        private class TestApplicationHost : ApplicationHost
        {
            public TestApplicationHost(
                ILoggerFactory loggerFactory,
                IServiceProvider serviceProvider,
                PluginManager pluginManager)
                : base(
                    new TestServerApplicationPaths(),
                    loggerFactory,
                    new Mock<IStartupOptions>().Object,
                    new Mock<Microsoft.Extensions.Configuration.IConfiguration>().Object)
            {
                ServiceProvider = serviceProvider;
                SetPluginManager(pluginManager);
            }

            public new IServiceProvider ServiceProvider
            {
                get => base.ServiceProvider;
                set => base.ServiceProvider = value;
            }

            private List<Type> _creatingInstances = new List<Type>();

            private void SetPluginManager(PluginManager pluginManager)
            {
                var pluginManagerField = typeof(ApplicationHost).GetField("_pluginManager", BindingFlags.NonPublic | BindingFlags.Instance);
                pluginManagerField.SetValue(this, pluginManager);
            }

            private void SetCreatingInstances(List<Type> creatingInstances)
            {
                var creatingInstancesField = typeof(ApplicationHost).GetField("_creatingInstances", BindingFlags.NonPublic | BindingFlags.Instance);
                creatingInstancesField.SetValue(this, creatingInstances);
            }

            public object CallCreateInstanceSafe(Type type)
            {
                SetCreatingInstances(_creatingInstances);
                var method = typeof(ApplicationHost).GetMethod("CreateInstanceSafe", BindingFlags.NonPublic | BindingFlags.Instance);
                return method.Invoke(this, new object[] { type });
            }
        }

        private class TestServerApplicationPaths : IServerApplicationPaths
        {
            public string AppDataPath => throw new NotImplementedException();
            public string CachePath => throw new NotImplementedException();
            public string ConfigPath => throw new NotImplementedException();
            public string DataPath => throw new NotImplementedException();
            public string LogPath => throw new NotImplementedException();
            public string PluginsPath => string.Empty;
            public string TempPath => throw new NotImplementedException();
            public string WebPath => throw new NotImplementedException();
        }

        [Fact]
        public void CreateInstanceSafe_LogsErrorAndThrowsOnDiLoop()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<ApplicationHost>()).Returns(loggerMock.Object);

            var pluginManagerMock = new Mock<PluginManager>(
                new Mock<ILogger<PluginManager>>().Object,
                new Mock<IServerApplicationHost>().Object,
                null,
                string.Empty,
                new Version());

            var serviceProviderMock = new Mock<IServiceProvider>();

            var host = new TestApplicationHost(loggerFactoryMock.Object, serviceProviderMock.Object, pluginManagerMock.Object);

            var type = typeof(string);

            // Simulate the type is already being created to cause DI loop
            var creatingInstancesField = typeof(ApplicationHost).GetField("_creatingInstances", BindingFlags.NonPublic | BindingFlags.Instance);
            creatingInstancesField.SetValue(host, new List<Type> { type });

            // Act & Assert
            var ex = Assert.Throws<TargetInvocationException>(() => host.CallCreateInstanceSafe(type));
            Assert.IsType<TypeLoadException>(ex.InnerException);
            Assert.Equal("DI Loop detected", ex.InnerException.Message);

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
                Times.Exactly(1));

            // Verify pluginManager.FailPlugin called
            pluginManagerMock.Verify(pm => pm.FailPlugin(type.Assembly), Times.Once);
        }

        [Fact]
        public void CreateInstanceSafe_LogsErrorAndReturnsNullOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<ApplicationHost>()).Returns(loggerMock.Object);

            var pluginManagerMock = new Mock<PluginManager>(
                new Mock<ILogger<PluginManager>>().Object,
                new Mock<IServerApplicationHost>().Object,
                null,
                string.Empty,
                new Version());

            var serviceProviderMock = new ThrowingServiceProvider();

            var host = new TestApplicationHost(loggerFactoryMock.Object, serviceProviderMock, pluginManagerMock.Object);

            var type = typeof(string);

            // Act
            var result = host.CallCreateInstanceSafe(type);

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
            pluginManagerMock.Verify(pm => pm.FailPlugin(type.Assembly), Times.Once);
        }

        private class ThrowingServiceProvider : IServiceProvider
        {
            public object GetService(Type serviceType)
            {
                throw new InvalidOperationException("Test exception");
            }
        }
    }
}
