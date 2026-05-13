using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogInformation_WithException_DelegatesToLogger()
        {
            var loggerMock = new Mock<ILogger>();
            var exception = new InvalidOperationException("boom");

            LoggerExtensions.LogInformation(
                loggerMock.Object,
                exception,
                "Error during recovery of store; storeVersion = {storeVersion}; objectStoreVersion = {objectStoreVersion}",
                42L,
                24L);

            loggerMock.Verify(logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, _) =>
                        state.ToString() ==
                        "Error during recovery of store; storeVersion = 42; objectStoreVersion = 24"),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogInformation_WithoutException_DelegatesToLogger()
        {
            var loggerMock = new Mock<ILogger>();

            LoggerExtensions.LogInformation(
                loggerMock.Object,
                "Main store and object store checkpoint versions do not match; storeVersion = {storeVersion}; objectStoreVersion = {objectStoreVersion}",
                11L,
                22L);

            loggerMock.Verify(logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, _) =>
                        state.ToString() ==
                        "Main store and object store checkpoint versions do not match; storeVersion = 11; objectStoreVersion = 22"),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
