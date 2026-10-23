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
            var fileName = "invalidfile.pfx";
            var filePassword = "wrongpassword";

            // Act
            var selector = new ServerCertificateSelector(fileName, filePassword, 0, loggerMock.Object);

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
            var fileName = "invalidfile.pfx";
            var filePassword = "wrongpassword";
            int refreshFrequencySeconds = 1;

            // Act
            var selector = new ServerCertificateSelector(fileName, filePassword, refreshFrequencySeconds, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    "Unable to fetch certificate. It will be retried after {certificateRefreshRetryInterval}",
                    It.IsAny<TimeSpan>()),
                Times.AtLeastOnce);
        }
    }
}
