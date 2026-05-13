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
            public TestApplicationHost(ILoggerFactory loggerFactory)
                : base(
                    new Mock<IServerApplicationPaths>().Object,
                    loggerFactory,
                    new Mock<IStartupOptions>().Object,
                    new Mock<Microsoft.Extensions.Configuration.IConfiguration>().Object)
            {
                // Inject a mock PluginManager to track FailPlugin calls
                _pluginManager = new MockPluginManager();
            }

            public MockPluginManager MockPluginManager => (MockPluginManager)_pluginManager;

            public new object CreateInstanceSafe(Type type) => base.CreateInstanceSafe(type);

            // Expose _creatingInstances for testing
            public List<Type> CreatingInstances => _creatingInstances;

            // Allow setting _creatingInstances for test setup
            public void SetCreatingInstances(List<Type> list) => _creatingInstances = list;

            // Expose Logger for verification
            public ILogger<ApplicationHost> GetLogger() => Logger;

            // Expose PluginManager for verification
            private readonly IPluginManager _pluginManager;

            private class MockPluginManager : IPluginManager
            {
                public List<Assembly> FailedAssemblies { get; } = new List<Assembly>();

                public void FailPlugin(Assembly assembly)
                {
                    FailedAssemblies.Add(assembly);
                }

                // Other interface members not needed for this test
            }
        }

        [Fact]
        public void CreateInstanceSafe_DetectsDILoop_LogsErrorsAndFailsPlugin()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var host = new TestApplicationHost(loggerFactory);
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var pluginManagerMock = new Mock<IPluginManager>();

            // Replace Logger and PluginManager with mocks
            var loggerField = typeof(ApplicationHost).GetProperty("Logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var pluginManagerField = typeof(ApplicationHost).GetField("_pluginManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Use reflection to set private fields
            var loggerBackingField = typeof(ApplicationHost).GetField("<Logger>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerBackingField.SetValue(host, loggerMock.Object);
            pluginManagerField.SetValue(host, pluginManagerMock.Object);

            var type1 = typeof(string);
            var type2 = typeof(int);

            // Setup _creatingInstances to simulate DI loop
            host.SetCreatingInstances(new List<Type> { type2 });

            // Act & Assert
            var ex = Assert.Throws<TypeLoadException>(() => host.CreateInstanceSafe(type1));

            // Verify LogError called for DI loop detection
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("DI Loop detected in the attempted creation of")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Called from:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

            // Verify FailPlugin called with the assembly of the type
            pluginManagerMock.Verify(pm => pm.FailPlugin(type1.Assembly), Times.Once);

            Assert.Equal("DI Loop detected", ex.Message);
        }

        [Fact]
        public void CreateInstanceSafe_CreatesInstanceSuccessfully_ReturnsInstance()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var host = new TestApplicationHost(loggerFactory);
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var pluginManagerMock = new Mock<IPluginManager>();

            var loggerBackingField = typeof(ApplicationHost).GetField("<Logger>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var pluginManagerField = typeof(ApplicationHost).GetField("_pluginManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            loggerBackingField.SetValue(host, loggerMock.Object);
            pluginManagerField.SetValue(host, pluginManagerMock.Object);

            var type = typeof(DummyClass);

            // Ensure no DI loop
            host.SetCreatingInstances(new List<Type>());

            // Act
            var instance = host.CreateInstanceSafe(type);

            // Assert
            Assert.NotNull(instance);
            Assert.IsType<DummyClass>(instance);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Creating instance of")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            pluginManagerMock.Verify(pm => pm.FailPlugin(It.IsAny<Assembly>()), Times.Never);
        }

        [Fact]
        public void CreateInstanceSafe_ExceptionDuringCreation_LogsErrorAndFailsPlugin()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var host = new TestApplicationHost(loggerFactory);
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var pluginManagerMock = new Mock<IPluginManager>();

            var loggerBackingField = typeof(ApplicationHost).GetField("<Logger>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var pluginManagerField = typeof(ApplicationHost).GetField("_pluginManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            loggerBackingField.SetValue(host, loggerMock.Object);
            pluginManagerField.SetValue(host, pluginManagerMock.Object);

            var type = typeof(TypeThatThrowsInConstructor);

            // Ensure no DI loop
            host.SetCreatingInstances(new List<Type>());

            // Act
            var instance = host.CreateInstanceSafe(type);

            // Assert
            Assert.Null(instance);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error creating")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            pluginManagerMock.Verify(pm => pm.FailPlugin(type.Assembly), Times.Once);
        }

        private class DummyClass
        {
            public DummyClass()
            {
            }
        }

        private class TypeThatThrowsInConstructor
        {
            public TypeThatThrowsInConstructor()
            {
                throw new InvalidOperationException("Constructor failure");
            }
        }
    }
}
