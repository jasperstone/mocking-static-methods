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
        public void Constructor_LogsError_WhenCertificateUtilsFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>())).Verifiable();

            // Act
            try
            {
                new ServerCertificateSelector("testSubjectName", 0, loggerMock.Object);
            }
            catch (Exception)
            {
                // Expected
            }

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Unable to fetch certificate using the provided filename and password. Make sure you specify a correct CertFileName and CertPassword."), Times.Once);
        }

        [Fact]
        public void Constructor_LogsErrorWithRetry_WhenCertificateUtilsFailsAndRetryIsEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>())).Verifiable();

            // Act
            try
            {
                new ServerCertificateSelector("testSubjectName", 10, loggerMock.Object);
            }
            catch (Exception)
            {
                // Expected
            }

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Unable to fetch certificate. It will be retried after {certificateRefreshRetryInterval}", It.IsAny<object>()), Times.Once);
        }
    }
}
