using Emby.Server.Implementations;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Emby.Server.Implementations.Tests
{
    public class ApplicationHostTests
    {
        [Fact]
        public void CreateInstanceSafe_LogsErrorAndFailsPlugin_WhenDiLoopDetected()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<ApplicationHost>();
            var pluginManager = new Mock<Emby.Server.Implementations.PluginManager>();
            var applicationPaths = new Mock<Emby.Server.Implementations.IServerApplicationPaths>();
            var startupOptions = new Mock<Emby.Server.Implementations.IStartupOptions>();
            var startupConfig = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            var applicationHost = new TestApplicationHost(
                applicationPaths.Object,
                loggerFactory,
                startupOptions.Object,
                startupConfig.Object)
            {
                Logger = logger,
                PluginManager = pluginManager.Object
            };

            applicationHost._creatingInstances = new List<Type>();
            applicationHost._creatingInstances.Add(typeof(ApplicationHost));

            // Act
            applicationHost.CreateInstanceSafe(typeof(ApplicationHost));

            // Assert
            pluginManager.Verify(pm => pm.FailPlugin(It.IsAny<Assembly>()), Times.Once);
            loggerFactory.AssertLogContains(logger, Microsoft.Extensions.Logging.LogLevel.Error, "DI Loop detected in the attempted creation of Emby.Server.Implementations.ApplicationHost");
        }

        [Fact]
        public void CreateInstanceSafe_LogsErrorAndFailsPlugin_WhenExceptionThrown()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<ApplicationHost>();
            var pluginManager = new Mock<Emby.Server.Implementations.PluginManager>();
            var applicationPaths = new Mock<Emby.Server.Implementations.IServerApplicationPaths>();
            var startupOptions = new Mock<Emby.Server.Implementations.IStartupOptions>();
            var startupConfig = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            var applicationHost = new TestApplicationHost(
                applicationPaths.Object,
                loggerFactory,
                startupOptions.Object,
                startupConfig.Object)
            {
                Logger = logger,
                PluginManager = pluginManager.Object
            };

            // Act
            applicationHost.CreateInstanceSafe(typeof(string));

            // Assert
            pluginManager.Verify(pm => pm.FailPlugin(It.IsAny<Assembly>()), Times.Once);
            loggerFactory.AssertLogContains(logger, Microsoft.Extensions.Logging.LogLevel.Error, "Error creating System.String");
        }
    }

    public class TestApplicationHost : Emby.Server.Implementations.ApplicationHost
    {
        public TestApplicationHost(
            Emby.Server.Implementations.IServerApplicationPaths applicationPaths,
            Microsoft.Extensions.Logging.ILoggerFactory loggerFactory,
            Emby.Server.Implementations.IStartupOptions options,
            Microsoft.Extensions.Configuration.IConfiguration startupConfig) 
            : base(applicationPaths, loggerFactory, options, startupConfig)
        {
        }

        public new Emby.Server.Implementations.PluginManager PluginManager
        {
            get { return _pluginManager; }
            set { _pluginManager = value; }
        }

        protected override Assembly[] GetAssembliesWithPartsInternal()
        {
            return new Assembly[0];
        }
    }
}
