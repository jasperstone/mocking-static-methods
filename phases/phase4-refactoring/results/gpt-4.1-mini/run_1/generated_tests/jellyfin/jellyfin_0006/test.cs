using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Model.Configuration;

namespace Emby.Server.Implementations.Tests
{
    public class ApplicationHostTests
    {
        private class TestApplicationHost : ApplicationHost
        {
            public TestApplicationHost(
                ILogger<ApplicationHost> logger,
                PluginManager pluginManager)
                : base(
                    new Mock<IServerApplicationPaths>().Object,
                    new Mock<ILoggerFactory>().Object,
                    new Mock<IStartupOptions>().Object,
                    new Mock<Microsoft.Extensions.Configuration.IConfiguration>().Object)
            {
                Logger = logger;
                _pluginManager = pluginManager;
                _creatingInstances = new List<Type>();
            }

            // Implement abstract method with minimal stub
            protected override Assembly[] GetAssembliesWithPartsInternal()
            {
                return Array.Empty<Assembly>();
            }

            // Expose protected members for testing
            public new object CreateInstanceSafe(Type type)
            {
                return base.CreateInstanceSafe(type);
            }

            public new ILogger<ApplicationHost> Logger { get; set; }
            public new PluginManager _pluginManager { get; set; }
            public new List<Type> _creatingInstances { get; set; }
        }

        [Fact]
        public void CreateInstanceSafe_LogsErrorAndThrows_WhenDiLoopDetected()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var pluginManagerMock = new Mock<PluginManager>(
                MockBehavior.Loose,
                new object[]
                {
                    new Mock<ILogger<PluginManager>>().Object,
                    new Mock<IServerApplicationHost>().Object,
                    new ServerConfiguration(),
                    "pluginsPath",
                    new Version(1, 0, 0, 0)
                });
            var host = new TestApplicationHost(loggerMock.Object, pluginManagerMock.Object);

            var testType = typeof(string);
            host._creatingInstances.Add(testType);

            // Act & Assert
            var ex = Assert.Throws<TypeLoadException>(() => host.CreateInstanceSafe(testType));

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
                Times.AtLeastOnce);

            // Verify pluginManager.FailPlugin called
            pluginManagerMock.Verify(x => x.FailPlugin(testType.Assembly), Times.Once);

            Assert.Equal("DI Loop detected", ex.Message);
        }

        [Fact]
        public void CreateInstanceSafe_LogsErrorAndReturnsNull_WhenExceptionThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var pluginManagerMock = new Mock<PluginManager>(
                MockBehavior.Loose,
                new object[]
                {
                    new Mock<ILogger<PluginManager>>().Object,
                    new Mock<IServerApplicationHost>().Object,
                    new ServerConfiguration(),
                    "pluginsPath",
                    new Version(1, 0, 0, 0)
                });
            var host = new TestApplicationHost(loggerMock.Object, pluginManagerMock.Object);

            var typeThatThrows = typeof(TypeThatThrowsOnCreate);

            // Act
            var result = host.CreateInstanceSafe(typeThatThrows);

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
            pluginManagerMock.Verify(x => x.FailPlugin(typeThatThrows.Assembly), Times.Once);
        }

        private class TypeThatThrowsOnCreate
        {
            public TypeThatThrowsOnCreate()
            {
                throw new InvalidOperationException("Fail creation");
            }
        }
    }
}
