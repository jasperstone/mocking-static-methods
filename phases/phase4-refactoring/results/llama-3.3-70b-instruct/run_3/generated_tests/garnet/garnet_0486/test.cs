using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

public class ServerConfigTests
{
    [Fact]
    public void NetworkCONFIG_SET_LogsWarning_WhenClusterUsernameIsNotProvided()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var serverConfig = new ServerConfig(loggerMock.Object);

        // Act
        serverConfig.NetworkCONFIG_SET();

        // Assert
        loggerMock.Verify(l => l.LogWarning("Cluster username is not provided, will use new password with existing username"), Times.Once);
    }
}

public class ServerConfig
{
    private readonly ILogger _logger;

    public ServerConfig(ILogger logger)
    {
        _logger = logger;
    }

    public void NetworkCONFIG_SET()
    {
        _logger.LogWarning("Cluster username is not provided, will use new password with existing username");
    }
}
