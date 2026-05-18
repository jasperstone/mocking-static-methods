using System;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans.Transactions.TestKit;
using Xunit;

namespace Orleans.Transactions.TestKit.Tests
{
    public class SimpleAzureStorageExceptionInjectorTests
    {
        private readonly Mock<ILogger<SimpleAzureStorageExceptionInjector>> _loggerMock;
        private readonly SimpleAzureStorageExceptionInjector _injector;

        public SimpleAzureStorageExceptionInjectorTests()
        {
            _loggerMock = new Mock<ILogger<SimpleAzureStorageExceptionInjector>>();
            _injector = new SimpleAzureStorageExceptionInjector(_loggerMock.Object);
        }

        [Fact]
        public void AfterStore_InjectAfterStoreFalse_DoesNotLogOrThrow()
        {
            // Arrange
            _injector.InjectAfterStore = false;

            // Act
            _injector.AfterStore();

            // Assert
            _loggerMock.VerifyLogInformationNotCalled();
        }

        [Fact]
        public void AfterStore_InjectAfterStoreTrue_LogsMessageAndThrows()
        {
            // Arrange
            _injector.InjectAfterStore = true;

            // Act & Assert
            var exception = Assert.Throws<SimpleAzureStorageException>(() => _injector.AfterStore());
            Assert.Contains("Storage exception thrown after store, thrown total 1", exception.Message);
            _loggerMock.VerifyLogInformationCalledOnceWithMessageContaining("Storage exception thrown after store, thrown total 1");
            Assert.False(_injector.InjectAfterStore); // Should be reset to false
        }

        [Fact]
        public void BeforeStore_InjectBeforeStoreFalse_DoesNotLogOrThrow()
        {
            // Arrange
            _injector.InjectBeforeStore = false;

            // Act
            _injector.BeforeStore();

            // Assert
            _loggerMock.VerifyLogInformationNotCalled();
        }

        [Fact]
        public void BeforeStore_InjectBeforeStoreTrue_LogsMessageAndThrows()
        {
            // Arrange
            _injector.InjectBeforeStore = true;

            // Act & Assert
            var exception = Assert.Throws<SimpleAzureStorageException>(() => _injector.BeforeStore());
            Assert.Contains("Storage exception thrown before store. Thrown total 1", exception.Message);
            _loggerMock.VerifyLogInformationCalledOnceWithMessageContaining("Storage exception thrown before store. Thrown total 1");
            Assert.False(_injector.InjectBeforeStore); // Should be reset to false
        }

        [Fact]
        public void AfterStore_MultipleCalls_LogsCorrectCounter()
        {
            // Arrange
            _injector.InjectAfterStore = true;

            // Act & Assert - first call
            var ex1 = Assert.Throws<SimpleAzureStorageException>(() => _injector.AfterStore());
            _loggerMock.VerifyLogInformationCalledOnceWithMessageContaining("thrown total 1");

            // Reset for second call
            _injector.InjectAfterStore = true;
            _loggerMock.Invocations.Clear();

            // Act & Assert - second call
            var ex2 = Assert.Throws<SimpleAzureStorageException>(() => _injector.AfterStore());
            _loggerMock.VerifyLogInformationCalledOnceWithMessageContaining("thrown total 2");
        }
    }

    // Extension methods for Mock verification
    public static class MockLoggerExtensions
    {
        public static void VerifyLogInformationNotCalled(this Mock<ILogger<SimpleAzureStorageExceptionInjector>> mock)
        {
            mock.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("thrown")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
        }

        public static void VerifyLogInformationCalledOnceWithMessageContaining(this Mock<ILogger<SimpleAzureStorageExceptionInjector>> mock, string expectedSubstring)
        {
            mock.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedSubstring)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }
    }
}
