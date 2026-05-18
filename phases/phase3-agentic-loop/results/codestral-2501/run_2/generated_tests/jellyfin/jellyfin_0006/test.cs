using System;
using System.Collections.Generic;
using Emby.Server.Implementations;
using Emby.Server.Implementations.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests
{
    public class TestableApplicationHost : ApplicationHost
    {
        public TestableApplicationHost(
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
    }

    public class ApplicationHostTests
    {
        [Fact]
        public void CreateInstanceSafe_DetectsDILoop_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var pluginManagerMock = new Mock<PluginManager>(MockBehavior.Strict, null, null, null, null, null);
            var serviceProviderMock = new Mock<IServiceProvider>();

            var applicationHost = new TestableApplicationHost(null, null, null, null)
            {
                Logger = loggerMock.Object,
                _pluginManager = pluginManagerMock.Object,
                ServiceProvider = serviceProviderMock.Object
            };

            var type = typeof(ApplicationHost);

            // Act
            applicationHost.CreateInstanceSafe(type);

            // Assert
            loggerMock.Verify(
                x => x.LogError("DI Loop detected in the attempted creation of {Type}", type.FullName),
                Times.Once);

            loggerMock.Verify(
                x => x.LogError("Called from: {TypeName}", type.FullName),
                Times.Once);

            pluginManagerMock.Verify(
                x => x.FailPlugin(type.Assembly),
                Times.Once);

            Assert.Throws<TypeLoadException>(() => applicationHost.CreateInstanceSafe(type));
        }

        [Fact]
        public void CreateInstanceSafe_HandlesException_LogsErrorAndFailsPlugin()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var pluginManagerMock = new Mock<PluginManager>(MockBehavior.Strict, null, null, null, null, null);
            var serviceProviderMock = new Mock<IServiceProvider>();

            var applicationHost = new TestableApplicationHost(null, null, null, null)
            {
                Logger = loggerMock.Object,
                _pluginManager = pluginManagerMock.Object,
                ServiceProvider = serviceProviderMock.Object
            };

            var type = typeof(ApplicationHost);

            // Act
            applicationHost.CreateInstanceSafe(type);

            // Assert
            loggerMock.Verify(
                x => x.LogError(It.IsAny<Exception>(), "Error creating {Type}", type),
                Times.Once);

            pluginManagerMock.Verify(
                x => x.FailPlugin(type.Assembly),
                Times.Once);
        }
    }
}
