using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography.X509Certificates;
using Garnet.server.TLS;

public class ServerCertificateSelectorTests
{
    [Fact]
    public void GetServerCertificate_LogsError_WhenCertificateFetchFails()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var certificateUtilsWrapperMock = new Mock<ICertificateUtilsWrapper>();
        certificateUtilsWrapperMock.Setup(x => x.GetMachineCertificateBySubjectName(It.IsAny<string>())).Throws<ArgumentException>();

        var selector = new ServerCertificateSelector("invalidSubjectName", 0, loggerMock.Object, certificateUtilsWrapperMock.Object);

        // Act
        selector.GetServerCertificate(null);

        // Assert
        loggerMock.Verify(
            x => x.LogError(
                It.IsAny<Exception>(),
                It.IsAny<string>(),
                It.IsAny<object[]>()
            ),
            Times.Once
        );
    }

    [Fact]
    public void GetServerCertificate_LogsError_WhenCertificateFetchFailsWithFile()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var certificateUtilsWrapperMock = new Mock<ICertificateUtilsWrapper>();
        certificateUtilsWrapperMock.Setup(x => x.GetMachineCertificateByFile(It.IsAny<string>(), It.IsAny<string>())).Throws<ArgumentException>();

        var selector = new ServerCertificateSelector("invalidFileName", "invalidPassword", 0, loggerMock.Object, certificateUtilsWrapperMock.Object);

        // Act
        selector.GetServerCertificate(null);

        // Assert
        loggerMock.Verify(
            x => x.LogError(
                It.IsAny<Exception>(),
                It.IsAny<string>(),
                It.IsAny<object[]>()
            ),
            Times.Once
        );
    }
}
