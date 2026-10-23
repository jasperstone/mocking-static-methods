using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Garnet.common;
using Garnet.server;

namespace Garnet.server.tests
{
    public class ServerConfigTests
    {
        private class TestableStoreWrapper : StoreWrapper
        {
            public IClusterProvider TestClusterProvider { get; set; } = null!;
            public new IClusterProvider clusterProvider => TestClusterProvider;

            public TestableStoreWrapper() : base(
                "test-version", "RESP3", Array.Empty<IGarnetServer>(), 
                new Mock<CustomCommandManager>().Object,
                new GarnetServerOptions(),
                new Mock<SubscribeBroker>().Object())
            {
            }
        }

        [Fact]
        public void NetworkCONFIG_SET_LogsWarning_WhenClusterPasswordProvidedWithoutUsername()
        {
            // Arrange
            var storeWrapper = new TestableStoreWrapper();
            storeWrapper.TestClusterProvider = null;

            var mockLogger = new Mock<ILogger<RespServerSession>>();
            var session = new MockRespServerSession(storeWrapper, mockLogger.Object);

            session.SetParseState(new List<(ReadOnlyMemory<byte>, ReadOnlyMemory<byte>)>
            {
                (Encoding.ASCII.GetBytes("cluster-password"), Encoding.ASCII.GetBytes("testpass"))
            });

            // Act
            session.CallNetworkCONFIG_SET();

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Cluster username is not provided")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void NetworkCONFIG_SET_LogsNoWarning_WhenBothClusterUsernameAndPasswordProvided()
        {
            // Arrange
            var storeWrapper = new TestableStoreWrapper();
            var mockLogger = new Mock<ILogger<RespServerSession>>();
            var session = new MockRespServerSession(storeWrapper, mockLogger.Object);

            session.SetParseState(new List<(ReadOnlyMemory<byte>, ReadOnlyMemory<byte>)>
            {
                (Encoding.ASCII.GetBytes("cluster-username"), Encoding.ASCII.GetBytes("testuser")),
                (Encoding.ASCII.GetBytes("cluster-password"), Encoding.ASCII.GetBytes("testpass"))
            });

            // Act
            session.CallNetworkCONFIG_SET();

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Cluster username is not provided")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public void NetworkCONFIG_SET_LogsNoWarning_WhenNoClusterCredentialsProvided()
        {
            // Arrange
            var storeWrapper = new TestableStoreWrapper();
            var mockLogger = new Mock<ILogger<RespServerSession>>();
            var session = new MockRespServerSession(storeWrapper, mockLogger.Object);

            session.SetParseState(new List<(ReadOnlyMemory<byte>, ReadOnlyMemory<byte>)>());

            // Act
            session.CallNetworkCONFIG_SET();

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Cluster username is not provided")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
    }

    public class MockRespServerSession : RespServerSession
    {
        public MockRespServerSession(StoreWrapper storeWrapper, ILogger<RespServerSession> logger) : base(storeWrapper)
        {
            this.logger = logger;
        }

        public void SetParseState(List<(ReadOnlyMemory<byte> key, ReadOnlyMemory<byte> value)> args)
        {
            parseState.Clear();
            foreach (var (keyBytes, valueBytes) in args)
            {
                parseState.Add(new ArgSlice { buffer = keyBytes, start = 0, len = (int)keyBytes.Length });
                parseState.Add(new ArgSlice { buffer = valueBytes, start = 0, len = (int)valueBytes.Length });
            }
        }

        public bool CallNetworkCONFIG_SET()
        {
            return NetworkCONFIG_SET();
        }
    }
}
