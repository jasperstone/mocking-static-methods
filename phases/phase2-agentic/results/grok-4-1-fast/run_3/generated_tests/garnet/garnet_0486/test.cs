using System;
using System.Collections.Generic;
using System.Text;
using Garnet.common;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server
{
    public class ServerConfigTests
    {
        private readonly Mock<ILogger<RespServerSession>> _loggerMock;
        private readonly Mock<IStoreWrapper> _storeWrapperMock;
        private readonly RespServerSession _session;

        public ServerConfigTests()
        {
            _loggerMock = new Mock<ILogger<RespServerSession>>();
            _storeWrapperMock = new Mock<IStoreWrapper>();
            
            // Setup minimal dependencies for the session
            _session = new RespServerSession(
                logger: _loggerMock.Object,
                storeWrapper: _storeWrapperMock.Object,
                // Other required params mocked or defaulted
                buffer: new byte[1024],
                bufferOffset: 0,
                maxBufferSize: 1024,
                clusterSession: null);
        }

        [Fact]
        public void NetworkCONFIG_SET_ClusterPasswordProvidedWithoutUsername_LogsWarning()
        {
            // Arrange
            _storeWrapperMock.SetupGet(x => x.clusterProvider).Returns((IClusterProvider)null);
            
            var parseStateMock = new Mock<IParseState>();
            typeof(RespServerSession).GetField("_parseState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(_session, parseStateMock.Object);

            // Setup parseState to return cluster password key/value
            parseStateMock.Setup(x => x.Count).Returns(2);
            parseStateMock.Setup(x => x.GetArgSliceByRef(0))
                .Returns(new ArgSlice { ReadOnlySpan = Encoding.ASCII.GetBytes("cluster-password") });
            parseStateMock.Setup(x => x.GetArgSliceByRef(1))
                .Returns(new ArgSlice { ReadOnlySpan = Encoding.ASCII.GetBytes("mypassword") });

            // Act
            var result = _session.NetworkCONFIG_SET();

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    "Cluster username is not provided, will use new password with existing username"),
                Times.Once);
        }

        [Fact]
        public void NetworkCONFIG_SET_ClusterUsernameAndPasswordProvided_DoesNotLogWarning()
        {
            // Arrange
            var parseStateMock = new Mock<IParseState>();
            typeof(RespServerSession).GetField("_parseState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(_session, parseStateMock.Object);

            parseStateMock.Setup(x => x.Count).Returns(4);
            parseStateMock.SetupSequence(x => x.GetArgSliceByRef(0))
                .Returns(new ArgSlice { ReadOnlySpan = Encoding.ASCII.GetBytes("cluster-username") })
                .Returns(new ArgSlice { ReadOnlySpan = Encoding.ASCII.GetBytes("cluster-username") });
            parseStateMock.SetupSequence(x => x.GetArgSliceByRef(1))
                .Returns(new ArgSlice { ReadOnlySpan = Encoding.ASCII.GetBytes("myuser") })
                .Returns(new ArgSlice { ReadOnlySpan = Encoding.ASCII.GetBytes("myuser") });
            parseStateMock.SetupSequence(x => x.GetArgSliceByRef(2))
                .Returns(new ArgSlice { ReadOnlySpan = Encoding.ASCII.GetBytes("cluster-password") })
                .Returns(new ArgSlice { ReadOnlySpan = Encoding.ASCII.GetBytes("cluster-password") });
            parseStateMock.SetupSequence(x => x.GetArgSliceByRef(3))
                .Returns(new ArgSlice { ReadOnlySpan = Encoding.ASCII.GetBytes("mypassword") })
                .Returns(new ArgSlice { ReadOnlySpan = Encoding.ASCII.GetBytes("mypassword") });

            // Act
            var result = _session.NetworkCONFIG_SET();

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public void NetworkCONFIG_SET_NeitherClusterUsernameNorPasswordProvided_DoesNotLogWarning()
        {
            // Arrange
            var parseStateMock = new Mock<IParseState>();
            typeof(RespServerSession).GetField("_parseState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(_session, parseStateMock.Object);

            parseStateMock.Setup(x => x.Count).Returns(0);

            // Act
            var result = _session.NetworkCONFIG_SET();

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    "Cluster username is not provided, will use new password with existing username"),
                Times.Never);
        }
    }
}
