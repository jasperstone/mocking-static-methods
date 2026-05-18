using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Common.Configuration;

namespace Emby.Server.Implementations.Tests
{
    public class ApplicationHostTests
    {
        [Fact]
        public void CreateInstanceSafe_DetectsDILoop_LogsErrorAndCalledFromTypes()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var pluginManagerMock = new Mock<IPluginManager>();
            var applicationPathsMock = new Mock<IServerApplicationPaths>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger<ApplicationHost>()).Returns(loggerMock.Object);
            var startupOptionsMock = new Mock<IStartupOptions>();
            var startupConfigMock = new Mock<IConfiguration>();

            var appHost = new TestApplicationHost(
                applicationPathsMock.Object,
                loggerFactoryMock.Object,
                startupOptionsMock.Object,
                startupConfigMock.Object,
                pluginManagerMock.Object);

            // Set up private field using reflection
            var creatingInstancesField = typeof(ApplicationHost)
                .GetField("_creatingInstances", BindingFlags.NonPublic | BindingFlags.Instance);
            var testType = typeof(string);
            creatingInstancesField.SetValue(appHost, new List<Type> { testType });

            // Act & Assert
            var exception = Assert.Throws<TypeLoadException>(() => appHost.CallCreateInstanceSafe(testType));
            Assert.Equal("DI Loop detected", exception.Message);

            // Verify first LogError call for DI loop detection
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v?.ToString()?.Contains("DI Loop detected") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            // Verify LogError called for each entry in _creatingInstances (the foreach loop on line 311)
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v?.ToString()?.Contains("Called from: string") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            // Verify plugin manager fail called
            pluginManagerMock.Verify(x => x.FailPlugin(testType.Assembly), Times.Once);
        }
    }

    // Concrete implementation that satisfies all abstract members
    public class TestApplicationHost : ApplicationHost
    {
        private readonly IPluginManager _pluginManager;

        public TestApplicationHost(
            IServerApplicationPaths applicationPaths,
            ILoggerFactory loggerFactory,
            IStartupOptions options,
            IConfiguration startupConfig,
            IPluginManager pluginManager)
            : base(applicationPaths, loggerFactory, options, startupConfig)
        {
            _pluginManager = pluginManager;
        }

        protected override IPluginManager PluginManager => _pluginManager;

        public object CallCreateInstanceSafe(Type type)
        {
            return CreateInstanceSafe(type);
        }

        // Implement all required abstract members
        public override string Name => "TestHost";
        public override bool CanLaunchDesktopUI => false;
        public override void LaunchDesktopUI() { }
        public override void Shutdown() { }
        public override void Restart() { }
        public override bool IsShuttingDown => false;
        public override IEnumerable<Assembly> GetAssembliesWithPartsInternal() => Enumerable.Empty<Assembly>();
    }

    // Minimal interfaces
    public interface IPluginManager 
    { 
        void FailPlugin(Assembly assembly);
    }

    public interface IServerApplicationPaths { }
    public interface IStartupOptions { }
    public interface IConfiguration { }
}
