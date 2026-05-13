using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server.Tests
{
    public class RespServerSessionTests
    {
        [Fact]
        public void NetworkConfigSet_LogsWarning_WhenClusterUsernameIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var session = new RespServerSession(loggerMock.Object, storeWrapperMock.Object);

            // Simulate the scenario where clusterUsername is null
            session.ParseState = new ParseState(new[] { "clusterPassword", "passwordValue" });

            // Act
            session.NetworkCONFIG_SET();

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Cluster username is not provided, will use new password with existing username")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Mock classes to support the test
    public class ParseState
    {
        public ParseState(string[] args)
        {
            Args = args;
        }

        public string[] Args { get; }

        public int Count => Args.Length;

        public ReadOnlySpan<byte> GetArgSliceByRef(int index)
        {
            return Encoding.ASCII.GetBytes(Args[index]);
        }
    }

    public class StoreWrapper
    {
        public ServerOptions serverOptions { get; set; }
        public ClusterProvider clusterProvider { get; set; }
    }

    public class ServerOptions
    {
        public TlsOptions TlsOptions { get; set; }
    }

    public class TlsOptions
    {
        public bool UpdateCertFile(string certFileName, string certPassword, out string certErrorMessage)
        {
            certErrorMessage = null;
            return true;
        }
    }

    public class ClusterProvider
    {
        public void UpdateClusterAuth(string clusterUsername, string clusterPassword) { }
    }

    public class RespServerSession
    {
        public ParseState ParseState { get; set; }
        private readonly ILogger _logger;
        private readonly StoreWrapper _storeWrapper;

        public RespServerSession(ILogger logger, StoreWrapper storeWrapper)
        {
            _logger = logger;
            _storeWrapper = storeWrapper;
        }

        public bool NetworkCONFIG_SET()
        {
            if (ParseState.Count == 0 || ParseState.Count % 2 != 0)
            {
                return false; // Simulate AbortWithWrongNumberOfArguments
            }

            string certFileName = null;
            string certPassword = null;
            string clusterUsername = null;
            string clusterPassword = null;

            for (var c = 0; c < ParseState.Count; c += 2)
            {
                var key = ParseState.GetArgSliceByRef(c).ReadOnlySpan;
                var value = ParseState.GetArgSliceByRef(c + 1).ReadOnlySpan;

                if (Encoding.ASCII.GetString(key).Equals("clusterUsername", StringComparison.OrdinalIgnoreCase))
                    clusterUsername = Encoding.ASCII.GetString(value);
                else if (Encoding.ASCII.GetString(key).Equals("clusterPassword", StringComparison.OrdinalIgnoreCase))
                    clusterPassword = Encoding.ASCII.GetString(value);
            }

            if (clusterUsername == null)
                _logger?.LogWarning("Cluster username is not provided, will use new password with existing username");

            return true;
        }
    }
}
