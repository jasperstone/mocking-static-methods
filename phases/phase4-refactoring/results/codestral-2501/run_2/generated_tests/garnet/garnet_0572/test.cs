using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using Garnet.server.TLS;

public class TestableServerCertificateSelector : ServerCertificateSelector
{
    public TestableServerCertificateSelector(string fileName, string filePassword, int certRefreshFrequencyInSeconds = 0, ILogger logger = null)
        : base(fileName, filePassword, certRefreshFrequencyInSeconds, logger)
    {
    }

    public new void GetServerCertificate(object _)
    {
        base.GetServerCertificate(_);
    }
}

public class ServerCertificateSelectorTests
{
    [Fact]
    public void LogError_Called_When_Certificate_Fetch_Fails()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var certificateSelector = new TestableServerCertificateSelector("invalidFileName", "invalidPassword", 0, loggerMock.Object);

        // Act
        certificateSelector.GetServerCertificate(null);

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
