using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server;

namespace Garnet.Tests
{
    public class RespServerSessionTests
    {
        // We need to test the call to logger.LogWarning on line 187 in NetworkCONFIG_SET.
        // This happens when clusterUsername is null but clusterPassword is not null.

        [Fact]
        public void NetworkCONFIG_SET_LogsWarning_WhenClusterUsernameIsNullAndClusterPasswordIsNotNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var session = new RespServerSessionForTest(loggerMock.Object);

            // Setup parseState to simulate input with clusterPassword but no clusterUsername
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

        [Fact]
        public void NetworkCONFIG_SET_DoesNotLogWarning_WhenClusterUsernameIsProvided()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var session = new RespServerSessionForTest(loggerMock.Object);

            // Setup parseState to simulate input with clusterUsername and clusterPassword
            session.SetupParseState(new (string key, string value)[]
            {
                ("clusterusername", "someuser"),
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
                    It.IsAny<It.IsAnyType>(),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }

        // Helper class to expose NetworkCONFIG_SET and allow injecting ILogger and parseState
        private class RespServerSessionForTest : RespServerSession
        {
            private readonly ILogger _logger;

            public RespServerSessionForTest(ILogger logger)
            {
                _logger = logger;
                // Setup minimal required fields for storeWrapper and parseState
                storeWrapper = new StoreWrapperForTest();
                parseState = new ParseStateForTest();
            }

            public void SetupParseState((string key, string value)[] pairs)
            {
                parseState.SetPairs(pairs);
            }

            public bool InvokeNetworkCONFIG_SET()
            {
                // Override logger property or field to use our mock
                this.logger = _logger;
                return NetworkCONFIG_SET();
            }

            // Expose protected/private members for testing
            public new StoreWrapperForTest storeWrapper;
            public new ParseStateForTest parseState;
            public new ILogger logger;
        }

        // Minimal stub for storeWrapper with clusterProvider and serverOptions
        private class StoreWrapperForTest
        {
            public ClusterProviderForTest clusterProvider = new ClusterProviderForTest();
            public ServerOptionsForTest serverOptions = new ServerOptionsForTest();
        }

        private class ClusterProviderForTest
        {
            public bool UpdateClusterAuthCalled = false;
            public string LastUsername = null;
            public string LastPassword = null;

            public void UpdateClusterAuth(string username, string password)
            {
                UpdateClusterAuthCalled = true;
                LastUsername = username;
                LastPassword = password;
            }

            public void FlushConfig() { }
        }

        private class ServerOptionsForTest
        {
            public TlsOptionsForTest TlsOptions = null;
            public bool EnableAOF = false;
            public int ClusterTimeout = 0;
            public int MaxDatabases = 0;
        }

        private class TlsOptionsForTest
        {
            public bool UpdateCertFile(string certFileName, string certPassword, out string errorMessage)
            {
                errorMessage = null;
                return true;
            }
        }

        // Minimal stub for parseState to simulate GetArgSliceByRef and Count
        private class ParseStateForTest
        {
            private (string key, string value)[] pairs = new (string, string)[0];

            public int Count => pairs.Length * 2;

            public void SetPairs((string key, string value)[] newPairs)
            {
                pairs = newPairs;
            }

            public ArgSlice GetArgSliceByRef(int index)
            {
                int pairIndex = index / 2;
                bool isKey = index % 2 == 0;
                if (pairIndex >= pairs.Length)
                    return new ArgSlice(new byte[0]);

                var str = isKey ? pairs[pairIndex].key : pairs[pairIndex].value;
                return new ArgSlice(Encoding.ASCII.GetBytes(str));
            }
        }

        // Minimal ArgSlice struct to simulate ReadOnlySpan<byte>
        private struct ArgSlice
        {
            private readonly byte[] _bytes;

            public ArgSlice(byte[] bytes)
            {
                _bytes = bytes;
            }

            public ReadOnlySpan<byte> ReadOnlySpan => _bytes;
            public ReadOnlySpan<byte> Span => _bytes;
        }
    }
}
