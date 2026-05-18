using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Microsoft.Extensions.Logging.LoggerExtensionsTests
{
    public class LogInformationTests
    {
        private readonly Mock<ILogger> _loggerMock;

        public LogInformationTests()
        {
            _loggerMock = new Mock<ILogger>();
        }

        [Fact]
        public void LogInformation_WithExceptionAndArguments_CallsLogWithCorrectParameters()
        {
            // Arrange
            var exception = new Exception("Test exception");
            var message = "Error during recovery of store; storeVersion = {storeVersion}; objectStoreVersion = {objectStoreVersion}";
            var storeVersion = 123L;
            var objectStoreVersion = 456L;

            // Act
            _loggerMock.Object.LogInformation(exception, message, storeVersion, objectStoreVersion);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogInformation_WithExceptionAndPathArguments_CallsLogWithCorrectParameters()
        {
            // Arrange
            var exception = new InvalidDataException("Test error");
            var message = "Error during recovery of database ids; checkpointParentDir = {checkpointParentDir}; checkpointDirBaseName = {checkpointDirBaseName}";
            var checkpointParentDir = "/test/path";
            var checkpointDirBaseName = "checkpoint0";

            // Act
            _loggerMock.Object.LogInformation(exception, message, checkpointParentDir, checkpointDirBaseName);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogInformation_WithoutException_CallsLogWithCorrectParameters()
        {
            // Arrange
            var message = "Main store and object store checkpoint versions do not match; storeVersion = {storeVersion}; objectStoreVersion = {objectStoreVersion}";
            var storeVersion = 1L;
            var objectStoreVersion = 2L;

            // Act
            _loggerMock.Object.LogInformation(message, storeVersion, objectStoreVersion);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(e => e == null),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
