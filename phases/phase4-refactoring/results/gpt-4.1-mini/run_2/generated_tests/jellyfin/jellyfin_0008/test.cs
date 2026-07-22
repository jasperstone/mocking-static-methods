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
            public TestApplicationHost(ILoggerFactory loggerFactory, PluginManager pluginManager)
                : base(null, loggerFactory, null, null)
            {
                _pluginManager = pluginManager;
                _creatingInstances = new List<Type>();
            }

            public new List<Type> _creatingInstances;
            public new PluginManager _pluginManager;

            // Implement abstract member with dummy implementation
            protected override IEnumerable<Assembly> GetAssembliesWithPartsInternal()
            {
                return Array.Empty<Assembly>();
            }

            public object CallCreateInstanceSafe(Type type)
            {
                return CreateInstanceSafe(type);
            }
        }

        [Fact]
        public void CreateInstanceSafe_LogsErrorAndThrowsOnDiLoop()
        {
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<ApplicationHost>()).Returns(loggerMock.Object);

            var pluginManagerMock = new Mock<PluginManager>(
                Mock.Of<ILogger<PluginManager>>(),
                Mock.Of<MediaBrowser.Controller.IServerApplicationHost>(),
                null,
                string.Empty,
                new Version(1, 0, 0));

            var host = new TestApplicationHost(loggerFactoryMock.Object, pluginManagerMock.Object);

            var type = typeof(string);
            host._creatingInstances.Add(type);

            var ex = Assert.Throws<TypeLoadException>(() => host.CallCreateInstanceSafe(type));

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

            pluginManagerMock.Verify(x => x.FailPlugin(type.Assembly), Times.Once);
            Assert.Equal("DI Loop detected", ex.Message);
        }

        [Fact]
        public void CreateInstanceSafe_LogsErrorOnExceptionAndReturnsNull()
        {
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<ApplicationHost>()).Returns(loggerMock.Object);

            var pluginManagerMock = new Mock<PluginManager>(
                Mock.Of<ILogger<PluginManager>>(),
                Mock.Of<MediaBrowser.Controller.IServerApplicationHost>(),
                null,
                string.Empty,
                new Version(1, 0, 0));

            var host = new TestApplicationHost(loggerFactoryMock.Object, pluginManagerMock.Object);

            var type = typeof(TypeThatThrowsOnCreate);
            host._creatingInstances = new List<Type>();

            var result = host.CallCreateInstanceSafe(type);

            Assert.Null(result);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error creating")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            pluginManagerMock.Verify(x => x.FailPlugin(type.Assembly), Times.Once);
        }

        private class TypeThatThrowsOnCreate
        {
            public TypeThatThrowsOnCreate()
            {
                throw new InvalidOperationException("Fail");
            }
        }
    }
}
