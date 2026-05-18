using System;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Garnet.server;

namespace Garnet.Tests
{
    public class ServerConfigTests
    {
        [Fact]
        public void NetworkConfigSet_ShouldLogWarning_WhenClusterUsernameIsNull()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var serverOptionsMock = new Mock<ServerOptions>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var serverSessionMock = new Mock<ServerSessionBase>();
            var respServerSession = new RespServerSession
            {
                logger = mockLogger.Object,
                storeWrapper = storeWrapperMock.Object,
                clusterSession = null,
                parseState = new ParseState(),
                dcurr = new byte[1024],
                dend = new byte[1024]
            };

            // Setup parseState with key-value pairs, including ClusterUsername with null value
            respServerSession.parseState.AddArg(CmdStrings.ClusterUsername, null);
            respServerSession.parseState.AddArg(CmdStrings.ClusterPassword, "password");
            // Act
            var result = respServerSession.NetworkCONFIG_SET();

            // Assert
            mockLogger.Verify(
                logger => logger.LogWarning("Cluster username is not provided, will use new password with existing username"),
                Times.Once);
        }
    }
}
