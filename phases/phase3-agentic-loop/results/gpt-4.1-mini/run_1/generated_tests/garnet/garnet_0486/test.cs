using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server;

namespace Garnet.Tests
{
    public class RespServerSessionTests
    {
        [Fact]
        public void NetworkCONFIG_SET_LogsWarning_WhenClusterUsernameIsNullAndClusterPasswordIsNotNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var session = new RespServerSessionTestWrapper(loggerMock.Object);

            // Setup parseState to simulate input:
            // keys: ClusterPassword only (no ClusterUsername)
            // values: some password string
            session.SetupParseState(new (string key, string value)[]
            {
                ("ClusterPassword", "somepassword")
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

        // Wrapper class to expose NetworkCONFIG_SET and allow injecting dependencies
        private class RespServerSessionTestWrapper : RespServerSession
        {
            private readonly MockParseState _parseState;

            public RespServerSessionTestWrapper(ILogger logger)
            {
                base.logger = logger;
                _parseState = new MockParseState();
                base.parseState = _parseState;
                base.storeWrapper = new StoreWrapperForTest();
            }

            public void SetupParseState((string key, string value)[] pairs)
            {
                _parseState.SetPairs(pairs);
            }

            public bool InvokeNetworkCONFIG_SET()
            {
                return base.NetworkCONFIG_SET();
            }
        }

        // Minimal mock parseState to simulate key-value pairs
        private class MockParseState : IParseState
        {
            private (string key, string value)[] _pairs;

            public int Count => _pairs?.Length * 2 ?? 0;

            public void SetPairs((string key, string value)[] pairs)
            {
                _pairs = pairs;
            }

            public ArgSlice GetArgSliceByRef(int index)
            {
                int pairIndex = index / 2;
                bool isKey = index % 2 == 0;
                string str = isKey ? _pairs[pairIndex].key : _pairs[pairIndex].value;
                return new ArgSlice(Encoding.ASCII.GetBytes(str));
            }
        }

        // Minimal ArgSlice struct to simulate the real one
        private readonly struct ArgSlice
        {
            private readonly byte[] _bytes;
            public ArgSlice(byte[] bytes) => _bytes = bytes;
            public ReadOnlySpan<byte> ReadOnlySpan => _bytes;
            public ReadOnlySpan<byte> Span => _bytes;
        }

        // Minimal StoreWrapper and clusterProvider to avoid null refs
        private class StoreWrapperForTest : IStoreWrapper
        {
            public IClusterProvider clusterProvider { get; } = new ClusterProviderForTest();
            public ServerOptions serverOptions { get; } = new ServerOptions();
        }

        private class ClusterProviderForTest : IClusterProvider
        {
            public void UpdateClusterAuth(string username, string password) { }
            public void FlushConfig() { }
        }

        private class ServerOptions
        {
            public TlsOptions TlsOptions { get; set; }
            public bool EnableAOF { get; set; }
            public int ClusterTimeout { get; set; }
            public int MaxDatabases { get; set; }
        }

        private class TlsOptions
        {
            public bool UpdateCertFile(string certFileName, string certPassword, out string errorMessage)
            {
                errorMessage = null;
                return true;
            }
        }

        // Interfaces to satisfy base class fields
        private interface IParseState
        {
            int Count { get; }
            ArgSlice GetArgSliceByRef(int index);
        }

        private interface IStoreWrapper
        {
            IClusterProvider clusterProvider { get; }
            ServerOptions serverOptions { get; }
        }

        private interface IClusterProvider
        {
            void UpdateClusterAuth(string username, string password);
            void FlushConfig();
        }
    }
}
