using Emby.Server.Implementations;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Common;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Model.Plugins;
using Xunit;

namespace Emby.Server.Implementations.Tests
{
    public class ApplicationHostTests
    {
        [Fact]
        public void CreateInstanceSafe_LogsErrorAndFailsPlugin_WhenExceptionOccurs()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<ApplicationHost>();
            var pluginManagerMock = new Mock<Emby.Server.Implementations.PluginManager>(MockBehavior.Strict);
            pluginManagerMock.Setup(pm => pm.FailPlugin(It.IsAny<Assembly>())).Verifiable();
            var applicationPathsMock = new Mock<MediaBrowser.Common.Configuration.IServerApplicationPaths>();
            var startupOptionsMock = new Mock<Emby.Server.Implementations.IStartupOptions>();
            var startupConfigMock = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            var applicationHost = new TestApplicationHost(
                applicationPathsMock.Object,
                loggerFactory,
                startupOptionsMock.Object,
                startupConfigMock.Object)
            {
                Logger = logger,
                _pluginManager = pluginManagerMock.Object
            };

            // Act
            try
            {
                applicationHost.CreateInstanceSafe(typeof(string));
            }
            catch (TypeLoadException)
            {
            }

            // Assert
            pluginManagerMock.Verify(pm => pm.FailPlugin(It.IsAny<Assembly>()), Times.Once);
        }

        [Fact]
        public void CreateInstanceSafe_LogsErrorAndFailsPlugin_WhenDILOOPDetected()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<ApplicationHost>();
            var pluginManagerMock = new Mock<Emby.Server.Implementations.PluginManager>(MockBehavior.Strict);
            pluginManagerMock.Setup(pm => pm.FailPlugin(It.IsAny<Assembly>())).Verifiable();
            var applicationPathsMock = new Mock<MediaBrowser.Common.Configuration.IServerApplicationPaths>();
            var startupOptionsMock = new Mock<Emby.Server.Implementations.IStartupOptions>();
            var startupConfigMock = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            var applicationHost = new TestApplicationHost(
                applicationPathsMock.Object,
                loggerFactory,
                startupOptionsMock.Object,
                startupConfigMock.Object)
            {
                Logger = logger,
                _pluginManager = pluginManagerMock.Object,
                _creatingInstances = new List<Type>()
            };

            applicationHost._creatingInstances.Add(typeof(string));

            // Act
            try
            {
                applicationHost.CreateInstanceSafe(typeof(string));
            }
            catch (TypeLoadException)
            {
            }

            // Assert
            pluginManagerMock.Verify(pm => pm.FailPlugin(It.IsAny<Assembly>()), Times.Once);
        }
    }

    public class TestApplicationHost : ApplicationHost
    {
        public TestApplicationHost(
            MediaBrowser.Common.Configuration.IServerApplicationPaths applicationPaths,
            ILoggerFactory loggerFactory,
            Emby.Server.Implementations.IStartupOptions options,
            Microsoft.Extensions.Configuration.IConfiguration startupConfig) 
            : base(applicationPaths, loggerFactory, options, startupConfig)
        {
        }

        public new List<Type> _creatingInstances { get; set; }
        public new Emby.Server.Implementations.PluginManager _pluginManager { get; set; }

        protected override IEnumerable<Assembly> GetAssembliesWithPartsInternal()
        {
            yield return typeof(ApplicationHost).Assembly;
        }
    }
}
