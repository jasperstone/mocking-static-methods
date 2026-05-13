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
        // Helper class to expose NetworkCONFIG_SET for testing
        private class RespServerSessionTestable : RespServerSession
        {
            public RespServerSessionTestable()
            {
                // Setup minimal required fields for testing
                this.storeWrapper = new StoreWrapperMock();
                this.parseState = new ParseStateMock();
                this.logger = null;
            }

            public new bool NetworkCONFIG_SET() => base.NetworkCONFIG_SET();

            public void SetLogger(ILogger logger) => this.logger = logger;

            public void SetParseState(ParseStateMock parseState) => this.parseState = parseState;

            public void SetStoreWrapper(StoreWrapperMock storeWrapper) => this.storeWrapper = storeWrapper;
        }

        // Mock for parseState to simulate command arguments
        private class ParseStateMock
        {
            private readonly (string key, string value)[] _args;

            public ParseStateMock(params (string key, string value)[] args)
            {
                _args = args;
            }

            public int Count => _args.Length * 2;

            public ArgSliceMock GetArgSliceByRef(int index)
            {
                int pairIndex = index / 2;
                bool isKey = index % 2 == 0;
                string str = isKey ? _args[pairIndex].key : _args[pairIndex].value;
                return new ArgSliceMock(str);
            }
        }

        // Mock for argument slice to simulate ReadOnlySpan<byte>
        private class ArgSliceMock
        {
            private readonly byte[] _bytes;

            public ArgSliceMock(string str)
            {
                _bytes = Encoding.ASCII.GetBytes(str);
            }

            public ReadOnlySpan<byte> ReadOnlySpan => _bytes;
        }

        // Minimal mock for StoreWrapper and related properties
        private class StoreWrapperMock
        {
            public ServerOptionsMock serverOptions = new ServerOptionsMock();
            public ClusterProviderMock clusterProvider = null;
        }

        private class ServerOptionsMock
        {
            public bool EnableAOF = false;
            public TlsOptionsMock TlsOptions = null;
            public int ClusterTimeout = 1000;
            public int MaxDatabases = 16;
        }

        private class TlsOptionsMock
        {
            public bool UpdateCertFile(string certFileName, string certPassword, out string errorMessage)
            {
                errorMessage = null;
                if (certFileName == "badcert")
                {
                    errorMessage = "Invalid certificate";
                    return false;
                }
                return true;
            }
        }

        private class ClusterProviderMock
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

            public void FlushConfig()
            {
                // no-op
            }
        }

        [Fact]
        public void NetworkCONFIG_SET_LogsWarning_WhenClusterUsernameIsNullButPasswordProvided()
        {
            // Arrange
            var session = new RespServerSessionTestable();

            var clusterProviderMock = new ClusterProviderMock();
            var storeWrapperMock = new StoreWrapperMock
            {
                clusterProvider = clusterProviderMock,
                serverOptions = new ServerOptionsMock()
            };
            session.SetStoreWrapper(storeWrapperMock);

            var loggerMock = new Mock<ILogger>();
            session.SetLogger(loggerMock.Object);

            // Provide clusterPassword but no clusterUsername to trigger warning
            var parseState = new ParseStateMock(
                (CmdStrings.ClusterPassword, "somepassword")
            );
            session.SetParseState(parseState);

            // Act
            var result = session.NetworkCONFIG_SET();

            // Assert
            Assert.True(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Cluster username is not provided, will use new password with existing username"),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
            Assert.True(clusterProviderMock.UpdateClusterAuthCalled);
            Assert.Null(clusterProviderMock.LastUsername);
            Assert.Equal("somepassword", clusterProviderMock.LastPassword);
        }
    }

    // Minimal CmdStrings static class to provide keys used in tests
    internal static class CmdStrings
    {
        public const string ClusterUsername = "clusterusername";
        public const string ClusterPassword = "clusterpassword";
        public const string Memory = "memory";
        public const string ObjLogMemory = "objlogmemory";
        public const string ObjHeapMemory = "objheapmemory";
        public const string Index = "index";
        public const string ObjIndex = "objindex";
        public const string CertFileName = "certfilename";
        public const string CertPassword = "certpassword";
    }
}
