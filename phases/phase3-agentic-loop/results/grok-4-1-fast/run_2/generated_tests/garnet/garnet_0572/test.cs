using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;
using Garnet.server.TLS;

namespace Garnet.server.TLS.Tests;

public class ServerCertificateSelectorTests
{
    [Fact]
    public void Constructor_FileMode_ZeroRefreshFrequency_LogsErrorOnCertificateLoadFailure()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ServerCertificateSelector>>();
        mockLogger.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(true);

        // Mock CertificateUtils to throw exception
        var mockCert = new Mock<X509Certificate2>();
        // Act & Assert
        Assert.ThrowsAny<Exception>(() => new ServerCertificateSelector("invalid.pfx", "wrongpass", 0, mockLogger.Object));

        // Verify the specific LogError call (line 139 path - file mode, zero refresh)
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to fetch certificate using the provided filename and password. Make sure you specify a correct CertFileName and CertPassword.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_SubjectNameMode_ZeroRefreshFrequency_LogsErrorOnCertificateLoadFailure()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ServerCertificateSelector>>();
        mockLogger.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(true);

        // Act & Assert
        Assert.ThrowsAny<Exception>(() => new ServerCertificateSelector("NonExistentSubject", 0, mockLogger.Object));

        // Verify the specific LogError call (line 139 path - but subject name mode also hits this branch when certRefreshFrequency == 0)
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to fetch certificate using the provided filename and password. Make sure you specify a correct CertFileName and CertPassword.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
