using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server.Tests
{
    public class ServerConfigTests
    {
        [Fact]
        public void NetworkConfigSet_LogsWarning_WhenClusterUsernameIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var parseStateMock = new Mock<ParseState>();

            // Set up the scenario where clusterUsername is null and clusterPassword is not null
            parseStateMock.Setup(p => p.Count).Returns(1);
            parseStateMock.Setup(p => p.GetArgSliceByRef(0)).Returns(new ArgSlice("cluster-password", "password123"));
            parseStateMock.Setup(p => p.GetArgSliceByRef(1)).Returns(new ArgSlice("cluster-username", ""));

            var session = new RespServerSession(loggerMock.Object, storeWrapperMock.Object)
            {
                ParseState = parseStateMock.Object
            };

            // Act
            session.NetworkCONFIG_SET();

            // Assert
            loggerMock.Verify(
                l => l.LogWarning(It.Is<string>(s => s == "Cluster username is not provided, will use new password with existing username")),
                Times.Once);
        }
    }

    // Minimal mock for ParseState
    public class ParseState
    {
        public int Count { get; set; }

        public ArgSlice GetArgSliceByRef(int index)
        {
            // Return a mock ArgSlice based on index
            return new ArgSlice();
        }
    }

    // Minimal mock for ArgSlice
    public class ArgSlice
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public ArgSlice(string key = "", string value = "")
        {
            Key = key;
            Value = value;
        }
    }
}
