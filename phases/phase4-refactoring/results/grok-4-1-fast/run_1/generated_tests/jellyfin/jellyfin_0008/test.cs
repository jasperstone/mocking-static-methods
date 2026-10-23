using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Common;
using MediaBrowser.Controller;
using MediaBrowser.Model.Tasks;

namespace Emby.Server.Implementations.Tests
{
    public class ApplicationHostLoggerTests
    {
        private class TestApplicationHost : ApplicationHost
        {
            public List<Type> CreatingInstances { get; set; } = new();
            public IServiceProvider ServiceProvider { get; set; }
            
            private readonly Mock<IPluginManager> _pluginManagerMock;

            public TestApplicationHost(
                ILoggerFactory loggerFactory,
                Mock<IPluginManager> pluginManagerMock = null)
                : base(
                    Mock.Of<IServerApplicationPaths>(),
                    loggerFactory,
                    Mock.Of<IStartupOptions>(),
                    Mock.Of<Microsoft.Extensions.Configuration.IConfiguration>())
            {
                _pluginManagerMock = pluginManagerMock ?? new Mock<IPluginManager>(MockBehavior.Strict);
            }

            protected override List<Type> CreatingInstances => CreatingInstances;
            
            public new object CreateInstanceSafe(Type type) => base.CreateInstanceSafe(type);

            protected override IEnumerable<Assembly> GetAssembliesWithPartsInternal() => Array.Empty<Assembly>();
            
            protected override IPluginManager CreatePluginManager(
                ILogger logger,
                ApplicationHost host,
                MediaBrowser.Common.Configuration.IConfigurationManager config,
                string pluginsPath,
                Version version)
            {
                return _pluginManagerMock?.Object ?? Mock.Of<IPluginManager>();
            }
        }

        [Fact]
        public void CreateInstanceSafe_ThrowsException_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new TestLoggerProvider(loggerMock.Object));
            
            var pluginManagerMock = new Mock<IPluginManager>();
            var testType = typeof(string);
            
            var host = new TestApplicationHost(loggerFactory, pluginManagerMock);
            host.ServiceProvider = Mock.Of<IServiceProvider>();

            // Act
            var result = host.CreateInstanceSafe(testType);

            // Assert
            Assert.Null(result);
            pluginManagerMock.Verify(p => p.FailPlugin(testType.Assembly), Times.Once);
            
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void CreateInstanceSafe_DetectsDILoop_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new TestLoggerProvider(loggerMock.Object));
            
            var pluginManagerMock = new Mock<IPluginManager>();
            var testType = typeof(string);
            
            var host = new TestApplicationHost(loggerFactory, pluginManagerMock);
            host.CreatingInstances.Add(testType);

            // Act & Assert
            Assert.Throws<TypeLoadException>(() => host.CreateInstanceSafe(testType));
            
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
            
            pluginManagerMock.Verify(p => p.FailPlugin(testType.Assembly), Times.Once);
        }
    }

    public class TestLoggerProvider : ILoggerProvider
    {
        private readonly ILogger _logger;
        public TestLoggerProvider(ILogger logger) => _logger = logger;
        
        public ILogger CreateLogger(string categoryName) => _logger;
        public void Dispose() { }
    }
}
