using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.PluginManager;

namespace Jellyfin.Database.Implementations.Tests
{
    public class ApplicationHostTests
    {
        [Fact]
        public void CreateInstanceSafe_LogsError_WhenExceptionOccurs()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var pluginManagerMock = new Mock<IPluginManager>();
            var applicationHost = new ApplicationHost(serviceProviderMock.Object, loggerMock.Object, pluginManagerMock.Object);

            var type = typeof(object);

            // Act
            applicationHost.CreateInstanceSafe(type);

            // Assert
            loggerMock.Verify(
                l => l.LogError(It.IsAny<Exception>(), "Error creating {Type}", type),
                Times.Once
            );
        }
    }
}
