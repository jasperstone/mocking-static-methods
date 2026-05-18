using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Garnet.common;

namespace Garnet.Tests
{
    public class FormatTests
    {
        private readonly Mock<ILogger> _loggerMock;

        public FormatTests()
        {
            _loggerMock = new Mock<ILogger>();
            _loggerMock.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()));
        }

        [Fact]
        public void TryCreateEndpoint_ValidIP_ReturnsEndpoint()
        {
            var result = Format.TryCreateEndpoint("127.0.0.1", 6379, logger: _loggerMock.Object);
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.IsType<IPEndPoint>(result[0]);
        }

        [Fact]
        public void TryCreateEndpoint_Localhost_ReturnsLoopbackEndpoints()
        {
            var result = Format.TryCreateEndpoint("localhost", 6379, logger: _loggerMock.Object);
            Assert.NotNull(result);
            Assert.HasLength(result, Socket.OSSupportsIPv6 ? 2 : 1);
        }

        [Fact]
        public void TryCreateEndpoint_EmptyString_ReturnsBindAnyEndpoints()
        {
            var result = Format.TryCreateEndpoint("", 6379, logger: _loggerMock.Object);
            Assert.NotNull(result);
            Assert.HasLength(result, Socket.OSSupportsIPv6 ? 2 : 1);
        }

        [Fact]
        public void TryCreateEndpoint_NullLogger_DoesNotThrow()
        {
            var result = Format.TryCreateEndpoint("localhost", 6379, logger: NullLogger.Instance);
            Assert.NotNull(result);
        }
    }
}
