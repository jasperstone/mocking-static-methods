using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Garnet.common.Tests
{
    public class FormatTests
    {
        [Fact]
        public void TryCreateEndpoint_ValidIpAddress_ReturnsSingleEndpoint()
        {
            // Arrange
            string validIp = "127.0.0.1";
            int port = 6379;

            // Act
            var result = Format.TryCreateEndpoint(validIp, port);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.IsType<IPEndPoint>(result[0]);
            var endpoint = (IPEndPoint)result[0];
            Assert.Equal(IPAddress.Loopback, endpoint.Address);
            Assert.Equal(port, endpoint.Port);
        }

        [Fact]
        public void TryCreateEndpoint_Localhost_ReturnsLoopbackEndpoints()
        {
            // Arrange
            int port = 6379;

            // Act
            var result = Format.TryCreateEndpoint("localhost", port);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length >= 1);
            Assert.All(result, ep => Assert.IsType<IPEndPoint>(ep));
        }

        [Fact]
        public void TryCreateEndpoint_EmptyString_ReturnsBindAnyEndpoints()
        {
            // Act
            var result = Format.TryCreateEndpoint("", 6379);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length >= 1);
            Assert.All(result, ep => Assert.IsType<IPEndPoint>(ep));
        }

        [Fact]
        public void TryCreateEndpoint_NullString_ReturnsBindAnyEndpoints()
        {
            // Act
            var result = Format.TryCreateEndpoint(null, 6379);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length >= 1);
            Assert.All(result, ep => Assert.IsType<IPEndPoint>(ep));
        }

        [Fact]
        public void TryCreateEndpoint_DashPrefix_StripsDash()
        {
            // Act
            var result = Format.TryCreateEndpoint("-127.0.0.1", 6379);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            var endpoint = (IPEndPoint)result[0];
            Assert.Equal(IPAddress.Loopback, endpoint.Address);
        }
    }
}
