using Emby.Server.Implementations;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Emby.Server.Implementations.Tests
{
    public class ApplicationHostTests
    {
        [Fact]
        public void CreateInstanceSafe_LogsErrorAndFailsPlugin_WhenDILoopDetected()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<ApplicationHost>();
            var pluginManagerMock = new Moq.Mock<Emby.Server.Implementations.Plugins.PluginManager>(Moq.MockBehavior.Strict, loggerFactory.CreateLogger<Emby.Server.Implementations.Plugins.PluginManager>(), new ApplicationHost(new Emby.Server.Implementations.ServerApplicationPaths(), loggerFactory, new Emby.Server.Implementations.StartupOptions(), new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build()), new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build(), new Emby.Server.Implementations.ServerApplicationPaths().PluginsPath, new Version());
            var applicationHost = new TestApplicationHost(loggerFactory, pluginManagerMock.Object);

            applicationHost._creatingInstances = new List<Type> { typeof(string) };

            // Act and Assert
            Assert.Throws<TypeLoadException>(() => applicationHost.CreateInstanceSafe(typeof(string)));
            pluginManagerMock.Verify(pm => pm.FailPlugin(It.IsAny<Assembly>()), Times.Once);
        }

        [Fact]
        public void CreateInstanceSafe_LogsErrorAndFailsPlugin_WhenExceptionThrown()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<ApplicationHost>();
            var pluginManagerMock = new Moq.Mock<Emby.Server.Implementations.Plugins.PluginManager>(Moq.MockBehavior.Strict, loggerFactory.CreateLogger<Emby.Server.Implementations.Plugins.PluginManager>(), new ApplicationHost(new Emby.Server.Implementations.ServerApplicationPaths(), loggerFactory, new Emby.Server.Implementations.StartupOptions(), new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build()), new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build(), new Emby.Server.Implementations.ServerApplicationPaths().PluginsPath, new Version());
            var applicationHost = new TestApplicationHost(loggerFactory, pluginManagerMock.Object);

            // Act and Assert
            var result = applicationHost.CreateInstanceSafe(typeof(object));
            Assert.Null(result);
            pluginManagerMock.Verify(pm => pm.FailPlugin(It.IsAny<Assembly>()), Times.Once);
        }

        private class TestApplicationHost : ApplicationHost
        {
            public TestApplicationHost(ILoggerFactory loggerFactory, Emby.Server.Implementations.Plugins.PluginManager pluginManager)
                : base(new Emby.Server.Implementations.ServerApplicationPaths(), loggerFactory, new Emby.Server.Implementations.StartupOptions(), new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build())
            {
                _pluginManager = pluginManager;
                _creatingInstances = new List<Type>();
            }

            public new object CreateInstanceSafe(Type type)
            {
                return base.CreateInstanceSafe(type);
            }

            protected override System.Collections.Generic.IEnumerable<System.Reflection.Assembly> GetAssembliesWithPartsInternal()
            {
                return new System.Reflection.Assembly[0];
            }
        }
    }
}
