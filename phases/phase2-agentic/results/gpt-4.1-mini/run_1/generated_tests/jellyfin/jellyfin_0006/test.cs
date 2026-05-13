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

            // Expose _creatingInstances for test
            public List<Type> CreatingInstances => _creatingInstances;

            // Allow setting _creatingInstances for test
            public void SetCreatingInstances(List<Type> list) => _creatingInstances = list;

            private class MockPluginManager : PluginManager
            {
                public List<Assembly> FailedAssemblies { get; } = new();

                public MockPluginManager()
                    : base(
                        new Mock<ILogger<PluginManager>>().Object,
                        new Mock<IServerApplicationHost>().Object,
                        new MediaBrowser.Model.Configuration.ServerConfiguration(),
                        "fakePath",
                        new Version(1, 0, 0))
                {
                }

                public override void FailPlugin(Assembly assembly)
                {
                    FailedAssemblies.Add(assembly);
                }
            }
        }

        [Fact]
        public void CreateInstanceSafe_DetectsDiLoop_LogsErrorsAndThrows()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var host = new TestApplicationHost(loggerFactory);
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var type1 = typeof(string);
            var type2 = typeof(int);

            // Replace Logger with mock
            var loggerField = typeof(ApplicationHost).GetProperty("Logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            // Can't set property, so use reflection to set private field _logger if exists or use constructor injection
            // Instead, we will use reflection to set the Logger property backing field
            var loggerBackingField = typeof(ApplicationHost).GetField("<Logger>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerBackingField.SetValue(host, loggerMock.Object);

            // Setup _creatingInstances to simulate DI loop
            host.SetCreatingInstances(new List<Type> { type1, type2 });

            // Act & Assert
            var ex = Assert.Throws<TypeLoadException>(() => host.CreateInstanceSafe(type1));

            // Verify logs
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
                Times.Exactly(2)); // once for each entry in _creatingInstances

            // Verify plugin fail called
            Assert.Contains(type1.Assembly, host.MockPluginManager.FailedAssemblies);

            Assert.Equal("DI Loop detected", ex.Message);
        }

        [Fact]
        public void CreateInstanceSafe_CreatesInstanceSuccessfully_WithoutServiceProvider()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var host = new TestApplicationHost(loggerFactory);
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var type = typeof(TestClassNoDefaultCtor);

            // Replace Logger with mock
            var loggerBackingField = typeof(ApplicationHost).GetField("<Logger>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerBackingField.SetValue(host, loggerMock.Object);

            // Clear _creatingInstances
            host.SetCreatingInstances(new List<Type>());

            // Act
            var instance = host.CreateInstanceSafe(typeof(TestClassWithDefaultCtor));

            // Assert
            Assert.NotNull(instance);
            Assert.IsType<TestClassWithDefaultCtor>(instance);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Creating instance of")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void CreateInstanceSafe_CreatesInstanceSuccessfully_WithServiceProvider()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var host = new TestApplicationHost(loggerFactory);
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var type = typeof(TestClassWithDependency);

            // Replace Logger with mock
            var loggerBackingField = typeof(ApplicationHost).GetField("<Logger>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerBackingField.SetValue(host, loggerMock.Object);

            // Clear _creatingInstances
            host.SetCreatingInstances(new List<Type>());

            // Setup ServiceProvider with dependency
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IDependency))).Returns(new DependencyImpl());
            var serviceProviderField = typeof(ApplicationHost).GetProperty("ServiceProvider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var serviceProviderBackingField = typeof(ApplicationHost).GetField("<ServiceProvider>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            serviceProviderBackingField.SetValue(host, serviceProviderMock.Object);

            // Act
            var instance = host.CreateInstanceSafe(type);

            // Assert
            Assert.NotNull(instance);
            Assert.IsType<TestClassWithDependency>(instance);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Creating instance of")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void CreateInstanceSafe_CatchesException_LogsErrorAndFailsPlugin()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var host = new TestApplicationHost(loggerFactory);
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var type = typeof(TypeThatThrowsOnCreate);

            // Replace Logger with mock
            var loggerBackingField = typeof(ApplicationHost).GetField("<Logger>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerBackingField.SetValue(host, loggerMock.Object);

            // Clear _creatingInstances
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

            Assert.Contains(type.Assembly, host.MockPluginManager.FailedAssemblies);
        }

        private class TestClassWithDefaultCtor
        {
            public TestClassWithDefaultCtor()
            {
            }
        }

        private interface IDependency { }

        private class DependencyImpl : IDependency { }

        private class TestClassWithDependency
        {
            public IDependency Dependency { get; }

            public TestClassWithDependency(IDependency dependency)
            {
                Dependency = dependency;
            }
        }

        private class TypeThatThrowsOnCreate
        {
            public TypeThatThrowsOnCreate()
            {
                throw new InvalidOperationException("Failing constructor");
            }
        }
    }
}
