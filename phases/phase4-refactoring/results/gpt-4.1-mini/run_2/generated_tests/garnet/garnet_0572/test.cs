using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server.TLS;

namespace Garnet.Tests.TLS
{
    public class ServerCertificateSelectorTests
    {
        [Fact]
        public void Constructor_WithFileNameAndPassword_LogsErrorWhenCertificateFetchFails_NoRefreshTimer()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();

            // Act
            var selector = new ServerCertificateSelector("nonexistentfile.pfx", "wrongpassword", 0, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    "Unable to fetch certificate using the provided filename and password. Make sure you specify a correct CertFileName and CertPassword."),
                Times.Once);
        }

        [Fact]
        public void Constructor_WithFileNameAndPassword_LogsErrorWhenCertificateFetchFails_WithRefreshTimer()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();

            // Act
            var selector = new ServerCertificateSelector("nonexistentfile.pfx", "wrongpassword", 1, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    "Unable to fetch certificate. It will be retried after {certificateRefreshRetryInterval}",
                    It.IsAny<TimeSpan>()),
                Times.Once);
        }
    }
}
