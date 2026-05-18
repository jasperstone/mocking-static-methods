using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.server;

public class ServerConfigTests
{
    [Fact]
    public void LogWarning_WhenClusterUsernameIsNotProvided()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var storeWrapperMock = new Mock<StoreWrapper>();
        storeWrapperMock.Setup(sw => sw.clusterProvider).Returns((IClusterProvider)null);

        var serverConfig = new ServerConfig();
        var parseState = new ParseState();
        parseState.AddArg("clusterUsername", "");
        parseState.AddArg("clusterPassword", "password");

        var session = new RespServerSession(loggerMock.Object, storeWrapperMock.Object, parseState);

        // Act
        session.NetworkCONFIG_SET();

        // Assert
        loggerMock.Verify(
            logger => logger.LogWarning("Cluster username is not provided, will use new password with existing username"),
            Times.Once);
    }
}
