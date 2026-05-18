using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Implementations; // Ensure this using directive is correct

namespace Jellyfin.Server.Implementations.Tests
{
    public class ApplicationHostTests
    {
        [Fact]
        public void CreateInstanceSafe_DILoopDetected_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var applicationHost = new ApplicationHost(loggerMock.Object, serviceProviderMock.Object);

            var type = typeof(object);
            applicationHost._creatingInstances = new List<Type> { type };

            // Act
            applicationHost.CreateInstanceSafe(type);

            // Assert
            loggerMock.Verify(
                l => l.LogError(
                    It.Is<string>(s => s == "DI Loop detected in the attempted creation of {Type}"),
                    It.Is<object>(o => o.ToString() == type.FullName)),
                Times.Once);

            loggerMock.Verify(
                l => l.LogError(
                    It.Is<string>(s => s == "Called from: {TypeName}"),
                    It.Is<object>(o => o.ToString() == type.FullName)),
                Times.Once);
        }
    }
}
