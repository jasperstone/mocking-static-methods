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
        public void CreateInstanceSafe_LogsError_OnDILOOP()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<ApplicationHost>();
            var pluginManager = new Mock<PluginManager>();
            var serviceProvider = new Mock<IServiceProvider>();
            var applicationHost = new TestApplicationHost(
                new Mock<MediaBrowser.Common.Configuration.IServerApplicationPaths>().Object,
                loggerFactory,
                new Mock<MediaBrowser.Common.Configuration.IStartupOptions>().Object,
                new Mock<Microsoft.Extensions.Configuration.IConfiguration>().Object)
            {
                Logger = logger,
                _pluginManager = pluginManager.Object,
                ServiceProvider = serviceProvider.Object
            };

            // Act and Assert
            applicationHost._creatingInstances = new List<Type> { typeof(string) };
            Assert.Throws<TypeLoadException>(() => applicationHost.CreateInstanceSafe(typeof(string)));
            pluginManager.Verify(pm => pm.FailPlugin(It.IsAny<Assembly>()), Times.Once);
        }

        [Fact]
        public void CreateInstanceSafe_LogsError_OnException()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<ApplicationHost>();
            var pluginManager = new Mock<PluginManager>();
            var serviceProvider = new Mock<IServiceProvider>();
            var applicationHost = new TestApplicationHost(
                new Mock<MediaBrowser.Common.Configuration.IServerApplicationPaths>().Object,
                loggerFactory,
                new Mock<MediaBrowser.Common.Configuration.IStartupOptions>().Object,
                new Mock<Microsoft.Extensions.Configuration.IConfiguration>().Object)
            {
                Logger = logger,
                _pluginManager = pluginManager.Object,
                ServiceProvider = serviceProvider.Object
            };

            // Act and Assert
            serviceProvider.Setup(sp => sp.GetService(It.IsAny<Type>())).Throws(new Exception());
            applicationHost.CreateInstanceSafe(typeof(string));
            pluginManager.Verify(pm => pm.FailPlugin(It.IsAny<Assembly>()), Times.Once);
        }
    }

    public class TestApplicationHost : ApplicationHost
    {
        public TestApplicationHost(
            MediaBrowser.Common.Configuration.IServerApplicationPaths applicationPaths,
            ILoggerFactory loggerFactory,
            MediaBrowser.Common.Configuration.IStartupOptions options,
            Microsoft.Extensions.Configuration.IConfiguration startupConfig)
            : base(applicationPaths, loggerFactory, options, startupConfig)
        {
        }

        protected override Assembly[] GetAssembliesWithPartsInternal()
        {
            return new Assembly[0];
        }
    }
}
