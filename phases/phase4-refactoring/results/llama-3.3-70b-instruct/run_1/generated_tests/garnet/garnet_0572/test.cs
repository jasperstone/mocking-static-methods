using Xunit;
using Moq;
using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;

namespace Garnet.server.TLS.Tests
{
    public class ServerCertificateSelectorTests
    {
        [Fact]
        public void Constructor_LogsError_WhenCertificateCannotBeFetched()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var serverCertificateSelector = new ServerCertificateSelector("subjectName", 0, loggerMock.Object);

            // Act
            serverCertificateSelector = new ServerCertificateSelector("subjectName", 0, loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Unable to fetch certificate using the provided filename and password. Make sure you specify a correct CertFileName and CertPassword."), Times.Once);
        }

        [Fact]
        public void Constructor_LogsError_WhenCertificateCannotBeFetched_WithRetry()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var serverCertificateSelector = new ServerCertificateSelector("subjectName", 10, loggerMock.Object);

            // Act
            serverCertificateSelector = new ServerCertificateSelector("subjectName", 10, loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Unable to fetch certificate. It will be retried after {certificateRefreshRetryInterval}", TimeSpan.FromSeconds(5)), Times.Once);
        }
    }
}
