using Emby.Server.Implementations;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Reflection;
using Xunit;

namespace Emby.Server.Implementations.Tests
{
    public class ApplicationHostTests
    {
        [Fact]
        public void CreateInstanceSafe_LogsErrorAndFailsPlugin_WhenDILOOPDetected()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<ApplicationHost>();
            var pluginManager = new Mock<Emby.Server.Implementations.PluginManager>(MockBehavior.Strict, 
                new object[] { loggerFactory.CreateLogger<Emby.Server.Implementations.PluginManager>(), 
                    new Mock<Emby.Server.Implementations.IServerApplicationHost>().Object, 
                    new Mock<Microsoft.Extensions.Configuration.IConfiguration>().Object, 
                    string.Empty, 
                    new Version() });
            pluginManager.Setup(pm => pm.FailPlugin(It.IsAny<Assembly>()));
            var applicationHost = new TestApplicationHost(
                new Mock<Emby.Server.Implementations.IServerApplicationPaths>().Object,
                loggerFactory,
                new Mock<Emby.Server.Implementations.IStartupOptions>().Object,
                new Mock<Microsoft.Extensions.Configuration.IConfiguration>().Object,
                pluginManager.Object);

            applicationHost._creatingInstances = new List<Type>();
            applicationHost._creatingInstances.Add(typeof(string));

            // Act and Assert
            Assert.Throws<TypeLoadException>(() => applicationHost.CreateInstanceSafe(typeof(string)));
            pluginManager.Verify(pm => pm.FailPlugin(It.IsAny<Assembly>()), Times.Once);
        }

        [Fact]
        public void CreateInstanceSafe_LogsErrorAndFailsPlugin_WhenExceptionThrown()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<ApplicationHost>();
            var pluginManager = new Mock<Emby.Server.Implementations.PluginManager>(MockBehavior.Strict, 
                new object[] { loggerFactory.CreateLogger<Emby.Server.Implementations.PluginManager>(), 
                    new Mock<Emby.Server.Implementations.IServerApplicationHost>().Object, 
                    new Mock<Microsoft.Extensions.Configuration.IConfiguration>().Object, 
                    string.Empty, 
                    new Version() });
            pluginManager.Setup(pm => pm.FailPlugin(It.IsAny<Assembly>()));
            var applicationHost = new TestApplicationHost(
                new Mock<Emby.Server.Implementations.IServerApplicationPaths>().Object,
                loggerFactory,
                new Mock<Emby.Server.Implementations.IStartupOptions>().Object,
                new Mock<Microsoft.Extensions.Configuration.IConfiguration>().Object,
                pluginManager.Object);

            // Act and Assert
            Assert.Throws<Exception>(() => applicationHost.CreateInstanceSafe(typeof(object)));
            pluginManager.Verify(pm => pm.FailPlugin(It.IsAny<Assembly>()), Times.Once);
        }
    }

    public class TestApplicationHost : ApplicationHost
    {
        private readonly Emby.Server.Implementations.PluginManager _pluginManager;

        public TestApplicationHost(
            Emby.Server.Implementations.IServerApplicationPaths applicationPaths,
            ILoggerFactory loggerFactory,
            Emby.Server.Implementations.IStartupOptions options,
            Microsoft.Extensions.Configuration.IConfiguration startupConfig,
            Emby.Server.Implementations.PluginManager pluginManager)
            : base(applicationPaths, loggerFactory, options, startupConfig)
        {
            _pluginManager = pluginManager;
        }

        protected override Emby.Server.Implementations.PluginManager CreatePluginManager(
            ILogger<Emby.Server.Implementations.PluginManager> logger,
            Emby.Server.Implementations.IServerApplicationHost host,
            Microsoft.Extensions.Configuration.IConfiguration config,
            string pluginsPath,
            Version applicationVersion)
        {
            return _pluginManager;
        }

        protected override Assembly[] GetAssembliesWithPartsInternal()
        {
            return new Assembly[0];
        }
    }
}
