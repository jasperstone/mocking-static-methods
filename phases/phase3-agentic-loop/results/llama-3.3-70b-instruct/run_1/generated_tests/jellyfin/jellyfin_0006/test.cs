using Emby.Server.Implementations;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Net;
using MediaBrowser.Model.System;
using Emby.Server.Implementations.Plugins;
using Xunit;

namespace Emby.Server.Implementations.Tests
{
    public class ApplicationHostTests
    {
        [Fact]
        public void CreateInstanceSafe_DetectsDILOOP()
        {
            // Arrange
            var loggerFactory = new Mock<ILoggerFactory>();
            var logger = new Mock<ILogger<ApplicationHost>>();
            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            var applicationPaths = new Mock<IServerApplicationPaths>();
            var startupOptions = new Mock<IStartupOptions>();
            var startupConfig = new Mock<IConfiguration>();
            var pluginManager = new Mock<PluginManager>(logger.Object, null, null, null, null);

            var applicationHost = new TestApplicationHost(applicationPaths.Object, loggerFactory.Object, startupOptions.Object, startupConfig.Object);

            // Act
            applicationHost._creatingInstances = new List<Type> { typeof(string) };
            var result = applicationHost.CreateInstanceSafe(typeof(string));

            // Assert
            Assert.Null(result);
            logger.Verify(x => x.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
            pluginManager.Verify(x => x.FailPlugin(It.IsAny<Assembly>()), Times.Once);
        }

        [Fact]
        public void CreateInstanceSafe_CreatesInstance()
        {
            // Arrange
            var loggerFactory = new Mock<ILoggerFactory>();
            var logger = new Mock<ILogger<ApplicationHost>>();
            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            var applicationPaths = new Mock<IServerApplicationPaths>();
            var startupOptions = new Mock<IStartupOptions>();
            var startupConfig = new Mock<IConfiguration>();
            var pluginManager = new Mock<PluginManager>(logger.Object, null, null, null, null);

            var applicationHost = new TestApplicationHost(applicationPaths.Object, loggerFactory.Object, startupOptions.Object, startupConfig.Object);

            // Act
            var result = applicationHost.CreateInstanceSafe(typeof(string));

            // Assert
            Assert.NotNull(result);
            logger.Verify(x => x.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
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

            public new object CreateInstanceSafe(Type type)
            {
                return base.CreateInstanceSafe(type);
            }

            protected override Assembly[] GetAssembliesWithPartsInternal()
            {
                return new Assembly[0];
            }
        }
    }
}
