using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server;

namespace Garnet.Tests
{
    public class RespServerSessionTests
    {
        // We will test the NetworkCONFIG_SET method focusing on the LogWarning call on line 187.
        // This happens when clusterUsername is null but clusterPassword is not null.

        [Fact]
        public void NetworkCONFIG_SET_LogsWarning_WhenClusterUsernameIsNullAndClusterPasswordIsNotNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var session = new RespServerSessionForTest(loggerMock.Object);

            // Setup parseState to simulate input with clusterPassword but no clusterUsername
            // parseState.Count must be even and > 0
            // We simulate keys and values as byte arrays for clusterPassword only
            session.SetupParseState(new (string key, string value)[]
            {
                ("clusterpassword", "somepassword")
            });

            // Act
            var result = session.InvokeNetworkCONFIG_SET();

            // Assert
            Assert.True(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Cluster username is not provided")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Helper subclass to expose and control parseState and logger for testing
        private class RespServerSessionForTest : RespServerSession
        {
            private readonly Mock<ILogger> _loggerMock;
            private ParseStateMock _parseStateMock;

            public RespServerSessionForTest(ILogger logger)
            {
                _loggerMock = Mock.Get(logger);
                this.logger = logger;
                _parseStateMock = new ParseStateMock();
                this.parseState = _parseStateMock;
                // Setup minimal storeWrapper and clusterProvider to avoid null refs
                this.storeWrapper = new StoreWrapperForTest();
            }

            public void SetupParseState((string key, string value)[] pairs)
            {
                _parseStateMock.SetPairs(pairs);
            }

            public bool InvokeNetworkCONFIG_SET()
            {
                return NetworkCONFIG_SET();
            }
        }

        // Mock for parseState to simulate GetArgSliceByRef and Count
        private class ParseStateMock : IParseState
        {
            private (string key, string value)[] _pairs = new (string, string)[0];

            public int Count => _pairs.Length * 2;

            public void SetPairs((string key, string value)[] pairs)
            {
                _pairs = pairs;
            }

            public ArgSlice GetArgSliceByRef(int index)
            {
                // index even: key, odd: value
                int pairIndex = index / 2;
                bool isKey = index % 2 == 0;
                string str = isKey ? _pairs[pairIndex].key : _pairs[pairIndex].value;
                return new ArgSlice(Encoding.ASCII.GetBytes(str));
            }
        }

        // Minimal interface and struct to simulate parseState and ArgSlice
        private interface IParseState
        {
            int Count { get; }
            ArgSlice GetArgSliceByRef(int index);
        }

        private struct ArgSlice
        {
            private readonly byte[] _bytes;
            public ArgSlice(byte[] bytes) => _bytes = bytes;
            public ReadOnlySpan<byte> ReadOnlySpan => _bytes;
        }

        // Minimal StoreWrapper and clusterProvider mocks to avoid null refs
        private class StoreWrapperForTest
        {
            public ServerOptions serverOptions = new ServerOptions();
            public ClusterProvider clusterProvider = null;

            public void UpdateClusterAuth(string username, string password) { }
        }

        private class ServerOptions
        {
            public TlsOptions TlsOptions = null;
            public bool EnableAOF = false;
            public int ClusterTimeout = 0;
            public int MaxDatabases = 0;
        }

        private class ClusterProvider
        {
            public void UpdateClusterAuth(string username, string password) { }
            public void FlushConfig() { }
        }
    }
}
