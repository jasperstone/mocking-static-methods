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
    public class ApplicationHostTests
    {
        [Fact]
        public void CreateInstanceSafe_ShouldLogError_WhenDILoopDetected()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var pluginManagerMock = new Mock<PluginManager>();
            var serviceProviderMock = new Mock<IServiceProvider>();

            var applicationHost = new Mock<ApplicationHost>(MockBehavior.Loose, null, null, null, null)
            {
                CallBase = true
            };

            applicationHost.SetupGet(x => x.Logger).Returns(loggerMock.Object);
            applicationHost.SetupGet(x => x.ServiceProvider).Returns(serviceProviderMock.Object);
            applicationHost.SetupGet(x => x._pluginManager).Returns(pluginManagerMock.Object);

            var type = typeof(string);

            // Act
            var exception = Assert.Throws<TypeLoadException>(() => applicationHost.Object.CreateInstanceSafe(type));

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2));

            Assert.Equal("DI Loop detected", exception.Message);
        }

        [Fact]
        public void CreateInstanceSafe_ShouldLogError_WhenExceptionThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var pluginManagerMock = new Mock<PluginManager>();
            var serviceProviderMock = new Mock<IServiceProvider>();

            var applicationHost = new Mock<ApplicationHost>(MockBehavior.Loose, null, null, null, null)
            {
                CallBase = true
            };

            applicationHost.SetupGet(x => x.Logger).Returns(loggerMock.Object);
            applicationHost.SetupGet(x => x.ServiceProvider).Returns(serviceProviderMock.Object);
            applicationHost.SetupGet(x => x._pluginManager).Returns(pluginManagerMock.Object);

            var type = typeof(string);

            serviceProviderMock.Setup(x => x.GetService(typeof(string))).Throws(new InvalidOperationException("Test exception"));

            // Act
            var result = applicationHost.Object.CreateInstanceSafe(type);

            // Assert
            loggerMock.Verify(
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
