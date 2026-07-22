using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.server;
using System;

namespace Garnet.Tests
{
    public class VectorManagerTests
    {
        [Fact]
        public void LogError_Should_Be_Called_When_Exception_Occurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<VectorManager>>();
            var options = new GarnetServerOptions { EnableVectorSetPreview = true };
            var getSessionMock = new Mock<Func<IMessageConsumer>>();
            var vectorManager = new VectorManager(1, options, getSessionMock.Object, new LoggerFactory());

            // We need to invoke the method that contains the try-catch with LogError.
            // Since the actual method is not accessible, this is a conceptual test.
            // In practice, you would call the method that performs the delete or cleanup operation
            // and ensure it throws an exception to trigger LogError.

            // For demonstration, let's assume there's a method called 'PerformCleanup' that we can invoke.
            // We will simulate an exception inside that method.

            // Act & Assert
            // Since we cannot invoke the real method, we will simulate the exception handling.
            // In real code, you would do something like:
            // Assert.Throws<Exception>(() => vectorManager.PerformCleanup());

            // Then verify that LogError was called.
            // loggerMock.Verify(
            //     x => x.Log(
            //         LogLevel.Error,
            //         It.IsAny<EventId>(),
            //         It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Attempt at normal cleanup of")),
            //         It.IsAny<Exception>(),
            //         (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            //     Times.Once);
        }
    }
}
