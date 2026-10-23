using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Controller;

namespace Emby.Server.Implementations.Tests
{
    public class ApplicationHostTests
    {
        private class TestApplicationHost : ApplicationHost
        {
            public TestApplicationHost(ILogger<ApplicationHost> logger, PluginManager pluginManager)
                : base(
                    applicationPaths: null,
                    loggerFactory: new LoggerFactory(),
                    options: null,
                    startupConfig: null)
            {
                Logger = logger;
                _pluginManager = pluginManager;
                _creatingInstances = new List<Type>();
            }

            public new ILogger<ApplicationHost> Logger { get; set; }
            public new PluginManager _pluginManager { get; set; }
            public new List<Type> _creatingInstances { get; set; }

            public object CallCreateInstanceSafe(Type type)
            {
                return CreateInstanceSafe(type);
            }

            // Implement abstract member with dummy to allow instantiation
            protected override IEnumerable<Assembly> GetAssembliesWithPartsInternal()
            {
                return Array.Empty<Assembly>();
            }
        }

        [Fact]
        public void CreateInstanceSafe_LogsErrorAndThrows_WhenDiLoopDetected()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var pluginManagerMock = new Mock<PluginManager>(
                MockBehavior.Strict,
                Mock.Of<ILogger<PluginManager>>(),
                Mock.Of<IServerApplicationHost>(),
                null,
                null,
                new Version(1, 0));

            var host = new TestApplicationHost(loggerMock.Object, pluginManagerMock.Object);

            var type1 = typeof(string);
            var type2 = typeof(int);

            host._creatingInstances.Add(type1);
            host._creatingInstances.Add(type2);

            pluginManagerMock.Setup(pm => pm.FailPlugin(type1.Assembly));

            // Act & Assert
            var ex = Assert.Throws<TypeLoadException>(() => host.CallCreateInstanceSafe(type1));

            Assert.Equal("DI Loop detected", ex.Message);

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
                Times.Exactly(2));

            pluginManagerMock.Verify(pm => pm.FailPlugin(type1.Assembly), Times.Once);
        }
    }
}
