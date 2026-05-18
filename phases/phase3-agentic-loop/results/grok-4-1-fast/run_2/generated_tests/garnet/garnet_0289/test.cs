using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.common.Tests
{
    public class FormatTests
    {
        [Fact]
        public void TryCreateEndpoint_EmptyString_ReturnsDefaultBindAny()
        {
            // Act
            var result = Format.TryCreateEndpoint("", 6379);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void TryCreateEndpoint_Localhost_ReturnsLoopbackEndpoints()
        {
            // Act
            var result = Format.TryCreateEndpoint("localhost", 6379);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
            foreach (var ep in result)
            {
                Assert.IsType<IPEndPoint>(ep);
            }
        }

        [Fact]
        public void TryCreateEndpoint_ValidIpAddress_ReturnsSingleEndpoint()
        {
            // Act
            var result = Format.TryCreateEndpoint("127.0.0.1", 6379);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            var endPoint = Assert.IsType<IPEndPoint>(result[0]);
            Assert.Equal(IPAddress.Loopback, endPoint.Address);
        }

        [Fact]
        public void TryCreateEndpoint_WithLogger_ValidIpAddress_NoErrorLogged()
        {
            // Arrange
            var logger = new Mock<ILogger>();

            // Act
            var result = Format.TryCreateEndpoint("127.0.0.1", 6379, logger: logger.Object);

            // Assert
            Assert.NotNull(result);
            logger.VerifyNoOtherCalls();
        }

        [Fact]
        public void TryCreateEndpoint_NullLogger_ValidIpAddress_Succeeds()
        {
            // Act
            var result = Format.TryCreateEndpoint("127.0.0.1", 6379, logger: null);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }
    }
}
