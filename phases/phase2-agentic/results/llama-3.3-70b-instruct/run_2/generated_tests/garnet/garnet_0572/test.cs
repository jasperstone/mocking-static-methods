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
            var certificateUtilsMock = new Mock<ICertificateUtils>();
            certificateUtilsMock.Setup(c => c.GetMachineCertificateBySubjectName(It.IsAny<string>())).Throws(new Exception("Test exception"));
            var serverCertificateSelector = new ServerCertificateSelector("testSubjectName", 0, loggerMock.Object);

            // Act
            serverCertificateSelector.GetServerCertificate(null);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Unable to fetch certificate using the provided filename and password. Make sure you specify a correct CertFileName and CertPassword."), Times.Once);
        }

        [Fact]
        public void GetServerCertificate_LogsErrorWithRetry_WhenCertificateUtilsFailsAndRetryIsEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var certificateUtilsMock = new Mock<ICertificateUtils>();
            certificateUtilsMock.Setup(c => c.GetMachineCertificateBySubjectName(It.IsAny<string>())).Throws(new Exception("Test exception"));
            var serverCertificateSelector = new ServerCertificateSelector("testSubjectName", 10, loggerMock.Object);

            // Act
            serverCertificateSelector.GetServerCertificate(null);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Unable to fetch certificate. It will be retried after {certificateRefreshRetryInterval}", serverCertificateSelector.certificateRefreshRetryInterval), Times.Once);
        }

        [Fact]
        public void GetServerCertificate_DoesNotLogError_WhenCertificateUtilsSucceeds()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var certificateUtilsMock = new Mock<ICertificateUtils>();
            certificateUtilsMock.Setup(c => c.GetMachineCertificateBySubjectName(It.IsAny<string>())).Returns(new X509Certificate2());
            var serverCertificateSelector = new ServerCertificateSelector("testSubjectName", 0, loggerMock.Object);

            // Act
            serverCertificateSelector.GetServerCertificate(null);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }
    }

    public interface ICertificateUtils
    {
        X509Certificate2 GetMachineCertificateBySubjectName(string subjectName);
    }

    public class CertificateUtilsWrapper : ICertificateUtils
    {
        public X509Certificate2 GetMachineCertificateBySubjectName(string subjectName)
        {
            return CertificateUtils.GetMachineCertificateBySubjectName(subjectName);
        }
    }
}
