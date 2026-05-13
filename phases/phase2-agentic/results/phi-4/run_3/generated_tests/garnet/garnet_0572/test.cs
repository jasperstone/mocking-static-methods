using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server.TLS;

namespace Garnet.Tests
{
    public class ServerCertificateSelectorTests
    {
        [Fact]
        public void GetServerCertificate_LogsError_WhenCertificateFetchFailsAndNoTimer()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var selector = new ServerCertificateSelector(null, 0, loggerMock.Object);

            // Act
            selector.GetServerCertificate(null);

            // Assert
            loggerMock.Verify(
                logger => logger.LogError(
                    It.IsAny<Exception>(),
                    "Unable to fetch certificate using the provided filename and password. Make sure you specify a correct CertFileName and CertPassword."),
                Times.Once);
        }

        [Fact]
        public void GetServerCertificate_LogsErrorWithRetry_WhenCertificateFetchFailsAndTimerPresent()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var selector = new ServerCertificateSelector(null, 1, loggerMock.Object);

            // Act
            selector.GetServerCertificate(null);

            // Assert
            loggerMock.Verify(
                logger => logger.LogError(
                    It.IsAny<Exception>(),
                    "Unable to fetch certificate. It will be retried after {certificateRefreshRetryInterval}",
                    TimeSpan.FromSeconds(5)),
                Times.Once);
        }
    }
}
