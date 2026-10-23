#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Emby.Server.Implementations;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Emby.Server.Implementations.Tests
{
    public class ApplicationHostTests
    {
        [Fact]
        public void CreateInstanceSafe_DetectsDILabelLoop_LogsErrorMessages()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            loggerMock.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            var loggerFactory = NullLoggerFactory.Instance;
            var applicationPaths = Mock.Of<IServerApplicationPaths>();
            var startupOptions = Mock.Of<IStartupOptions>();
            var startupConfig = Mock.Of<IConfiguration>();
            
            var pluginManagerMock = new Mock<PluginManager>(
                loggerFactory.CreateLogger<PluginManager>(), 
                Mock.Of<IServerApplicationHost>(), 
                Mock.Of<ServerConfiguration>(), 
                "plugins", 
                new Version(1, 0))
            {
                CallBase = true
            };

            var host = new TestApplicationHost(
                applicationPaths,
                loggerFactory,
                startupOptions,
                startupConfig,
                loggerMock.Object,
                pluginManagerMock.Object);

            var loopType = typeof(string);
            host.SetCreatingInstances(new List<Type> { loopType });

            // Act & Assert
            var exception = Assert.Throws<TypeLoadException>(() => host.CallCreateInstanceSafe(loopType));
            Assert.Equal("DI Loop detected", exception.Message);

            // Verify first LogError - DI Loop detected
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => t.ToString()!.Contains("DI Loop detected in the attempted creation of {Type}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Verify second LogError - Called from
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => t.ToString()!.Contains("Called from: {TypeName}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            pluginManagerMock.Verify(x => x.FailPlugin(loopType.Assembly), Times.Once);
        }

        private class TestApplicationHost : ApplicationHost
        {
            private readonly ILogger<ApplicationHost> _testLogger;
            private readonly PluginManager _pluginManager;
            private List<Type>? _testCreatingInstances;

            public TestApplicationHost(
                IServerApplicationPaths applicationPaths,
                ILoggerFactory loggerFactory,
                IStartupOptions startupOptions,
                IConfiguration startupConfig,
                ILogger<ApplicationHost> testLogger,
                PluginManager pluginManager)
                : base(applicationPaths, loggerFactory, startupOptions, startupConfig)
            {
                _testLogger = testLogger;
                _pluginManager = pluginManager;

                // Set private logger field via reflection
                var loggerField = typeof(ApplicationHost).GetField("Logger", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                loggerField?.SetValue(this, _testLogger);
            }

            public void SetCreatingInstances(List<Type> creatingInstances)
            {
                _testCreatingInstances = creatingInstances;
            }

            public object CallCreateInstanceSafe(Type type)
            {
                // Use reflection to call the protected method
                return typeof(ApplicationHost).GetMethod("CreateInstanceSafe", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.
                    Invoke(this, new object[] { type })!;
            }

            protected override IEnumerable<Assembly> GetAssembliesWithPartsInternal()
            {
                return Enumerable.Empty<Assembly>();
            }
        }
    }
}
