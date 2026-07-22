using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Garnet.common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Garnet.common.tests
{
    public class FormatTests
    {
        private readonly Mock<ILogger> _mockLogger = new();

        [Fact]
        public void TryCreateEndpoint_EmptyString_ReturnsDefaultBindAny()
        {
            // Act
            var result = Format.TryCreateEndpoint("", 8080, logger: NullLogger.Instance);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length >= 1);
            Assert.Equal(8080, ((IPEndPoint)result[0]).Port);
        }

        [Fact]
        public void TryCreateEndpoint_Localhost_ReturnsDefaultBindLoopback()
        {
            // Act
            var result = Format.TryCreateEndpoint("localhost", 8080, logger: NullLogger.Instance);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length >= 1);
            Assert.Equal(IPAddress.Loopback, ((IPEndPoint)result[0]).Address);
            Assert.Equal(8080, ((IPEndPoint)result[0]).Port);
        }

        [Fact]
        public void TryCreateEndpoint_ValidIpAddress_ReturnsSingleEndpoint()
        {
            // Act
            var result = Format.TryCreateEndpoint("127.0.0.1", 8080, logger: NullLogger.Instance);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(IPAddress.Loopback, ((IPEndPoint)result[0]).Address);
            Assert.Equal(8080, ((IPEndPoint)result[0]).Port);
        }

        [Fact]
        public void TryCreateEndpoint_HostnameMismatchMachineName_LogsErrorAndReturnsNull()
        {
            // Arrange
            var testHostname = "totallynotmyhostname";

            // Act
            var result = Format.TryCreateEndpoint(testHostname, 8080, tryConnect: false, logger: _mockLogger.Object);

            // Assert
            Assert.Null(result);
            _mockLogger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), 
                It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task TryCreateEndpointAsync_HostnameMismatchMachineName_LogsErrorAndReturnsNull()
        {
            // Arrange
            var testHostname = "totallynotmyhostname";

            // Act
            var result = await Format.TryCreateEndpointAsync(testHostname, 8080, tryConnect: false, logger: _mockLogger.Object);

            // Assert
            Assert.Null(result);
            _mockLogger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), 
                It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public void TryParseAddressList_EmptyAddressList_ReturnsDefaultEndpoints()
        {
            // Act
            var success = Format.TryParseAddressList("", 8080, out var endpoints, out var error, logger: NullLogger.Instance);

            // Assert
            Assert.True(success);
            Assert.NotNull(endpoints);
            Assert.Null(error);
        }

        [Fact]
        public void TryParseAddressList_InvalidAddress_ReturnsFalseWithError()
        {
            // Act
            var success = Format.TryParseAddressList("invalidhost123", 8080, out var endpoints, out var error, logger: NullLogger.Instance);

            // Assert
            Assert.False(success);
            Assert.Null(endpoints);
            Assert.Equal("invalidhost123", error);
        }
    }
}
