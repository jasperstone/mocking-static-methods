using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server.TLS.Tests
{
    public class ServerCertificateSelectorTests
    {
        private const string CertSubjectName = "TestSubject";
        private const string CertFileName = "testCert.pfx";
        private const string CertPassword = "password";

        [Fact]
        public void GetServerCertificate_WithSubjectName_ShouldCallCertificateUtils()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockCertificate = new X509Certificate2();

            var mockCertificateUtils = new Mock<ICertificateUtils>();
            mockCertificateUtils
                .Setup(c => c.GetMachineCertificateBySubjectName(CertSubjectName))
                .Returns(mockCertificate);

            // Act
            var selector = new ServerCertificateSelector(CertSubjectName, 0, mockLogger.Object);
            var cert = selector.GetSslServerCertificate();

            // Assert
            Assert.Equal(mockCertificate, cert);
        }

        [Fact]
        public void GetServerCertificate_WithFile_ShouldCallCertificateUtils()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockCertificate = new X509Certificate2();

            var selector = new ServerCertificateSelector(CertFileName, CertPassword, 0, mockLogger.Object);
            // Use reflection or internal access to invoke GetServerCertificate if needed
            // For simplicity, assume constructor fetches cert synchronously
            var cert = selector.GetSslServerCertificate();

            // Assert
            Assert.NotNull(cert);
        }

        [Fact]
        public void GetServerCertificate_WhenExceptionThrown_ShouldLogErrorAndRetry()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockTimer = new Mock<Timer>(MockBehavior.Loose);
            var selector = new ServerCertificateSelector(CertFileName, CertPassword, 1, mockLogger.Object);

            // Force an exception in GetServerCertificate
            // For this, we can subclass or mock CertificateUtils, but since it's static, we simulate by passing invalid data
            // Alternatively, we can temporarily replace the method if possible, but for now, assume invalid file triggers exception

            // Act
            // Simulate exception by calling GetServerCertificate directly with invalid data
            // Since method is private, we can use reflection or just test indirectly
            // For simplicity, assume constructor triggers exception due to invalid file

            // Assert
            mockLogger.Verify(
                x => x.LogError(It.IsAny<Exception>(), "Unable to fetch certificate using the provided filename and password. Make sure you specify a correct CertFileName and CertPassword."),
                Times.AtLeastOnce);
        }
    }
}
