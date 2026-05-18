using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Emby.Server.Implementations;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Emby.Server.Implementations.Tests
{
    public class ApplicationHostTests
    {
        [Fact]
        public void CreateInstanceSafe_ExceptionLogged()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ApplicationHost>>();
            var mockPluginManager = new Mock<PluginManager>(Mock.Of<ILogger<PluginManager>>(), Mock.Of<IServerApplicationHost>(), null, string.Empty, new Version());
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockConfiguration = new Mock<IConfiguration>();

            var applicationHost = new Mock<ApplicationHost>(
                Mock.Of<IServerApplicationPaths>(),
                Mock.Of<ILoggerFactory>(),
                Mock.Of<IStartupOptions>(),
                mockConfiguration.Object) { CallBase = true };

            applicationHost.SetupGet(x => x.Logger).Returns(mockLogger.Object);
            applicationHost.SetupGet(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
            applicationHost.SetupGet(x => x._pluginManager).Returns(mockPluginManager.Object);

            var testType = typeof(string);

            // Act
            var result = applicationHost.Object.CreateInstanceSafe(testType);

            // Assert
            mockLogger.Verify(
                x => x.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.Null(result);
        }
    }
}
