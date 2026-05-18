using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Model.Tasks;

namespace Emby.Server.Implementations.Tests
{
    public class ApplicationHostTests
    {
        [Fact]
        public void CreateInstanceSafe_DetectsDILoop_LogsErrorMessages()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var pluginManager = new MockPluginManager();
            
            var appPathsMock = new Mock<IServerApplicationPaths>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<ApplicationHost>()).Returns(loggerMock.Object);

            var host = new TestApplicationHost(
                appPathsMock.Object, 
                loggerFactoryMock.Object, 
                new Mock<IStartupOptions>().Object, 
                new Mock<Microsoft.Extensions.Configuration.IConfiguration>().Object);
            
            host.SetLogger(loggerMock.Object);
            host.SetPluginManager(pluginManager);
            
            var testType = typeof(string);
            host.SetCreatingInstances(new List<Type> { testType });

            // Act & Assert
            var ex = Assert.Throws<TypeLoadException>(() => host.CreateInstanceSafe(testType));
            Assert.Equal("DI Loop detected", ex.Message);

            // Verify LogError calls using extension method verification
            loggerMock.Verify(x => x.LogError("DI Loop detected in the attempted creation of {Type}", testType.FullName), Times.Once);
            loggerMock.Verify(x => x.LogError("Called from: {TypeName}", testType.FullName), Times.Once);
            Assert.True(pluginManager.FailPluginCalled);
        }

        [Fact]
        public void CreateInstanceSafe_NoLoop_SucceedsWithoutErrors()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var pluginManager = new MockPluginManager();
            
            var appPathsMock = new Mock<IServerApplicationPaths>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<ApplicationHost>()).Returns(loggerMock.Object);

            var host = new TestApplicationHost(
                appPathsMock.Object, 
                loggerFactoryMock.Object, 
                new Mock<IStartupOptions>().Object, 
                new Mock<Microsoft.Extensions.Configuration.IConfiguration>().Object);
            
            host.SetLogger(loggerMock.Object);
            host.SetPluginManager(pluginManager);

            var testType = typeof(object);

            // Act
            var result = host.CreateInstanceSafe(testType);

            // Assert
            Assert.NotNull(result);
            loggerMock.Verify(x => x.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }
    }

    public class TestApplicationHost : ApplicationHost
    {
        public TestApplicationHost(
            IServerApplicationPaths appPaths,
            ILoggerFactory loggerFactory,
            IStartupOptions options,
            Microsoft.Extensions.Configuration.IConfiguration startupConfig)
            : base(appPaths, loggerFactory, options, startupConfig)
        {
        }

        public void SetLogger(ILogger<ApplicationHost> logger) => Logger = logger;
        public void SetPluginManager(PluginManager pluginManager) => _pluginManager = pluginManager;
        public void SetCreatingInstances(List<Type> instances) => _creatingInstances = instances;

        public new object CreateInstanceSafe(Type type) => base.CreateInstanceSafe(type);
    }

    public class MockPluginManager : PluginManager
    {
        public bool FailPluginCalled { get; private set; }

        public MockPluginManager() 
            : base(Mock.Of<ILogger<PluginManager>>(), Mock.Of<IServerApplicationHost>(), new ServerConfiguration(), "", new Version(1, 0))
        {
        }

        public override void FailPlugin(Assembly assembly)
        {
            FailPluginCalled = true;
        }
    }
}
