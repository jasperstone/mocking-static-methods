using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server.Tests
{
    public class VectorManagerTests
    {
        [Fact]
        public void LogError_ShouldBeCalled_WhenExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var vectorManager = new VectorManager(1, new GarnetServerOptions(), () => null, null);

            // Act
            try
            {
                throw new InvalidOperationException("Test exception");
            }
            catch (Exception ex)
            {
                vectorManager.LogError(loggerMock.Object, ex, "Attempt at normal cleanup of {key} failed", "testKey");
            }

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<Exception>(),
                    "Attempt at normal cleanup of {key} failed",
                    It.Is<object[]>(args => args[0].ToString() == "testKey")),
                Times.Once);
        }
    }
}
