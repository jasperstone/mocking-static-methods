using System;
using System.Collections.Generic;
using Emby.Server.Implementations;
using Emby.Server.Implementations.Plugins;
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
        public void CreateInstanceSafe_DetectsDILoop_LogsErrorAndFailsPlugin()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var pluginManagerMock = new Mock<PluginManager>();

            var applicationHost = new TestableApplicationHost(
                Mock.Of<IServerApplicationPaths>(),
                Mock.Of<ILoggerFactory>(),
                Mock.Of<IStartupOptions>(),
                Mock.Of<IConfiguration>());

            applicationHost.Logger = loggerMock.Object;
            applicationHost._pluginManager = pluginManagerMock.Object;

            var type = typeof(ApplicationHost); // Using ApplicationHost itself to create a loop

            // Act
            Assert.Throws<TypeLoadException>(() => applicationHost.CreateInstanceSafe(type));

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
        }
    }
}
