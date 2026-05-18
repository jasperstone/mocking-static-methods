using System;
using System.Collections.Generic;
using System.Reflection;
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
                    new Mock<IServerApplicationPaths>().Object,
                    loggerFactory,
                    new Mock<IStartupOptions>().Object,
                    new Mock<Microsoft.Extensions.Configuration.IConfiguration>().Object)
            {
                ServiceProvider = serviceProvider;
                _pluginManager = pluginManager;
                _creatingInstances = new List<Type>();
                _allConcreteTypes = Array.Empty<Type>();
            }

            public new IServiceProvider ServiceProvider
            {
                get => base.ServiceProvider;
                set => base.ServiceProvider = value;
            }

            public new List<Type> _creatingInstances;

            public new PluginManager _pluginManager;

            public new Type[] _allConcreteTypes;

            // Implement abstract member with dummy implementation
            protected override IEnumerable<Assembly> GetAssembliesWithPartsInternal()
            {
                yield break;
            }

            // Public wrapper to call protected CreateInstanceSafe
            public object CallCreateInstanceSafe(Type type)
            {
                return CreateInstanceSafe(type);
            }
        }

        [Fact]
        public void CreateInstanceSafe_LogsErrorAndThrowsOnDiLoop()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<ApplicationHost>()).Returns(loggerMock.Object);

            var pluginManagerMock = new Mock<PluginManager>(
                Mock.Of<ILogger<PluginManager>>(),
                Mock.Of<IServerApplicationHost>(),
                null,
                null,
                new Version(1, 0));

            var serviceProviderMock = new Mock<IServiceProvider>();

            var host = new TestApplicationHost(loggerFactoryMock.Object, serviceProviderMock.Object, pluginManagerMock.Object);

            var type = typeof(string);

            // Simulate DI loop by adding the type to _creatingInstances
            host._creatingInstances.Add(type);

            // Act & Assert
            var ex = Assert.Throws<TypeLoadException>(() => host.CallCreateInstanceSafe(type));

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
                Times.Exactly(host._creatingInstances.Count));

            // Verify plugin fail called
            pluginManagerMock.Verify(p => p.FailPlugin(type.Assembly), Times.Once);
        }

        [Fact]
        public void CreateInstanceSafe_LogsErrorAndReturnsNullOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<ApplicationHost>()).Returns(loggerMock.Object);

            var pluginManagerMock = new Mock<PluginManager>(
                Mock.Of<ILogger<PluginManager>>(),
                Mock.Of<IServerApplicationHost>(),
                null,
                null,
                new Version(1, 0));

            var serviceProviderMock = new Mock<IServiceProvider>();

            var host = new TestApplicationHost(loggerFactoryMock.Object, serviceProviderMock.Object, pluginManagerMock.Object);

            var type = typeof(FakeTypeThatThrows);

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

            // Verify plugin fail called
            pluginManagerMock.Verify(p => p.FailPlugin(type.Assembly), Times.Once);
        }

        private class FakeTypeThatThrows
        {
            public FakeTypeThatThrows()
            {
                throw new InvalidOperationException("Constructor throws");
            }
        }
    }

    // Dummy interfaces to satisfy references
    public interface IServerApplicationPaths { }
    public interface IStartupOptions { }
    public interface IServerApplicationHost { }
}
