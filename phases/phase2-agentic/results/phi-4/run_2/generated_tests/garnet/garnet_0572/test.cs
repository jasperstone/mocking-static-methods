using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server.TLS.Tests
{
    public class ServerCertificateSelectorTests
    {
        [Fact]
        public void GetServerCertificate_LogsError_WhenCertificateRefreshFrequencyIsZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var selector = new ServerCertificateSelector(null, null, 0, loggerMock.Object);

            // Act
            selector.GetServerCertificate(null);

            // Assert
            loggerMock.Verify(
                l => l.LogError(
                    It.IsAny<Exception>(),
                    "Unable to fetch certificate using the provided filename and password. Make sure you specify a correct CertFileName and CertPassword."),
                Times.Once);
        }

        [Fact]
        public void GetServerCertificate_LogsErrorAndSchedulesRetry_WhenCertificateRefreshFrequencyIsPositive()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var selector = new ServerCertificateSelector(null, 1, loggerMock.Object);

            // Act
            selector.GetServerCertificate(null);

            // Assert
            loggerMock.Verify(
                l => l.LogError(
                    It.IsAny<Exception>(),
                    "Unable to fetch certificate. It will be retried after {certificateRefreshRetryInterval}",
                    TimeSpan.FromSeconds(5)),
                Times.Once);
        }
    }
}
