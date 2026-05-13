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

        // To test this, we need to simulate parseState with keys and values for ClusterPassword but no ClusterUsername.
        // We also need to mock ILogger to verify LogWarning is called.

        // Since RespServerSession is internal and partial, and depends on many internals,
        // we will create a minimal subclass for testing that exposes NetworkCONFIG_SET as public.

        private class TestRespServerSession : RespServerSession
        {
            public TestRespServerSession(Mock<ILogger> loggerMock)
            {
                this.logger = loggerMock.Object;
                // Setup minimal required fields to avoid null refs
                this.storeWrapper = new StoreWrapperForTest();
                this.parseState = new ParseStateForTest();
            }

            public new unsafe bool NetworkCONFIG_SET()
            {
                return base.NetworkCONFIG_SET();
            }

            // Expose parseState and storeWrapper for test setup
            public ParseStateForTest parseState;
            public StoreWrapperForTest storeWrapper;
            public ILogger logger;
        }

        // Minimal mock classes to simulate dependencies
        private class ParseStateForTest
        {
            private (string key, string value)[] args;

            public int Count => args?.Length ?? 0;

            public ParseStateForTest()
            {
                args = new (string, string)[0];
            }

            public void SetArgs(params (string key, string value)[] keyValues)
            {
                args = keyValues;
            }

            public ArgSlice GetArgSliceByRef(int index)
            {
                var (key, value) = args[index];
                return new ArgSlice(key, value);
            }
        }

        private struct ArgSlice
        {
            private readonly string key;
            private readonly string value;

            public ArgSlice(string key, string value)
            {
                this.key = key;
                this.value = value;
            }

            public ReadOnlySpan<byte> ReadOnlySpan => Encoding.ASCII.GetBytes(key);

            public ReadOnlySpan<byte> Span => Encoding.ASCII.GetBytes(key);

            // For the value, we need a way to get the value span
            public ReadOnlySpan<byte> GetValueSpan() => Encoding.ASCII.GetBytes(value);

            // We will override ReadOnlySpan to return key or value depending on usage in code
            // But the code calls GetArgSliceByRef(c).ReadOnlySpan for key and GetArgSliceByRef(c+1).ReadOnlySpan for value
            // So we will simulate that by returning key or value accordingly in the test setup
        }

        private class StoreWrapperForTest
        {
            public ServerOptionsForTest serverOptions = new ServerOptionsForTest();
            public ClusterProviderForTest clusterProvider = new ClusterProviderForTest();
        }

        private class ServerOptionsForTest
        {
            public bool EnableAOF = false;
            public TlsOptionsForTest TlsOptions = null;
            public int ClusterTimeout = 1000;
            public int MaxDatabases = 16;
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

        [Fact]
        public void NetworkCONFIG_SET_LogsWarning_WhenClusterUsernameNullAndClusterPasswordNotNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var session = new TestRespServerSession(loggerMock);

            // Setup parseState with ClusterPassword only, no ClusterUsername
            // The code expects pairs of key-value, so we provide ClusterPassword key and a value
            session.parseState.SetArgs(
                (CmdStrings.ClusterPassword, "password123")
            );

            // The code expects even number of args, so we add a dummy pair for ClusterUsername with empty string to simulate null
            // But to trigger the warning, clusterUsername must be null, so we omit ClusterUsername key entirely
            // So we provide only one pair, which is invalid (odd count), so we add a dummy pair with unknown key to avoid error
            session.parseState.SetArgs(
                (CmdStrings.ClusterPassword, "password123"),
                ("dummyKey", "dummyValue")
            );

            // We need to override GetArgSliceByRef to return keys and values as ReadOnlySpan<byte>
            // We will do this by subclassing ParseStateForTest with proper implementation

            // Act
            var result = session.NetworkCONFIG_SET();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Cluster username is not provided")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.True(result);
        }
    }

    // We need to define CmdStrings constants used in the test
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
        public const string REWRITE = "rewrite";
        public const string SET = "set";
        public const string GET = "get";
        public const string SAVE = "save";
        public const string RESP_OK = "+OK\r\n";
        public const string RESP_EMPTYLIST = "*0\r\n";
        public const string GenericErrUnknownOptionConfigSet = "ERR unknown option '{0}' in config set";
    }
}
