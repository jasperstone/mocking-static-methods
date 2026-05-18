using Moq;
using Moq.Protected;
using System;
using Xunit;
using Microsoft.Extensions.Logging;
using Garnet.server.TLS;
using System.Linq.Expressions;

namespace Garnet.server.TLS.Tests
{
    public class ServerCertificateSelectorTests
    {
        [Fact]
        public void Constructor_FileMode_NoRefreshTimer_LogsErrorOnCertificateFailure()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ServerCertificateSelector>>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Error)).Returns(true);

            // Mock CertificateUtils to throw exception
            var mockCertificateUtils = new Mock<ILogger<ServerCertificateSelector>>();
            // Act & Assert - constructor calls GetServerCertificate which fails and logs
            var selector = new ServerCertificateSelector("invalid.pfx", "wrongpassword", 0, mockLogger.Object);

            // Assert - verify the specific error message was logged (line 139 path)
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to fetch certificate using the provided filename and password. Make sure you specify a correct CertFileName and CertPassword.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void Constructor_SubjectNameMode_NoRefreshTimer_LogsErrorOnCertificateFailure()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ServerCertificateSelector>>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Error)).Returns(true);

            // Act & Assert - constructor calls GetServerCertificate which fails and logs
            var selector = new ServerCertificateSelector("NonExistentSubject", 0, mockLogger.Object);

            // Assert - verify the specific error message was logged (line 139 path)
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to fetch certificate using the provided filename and password. Make sure you specify a correct CertFileName and CertPassword.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void Constructor_FileMode_WithRefreshTimer_LogsDifferentError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ServerCertificateSelector>>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Error)).Returns(true);

            // Act & Assert - with timer (>0 seconds), uses different log message path
            var selector = new ServerCertificateSelector("invalid.pfx", "wrongpassword", 30, mockLogger.Object);

            // Assert - verify the timer path error message was logged
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to fetch certificate. It will be retried after")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
