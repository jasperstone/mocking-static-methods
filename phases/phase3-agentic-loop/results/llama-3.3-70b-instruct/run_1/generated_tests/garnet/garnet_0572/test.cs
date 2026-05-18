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
        public void GetServerCertificate_LogsError_WhenCertificateUtilsFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var serverCertificateSelector = new ServerCertificateSelector("testSubjectName", 0, loggerMock.Object);

            // Act
            try
            {
                serverCertificateSelector.GetSslServerCertificate();
            }
            catch (Exception)
            {
            }

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Unable to fetch certificate using the provided filename and password. Make sure you specify a correct CertFileName and CertPassword."), Times.Once);
        }

        [Fact]
        public void GetServerCertificate_LogsErrorWithRetry_WhenCertificateUtilsFailsAndRetryIsEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var serverCertificateSelector = new ServerCertificateSelector("testSubjectName", 10, loggerMock.Object);

            // Act
            try
            {
                serverCertificateSelector.GetSslServerCertificate();
            }
            catch (Exception)
            {
            }

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Unable to fetch certificate. It will be retried after {certificateRefreshRetryInterval}", TimeSpan.FromSeconds(5)), Times.Once);
        }
    }
}
