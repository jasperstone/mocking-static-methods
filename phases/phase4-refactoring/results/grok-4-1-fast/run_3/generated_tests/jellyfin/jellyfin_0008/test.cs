using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Emby.Server.Implementations;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Emby.Server.Implementations.Tests
{
    public class ApplicationHostTests
    {
        [Fact]
        public void CreateInstanceSafe_ThrowsException_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            
            var pluginManagerMock = new Mock<IPluginManager>();
            var applicationPathsMock = new Mock<IServerApplicationPaths>();
            var startupOptionsMock = new Mock<IStartupOptions>();
            var startupConfigMock = new Mock<IConfiguration>();

            var applicationHost = new TestApplicationHost(
                applicationPathsMock.Object,
                loggerFactoryMock.Object,
                startupOptionsMock.Object,
                startupConfigMock.Object,
                pluginManagerMock.Object);

            var testType = typeof(BadConstructorType);
            pluginManagerMock.Setup(x => x.FailPlugin(testType.Assembly));

            // Act
            var result = applicationHost.CallCreateInstanceSafe(testType);

            // Assert
            Assert.Null(result);
            pluginManagerMock.Verify(x => x.FailPlugin(testType.Assembly), Times.Once);
            
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error creating BadConstructorType")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void CreateInstanceSafe_DetectsCircularDependency_LogsMultipleErrors()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            
            var pluginManagerMock = new Mock<IPluginManager>();
            var applicationPathsMock = new Mock<IServerApplicationPaths>();
            var startupOptionsMock = new Mock<IStartupOptions>();
            var startupConfigMock = new Mock<IConfiguration>();

            var applicationHost = new TestApplicationHost(
                applicationPathsMock.Object,
                loggerFactoryMock.Object,
                startupOptionsMock.Object,
                startupConfigMock.Object,
                pluginManagerMock.Object);

            var testType = typeof(TestCircularDependencyType);
            pluginManagerMock.Setup(x => x.FailPlugin(testType.Assembly));

            // Act & Assert
            var ex = Assert.Throws<TypeLoadException>(() => applicationHost.CallCreateInstanceSafe(testType));
            Assert.Equal("DI Loop detected", ex.Message);
            
            pluginManagerMock.Verify(x => x.FailPlugin(testType.Assembly), Times.Once);
            loggerMock.Verify(
                x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeast(2));
        }

        private class BadConstructorType
        {
            public BadConstructorType() => throw new Exception("Test exception");
        }

        private class TestCircularDependencyType { }

        private class TestApplicationHost : ApplicationHost
        {
            private readonly IPluginManager _pluginManager;

            public TestApplicationHost(
                IServerApplicationPaths applicationPaths,
                ILoggerFactory loggerFactory,
                IStartupOptions startupOptions,
                IConfiguration startupConfig,
                IPluginManager pluginManager)
                : base(applicationPaths, loggerFactory, startupOptions, startupConfig)
            {
                _pluginManager = pluginManager;
            }

            public object CallCreateInstanceSafe(Type type) => CreateInstanceSafe(type);

            protected override IEnumerable<Assembly> GetAssembliesWithPartsInternal()
            {
                return Enumerable.Empty<Assembly>();
            }
        }
    }
}
