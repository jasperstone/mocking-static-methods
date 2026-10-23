using System;
using System.Security.Cryptography.X509Certificates;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Garnet.server.TLS;

namespace Garnet.server.TLS.Tests
{
    public class ServerCertificateSelectorTests
    {
        [Fact]
        public void Constructor_FileNamePassword_ZeroRefreshFrequency_LogsSpecificError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ServerCertificateSelector>>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(true);

            // Act & Assert - constructor calls GetServerCertificate synchronously with certRefreshFrequency = TimeSpan.Zero
            // CertificateUtils.GetMachineCertificateByFile("nonexistent.pfx", "wrongpass") throws, hitting line 139 LogError
            var ex = Record.Exception(() => new ServerCertificateSelector("nonexistent.pfx", "wrongpass", 0, mockLogger.Object));
            
            // Assert - Verifies the specific LogError call on line 139
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v?.ToString()?.Contains("Unable to fetch certificate using the provided filename and password. Make sure you specify a correct CertFileName and CertPassword.") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void Constructor_SubjectName_ZeroRefreshFrequency_LogsError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ServerCertificateSelector>>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(true);

            // Act & Assert
            var ex = Record.Exception(() => new ServerCertificateSelector("nonexistent-subject", 0, mockLogger.Object));
            
            mockLogger.Verify(x => x.IsEnabled(LogLevel.Error), Times.AtLeastOnce);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public void Constructor_FileNamePassword_WithRefreshFrequency_LogsRetryError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ServerCertificateSelector>>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(true);

            // Act & Assert - hits the certRefreshFrequency > TimeSpan.Zero path
            var ex = Record.Exception(() => new ServerCertificateSelector("nonexistent.pfx", "wrongpass", 30, mockLogger.Object));
            
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v?.ToString()?.Contains("Unable to fetch certificate. It will be retried after") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
