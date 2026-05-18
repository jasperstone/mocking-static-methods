using System;
using System.Linq;
using System.Net;
using Xunit;
using Garnet.common;

namespace Garnet.Tests
{
    public class FormatTests
    {
        [Fact]
        public void TryParseAddressList_EmptyAddressList_ReturnsDefaultEndpoints()
        {
            // Arrange
            string emptyAddressList = "";
            int port = 8080;
            EndPoint[] endpoints;
            string error;

            // Act
            var result = Format.TryParseAddressList(emptyAddressList, port, out endpoints, out error);

            // Assert
            Assert.True(result);
            Assert.NotNull(endpoints);
            Assert.True(endpoints.Length >= 1);
            Assert.Equal(port, ((IPEndPoint)endpoints[0]).Port);
        }

        [Fact]
        public void TryParseAddressList_ValidIP_ReturnsEndpoint()
        {
            // Arrange
            string addressList = "127.0.0.1";
            int port = 8080;
            EndPoint[] endpoints;
            string error;

            // Act
            var result = Format.TryParseAddressList(addressList, port, out endpoints, out error);

            // Assert
            Assert.True(result);
            Assert.NotNull(endpoints);
            Assert.Single(endpoints);
            Assert.Equal(IPAddress.Loopback, ((IPEndPoint)endpoints[0]).Address);
        }

        [Fact]
        public void TryParseAddressList_Localhost_ReturnsLoopbackEndpoints()
        {
            // Arrange
            string addressList = "localhost";
            int port = 8080;
            EndPoint[] endpoints;
            string error;

            // Act
            var result = Format.TryParseAddressList(addressList, port, out endpoints, out error);

            // Assert
            Assert.True(result);
            Assert.NotNull(endpoints);
            Assert.True(endpoints.Any(e => ((IPEndPoint)e).Address.Equals(IPAddress.Loopback)));
        }

        [Fact]
        public void TryParseAddressList_InvalidHostname_ReturnsFalseWithError()
        {
            // Arrange
            string invalidAddressList = "invalidhost123";
            int port = 8080;
            EndPoint[] endpoints;
            string error;

            // Act
            var result = Format.TryParseAddressList(invalidAddressList, port, out endpoints, out error);

            // Assert
            Assert.False(result);
            Assert.Equal("invalidhost123", error);
            Assert.Null(endpoints);
        }

        [Fact]
        public void TryParseAddressList_MultipleAddresses_FailsOnFirstInvalid()
        {
            // Arrange
            string addressList = "127.0.0.1, invalidhost";
            int port = 8080;
            EndPoint[] endpoints;
            string error;

            // Act
            var result = Format.TryParseAddressList(addressList, port, out endpoints, out error);

            // Assert - fails on first invalid
            Assert.False(result);
            Assert.Equal("invalidhost", error);
            Assert.Null(endpoints);
        }

        [Fact]
        public void TryParseAddressList_ProtectedModeEmpty_ReturnsLoopback()
        {
            // Arrange
            string emptyAddressList = " ";
            int port = 8080;
            EndPoint[] endpoints;
            string error;

            // Act
            var result = Format.TryParseAddressList(emptyAddressList, port, out endpoints, out error, protectedMode: true);

            // Assert
            Assert.True(result);
            Assert.NotNull(endpoints);
            Assert.True(endpoints.Any(e => ((IPEndPoint)e).Address.Equals(IPAddress.Loopback)));
        }
    }
}
