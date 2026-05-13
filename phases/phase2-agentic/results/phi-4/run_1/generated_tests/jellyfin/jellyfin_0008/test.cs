using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests
{
    public class ApplicationHostTests
    {
        [Fact]
        public void CreateInstanceSafe_LogsError_WhenExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var pluginManagerMock = new Mock<IPluginManager>();

            var applicationHost = new ApplicationHost(
                Mock.Of<IServerApplicationPaths>(),
                Mock.Of<ILoggerFactory>(),
                Mock.Of<IStartupOptions>(),
                Mock.Of<IConfiguration>())
            {
                Logger = loggerMock.Object,
                ServiceProvider = serviceProviderMock.Object,
                _pluginManager = pluginManagerMock.Object
            };

            var typeToCreate = typeof(object);
            var exception = new Exception("Test exception");

            serviceProviderMock.Setup(s => s.GetService(It.IsAny<Type>()))
                .Throws(exception);

            // Act
            applicationHost.CreateInstanceSafe(typeToCreate);

            // Assert
            loggerMock.Verify(
                l => l.LogError(
                    It.IsAny<Exception>(),
                    "Error creating {Type}",
                    typeToCreate),
                Times.Once);

            pluginManagerMock.Verify(
                p => p.FailPlugin(typeToCreate.Assembly),
                Times.Once);
        }
    }
}
