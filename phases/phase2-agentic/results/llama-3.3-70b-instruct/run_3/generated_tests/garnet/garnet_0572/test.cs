using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Xunit;

namespace Garnet.server.TLS.Tests
{
    public class ServerCertificateSelectorTests
    {
        [Fact]
        public void GetServerCertificate_LogsError_WhenCertificateCannotBeLoaded()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var serverCertificateSelector = new ServerCertificateSelector("InvalidSubjectName", 0, loggerMock.Object);

            // Act
            serverCertificateSelector.GetServerCertificate(null);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Unable to fetch certificate using the provided filename and password. Make sure you specify a correct CertFileName and CertPassword."), Times.Once);
        }

        [Fact]
        public void GetServerCertificate_LogsErrorAndReschedulesTimer_WhenCertificateCannotBeLoadedAndRefreshIsEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var serverCertificateSelector = new ServerCertificateSelector("InvalidSubjectName", 10, loggerMock.Object);

            // Act
            serverCertificateSelector.GetServerCertificate(null);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Unable to fetch certificate. It will be retried after {certificateRefreshRetryInterval}", serverCertificateSelector.certificateRefreshRetryInterval), Times.Once);
        }

        [Fact]
        public void GetServerCertificate_LoadsCertificate_WhenSubjectNameIsValid()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var serverCertificateSelector = new ServerCertificateSelector("ValidSubjectName", 0, loggerMock.Object);

            // Act
            serverCertificateSelector.GetServerCertificate(null);

            // Assert
            Assert.NotNull(serverCertificateSelector.GetSslServerCertificate());
        }

        [Fact]
        public void GetServerCertificate_LoadsCertificate_WhenFileNameAndPasswordAreValid()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var serverCertificateSelector = new ServerCertificateSelector("ValidFileName", "ValidPassword", 0, loggerMock.Object);

            // Act
            serverCertificateSelector.GetServerCertificate(null);

            // Assert
            Assert.NotNull(serverCertificateSelector.GetSslServerCertificate());
        }
    }
}
