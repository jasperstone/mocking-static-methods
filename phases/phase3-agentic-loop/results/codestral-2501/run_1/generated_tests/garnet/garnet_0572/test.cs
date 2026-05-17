using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography.X509Certificates;
using Garnet.server.TLS;

public class ServerCertificateSelectorTests
{
    [Fact]
    public void LogError_WhenCertificateFetchFails_WithFileAndPassword()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var selector = new ServerCertificateSelector("invalidFile", "invalidPassword", 0, loggerMock.Object);

        // Act
        selector.GetSslServerCertificate();

        // Assert
        loggerMock.Verify(
            logger => logger.LogError(
                It.IsAny<EventId>(),
                It.IsAny<Exception>(),
                It.Is<string>(s => s.Contains("Unable to fetch certificate using the provided filename and password")),
                It.IsAny<object[]>()),
            Times.Once);
    }

    [Fact]
    public void LogError_WhenCertificateFetchFails_WithSubjectName()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var selector = new ServerCertificateSelector("invalidSubjectName", 0, loggerMock.Object);

        // Act
        selector.GetSslServerCertificate();

        // Assert
        loggerMock.Verify(
            logger => logger.LogError(
                It.IsAny<EventId>(),
                It.IsAny<Exception>(),
                It.Is<string>(s => s.Contains("Unable to fetch certificate using the provided filename and password")),
                It.IsAny<object[]>()),
            Times.Once);
    }
}
