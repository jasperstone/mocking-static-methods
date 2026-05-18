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
        public void NetworkCONFIG_SET_ShouldLogWarning_WhenClusterUsernameIsNull()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockServerOptions = new Mock<ServerOptions>();
            var mockClusterProvider = new Mock<IClusterProvider>();
            var mockClusterSession = new Mock<ClusterSession>();
            var mockParseState = new Mock<IParseState>();

            // Setup parseState to simulate input args
            var args = new[]
            {
                CmdStrings.ClusterUsername, "testuser"
            };
            mockParseState.Setup(p => p.Count).Returns(args.Length);
            mockParseState.Setup(p => p.GetArgSliceByRef(It.IsAny<int>()))
                .Returns<int>(i => new ArgSlice { ReadOnlySpan = System.Text.Encoding.ASCII.GetBytes(args[i]) });

            // Setup storeWrapper
            mockStoreWrapper.Setup(s => s.clusterProvider).Returns(mockClusterProvider.Object);
            mockStoreWrapper.Setup(s => s.serverOptions).Returns(mockServerOptions.Object);
            mockServerOptions.Setup(s => s.TlsOptions).Returns((TlsOptions)null);
            // Setup clusterSession
            mockClusterSession.Setup(c => c.ReadWriteSession).Returns(true);
            // Setup logger
            var logger = mockLogger.Object;

            // Create instance of RespServerSession with necessary dependencies
            var respSession = new RespServerSession
            {
                logger = logger,
                storeWrapper = mockStoreWrapper.Object,
                clusterSession = mockClusterSession.Object,
                parseState = mockParseState.Object
            };

            // Act
            var result = respSession.NetworkCONFIG_SET();

            // Assert
            mockLogger.Verify(
                x => x.LogWarning("Cluster username is not provided, will use new password with existing username"),
                Times.Once);
        }
    }
}
