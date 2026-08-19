using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server.TLS;

namespace Garnet.server.TLS.Tests
{
    public class ServerCertificateSelectorTests
    {
        [Fact]
        public void Constructor_FileMode_NoRefresh_LogsSpecificFilePasswordError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ServerCertificateSelector>>();

            // Act - constructor calls GetServerCertificate synchronously with certRefreshFrequency = TimeSpan.Zero
            // which throws and hits the else branch (line 139)
            _ = new ServerCertificateSelector(
                fileName: "nonexistent.pfx",
                filePassword: "wrongpassword",
                certRefreshFrequencyInSeconds: 0,
                logger: loggerMock.Object);

            // Assert - verifies LogError call on line 139
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void Constructor_SubjectNameMode_NoRefresh_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ServerCertificateSelector>>();

            // Act - constructor calls GetServerCertificate synchronously with certRefreshFrequency = TimeSpan.Zero
            // throws from GetMachineCertificateBySubjectName and hits line 139
            _ = new ServerCertificateSelector(
                subjectName: "nonexistent-subject",
                certRefreshFrequencyInSeconds: 0,
                logger: loggerMock.Object);

            // Assert - verifies LogError call on line 139
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void Constructor_FileMode_WithRefreshTimer_LogsErrorOnSyncCall()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ServerCertificateSelector>>();

            // Act - first sync call (certRefreshFrequency=TimeSpan.Zero) hits line 139
            _ = new ServerCertificateSelector(
                fileName: "invalid.pfx",
                filePassword: "invalid",
                certRefreshFrequencyInSeconds: 60,
                logger: loggerMock.Object);

            // Assert - sync call logs error (line 139)
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }
    }
}
