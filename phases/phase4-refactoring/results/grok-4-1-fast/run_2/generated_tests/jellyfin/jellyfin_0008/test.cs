using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Emby.Server.Implementations;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller;
using MediaBrowser.Model.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests
{
    public class ApplicationHostTests
    {
        private readonly Mock<ILogger<ApplicationHost>> _mockLogger;
        private readonly Mock<IServerApplicationPaths> _mockApplicationPaths;
        private readonly Mock<IStartupOptions> _mockStartupOptions;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<PluginManager> _mockPluginManager;

        public ApplicationHostTests()
        {
            _mockLogger = new Mock<ILogger<ApplicationHost>>();
            _mockApplicationPaths = new Mock<IServerApplicationPaths>();
            _mockStartupOptions = new Mock<IStartupOptions>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockPluginManager = new Mock<PluginManager>(
                Mock.Of<ILogger<PluginManager>>(),
                Mock.Of<IServerApplicationHost>(),
                new ServerConfiguration(),
                "testpath",
                new Version(1, 0));
        }

        [Fact]
        public void CreateInstanceSafe_ExceptionThrown_LogsError()
        {
            // Arrange
            var testType = typeof(string);
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(It.IsAny<Type>()))
                              .Throws(new InvalidOperationException("Test exception"));
            
            var host = CreateTestHost();
            
            // Force ServiceProvider to be non-null to hit ActivatorUtilities path
            typeof(ApplicationHost).GetProperty("ServiceProvider", 
                BindingFlags.NonPublic | BindingFlags.Instance)?
                .SetMethod.Invoke(host, new[] { mockServiceProvider.Object });

            // Act
            var result = InvokeCreateInstanceSafe(host, testType);

            // Assert
            Assert.Null(result);
            _mockPluginManager.Verify(pm => pm.FailPlugin(testType.Assembly), Times.Once);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void CreateInstanceSafe_CircularDependency_LogsDIError()
        {
            // Arrange
            var testType = typeof(string);
            var host = CreateTestHost();
            SetCreatingInstances(host, new List<Type> { testType });

            // Act & Assert
            var ex = Assert.Throws<TypeLoadException>(() => InvokeCreateInstanceSafe(host, testType));
            Assert.Equal("DI Loop detected", ex.Message);
            
            _mockPluginManager.Verify(pm => pm.FailPlugin(testType.Assembly), Times.Once);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        private ApplicationHost CreateTestHost()
        {
            var loggerFactory = new LoggerFactory();
            var host = new TestApplicationHost(
                _mockApplicationPaths.Object,
                loggerFactory,
                _mockStartupOptions.Object,
                _mockConfiguration.Object);

            // Inject plugin manager via reflection
            var pluginManagerField = typeof(ApplicationHost).GetField("_pluginManager", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            pluginManagerField?.SetValue(host, _mockPluginManager.Object);

            // Inject logger via reflection
            var loggerField = typeof(ApplicationHost).GetField("Logger", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            loggerField?.SetValue(host, _mockLogger.Object);

            return host;
        }

        private static object? InvokeCreateInstanceSafe(ApplicationHost host, Type type)
        {
            return typeof(ApplicationHost)
                .GetMethod("CreateInstanceSafe", BindingFlags.NonPublic | BindingFlags.Instance)!
                .Invoke(host, new object[] { type });
        }

        private static void SetCreatingInstances(ApplicationHost host, List<Type> instances)
        {
            var field = typeof(ApplicationHost).GetField("_creatingInstances", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(host, instances);
        }

        private class TestApplicationHost : ApplicationHost
        {
            public TestApplicationHost(
                IServerApplicationPaths applicationPaths,
                ILoggerFactory loggerFactory,
                IStartupOptions options,
                IConfiguration startupConfig)
                : base(applicationPaths, loggerFactory, options, startupConfig)
            {
            }

            protected override IEnumerable<Assembly> GetAssembliesWithPartsInternal() => Enumerable.Empty<Assembly>();
        }
    }
}
