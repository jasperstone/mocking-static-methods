using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server.TLS.Tests
{
    public class ServerCertificateSelectorTests
    {
        [Fact]
        public void Constructor_FileNamePassword_ZeroRefreshFrequency_LogsErrorOnCertificateLoadFailure()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ServerCertificateSelector>>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(true);

            // Act
            _ = new ServerCertificateSelector(
                fileName: "nonexistent.pfx",
                filePassword: "wrongpassword",
                certRefreshFrequencyInSeconds: 0,
                logger: mockLogger.Object);

            // Assert - verify the specific LogError call (line 139 path)
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString()!.Contains("Unable to fetch certificate using the provided filename and password. Make sure you specify a correct CertFileName and CertPassword.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void Constructor_FileNamePassword_ZeroRefreshFrequency_ServerCertificateIsNullOnFailure()
        {
            // Act
            var selector = new ServerCertificateSelector(
                fileName: "nonexistent.pfx",
                filePassword: "wrongpassword",
                certRefreshFrequencyInSeconds: 0,
                logger: Mock.Of<ILogger<ServerCertificateSelector>>());

            // Assert
            Assert.Null(selector.GetSslServerCertificate());
        }

        [Fact]
        public void Constructor_SubjectName_ZeroRefreshFrequency_ServerCertificateIsNullOnFailure()
        {
            // Act
            var selector = new ServerCertificateSelector(
                subjectName: "nonexistent-subject",
                certRefreshFrequencyInSeconds: 0,
                logger: Mock.Of<ILogger<ServerCertificateSelector>>());

            // Assert
            Assert.Null(selector.GetSslServerCertificate());
        }
    }
}
