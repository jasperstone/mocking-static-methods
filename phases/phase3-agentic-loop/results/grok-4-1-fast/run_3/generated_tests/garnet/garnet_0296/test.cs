using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Garnet.common
{
    public class FormatTests
    {
        private readonly Mock<ILogger> _loggerMock;

        public FormatTests()
        {
            _loggerMock = new Mock<ILogger>();
            _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        }

        [Fact]
        public void TryParseAddressList_EmptyAddressList_UsesDefaultBindAny()
        {
            // Act
            var result = Format.TryParseAddressList("", 6379, out var endpoints, out var error);

            // Assert
            Assert.True(result);
            Assert.Null(error);
            Assert.NotNull(endpoints);
            Assert.Contains(endpoints, e => e is IPEndPoint ep && ep.Address.Equals(IPAddress.Any));
        }

        [Fact]
        public void TryParseAddressList_ValidIP_UsesParsedIP()
        {
            // Act
            var result = Format.TryParseAddressList("127.0.0.1", 6379, out var endpoints, out var error);

            // Assert
            Assert.True(result);
            Assert.Null(error);
            Assert.Single(endpoints);
            Assert.IsType<IPEndPoint>(endpoints[0]);
            var ep = (IPEndPoint)endpoints[0];
            Assert.Equal(IPAddress.Parse("127.0.0.1"), ep.Address);
            Assert.Equal(6379, ep.Port);
        }

        [Fact]
        public void TryParseAddressList_Localhost_UsesLoopback()
        {
            // Act
            var result = Format.TryParseAddressList("localhost", 6379, out var endpoints, out var error);

            // Assert
            Assert.True(result);
            Assert.Null(error);
            Assert.NotNull(endpoints);
            Assert.Contains(endpoints, e => e is IPEndPoint ep && ep.Address.Equals(IPAddress.Loopback));
        }

        [Fact]
        public void TryCreateEndpoint_NullOrEmpty_UsesDefaultBindAny()
        {
            // Act
            var result1 = Format.TryCreateEndpoint(null, 6379);
            var result2 = Format.TryCreateEndpoint("", 6379);

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.Contains(result1, e => e is IPEndPoint ep && ep.Address.Equals(IPAddress.Any));
        }

        [Fact]
        public void TryCreateEndpoint_ValidIP_ReturnsEndpoint()
        {
            // Act
            var result = Format.TryCreateEndpoint("127.0.0.1", 6379);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            var ep = Assert.IsType<IPEndPoint>(result[0]);
            Assert.Equal(IPAddress.Parse("127.0.0.1"), ep.Address);
            Assert.Equal(6379, ep.Port);
        }

        [Fact]
        public void TryCreateEndpoint_Localhost_ReturnsLoopback()
        {
            // Act
            var result = Format.TryCreateEndpoint("localhost", 6379);

            // Assert
            Assert.NotNull(result);
            Assert.Contains(result, e => e is IPEndPoint ep && ep.Address.Equals(IPAddress.Loopback));
        }
    }
}
