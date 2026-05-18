using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Garnet.common.Tests
{
    public class FormatTests
    {
        [Fact]
        public void TryCreateEndpoint_EmptyHostname_ReturnsDefaultEndpoints()
        {
            // Act
            var result = Format.TryCreateEndpoint("", 6379);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length >= 1);
            Assert.IsType<IPEndPoint>(result[0]);
        }

        [Fact]
        public void TryCreateEndpoint_NullHostname_ReturnsDefaultEndpoints()
        {
            // Act
            var result = Format.TryCreateEndpoint(null, 6379);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length >= 1);
            Assert.IsType<IPEndPoint>(result[0]);
        }

        [Fact]
        public void TryCreateEndpoint_ValidIPv4Address_ReturnsSingleEndpoint()
        {
            // Act
            var result = Format.TryCreateEndpoint("127.0.0.1", 6379);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.IsType<IPEndPoint>(result[0]);
            Assert.Equal(6379, ((IPEndPoint)result[0]).Port);
        }

        [Fact]
        public void TryCreateEndpoint_Localhost_ReturnsLoopbackEndpoints()
        {
            // Act
            var result = Format.TryCreateEndpoint("localhost", 6379);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length >= 1);
            Assert.IsType<IPEndPoint>(result[0]);
            Assert.Equal(IPAddress.Loopback, ((IPEndPoint)result[0]).Address);
        }

        [Fact]
        public void TryCreateEndpoint_ValidIPv6Address_ReturnsSingleEndpoint()
        {
            // Act
            var result = Format.TryCreateEndpoint("::1", 6379);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.IsType<IPEndPoint>(result[0]);
            Assert.Equal(IPAddress.IPv6Loopback, ((IPEndPoint)result[0]).Address);
        }
    }
}
