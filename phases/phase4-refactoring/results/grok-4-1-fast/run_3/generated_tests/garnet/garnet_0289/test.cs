using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.common;

namespace Garnet.common.tests
{
    public class FormatTests
    {
        [Fact]
        public void TryCreateEndpoint_ValidIPv4Address_ReturnsSingleEndpoint()
        {
            // Arrange
            string ipAddress = "127.0.0.1";
            int port = 6379;

            // Act
            EndPoint[] result = Format.TryCreateEndpoint(ipAddress, port);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.IsType<IPEndPoint>(result[0]);
            var endpoint = (IPEndPoint)result[0];
            Assert.Equal(IPAddress.Parse(ipAddress), endpoint.Address);
            Assert.Equal(port, endpoint.Port);
        }

        [Fact]
        public void TryCreateEndpoint_Localhost_ReturnsLoopbackEndpoints()
        {
            // Act
            EndPoint[] result = Format.TryCreateEndpoint("localhost", 6379);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length >= 1);
            Assert.Contains(result, ep => ep is IPEndPoint ipe && 
                (ipe.Address.Equals(IPAddress.Loopback) || ipe.Address.Equals(IPAddress.IPv6Loopback)));
        }

        [Fact]
        public void TryCreateEndpoint_EmptyString_ReturnsBindAnyEndpoints()
        {
            // Act
            EndPoint[] result = Format.TryCreateEndpoint("", 6379);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length >= 1);
            Assert.Contains(result, ep => ep is IPEndPoint ipe && 
                (ipe.Address.Equals(IPAddress.Any) || ipe.Address.Equals(IPAddress.IPv6Any)));
        }

        [Fact]
        public void TryParseAddressList_SingleValidAddress_ReturnsEndpoint()
        {
            // Arrange
            string addressList = "127.0.0.1";
            int port = 6379;

            // Act
            bool success = Format.TryParseAddressList(addressList, port, out EndPoint[] endpoints, out string error);

            // Assert
            Assert.True(success);
            Assert.Null(error);
            Assert.NotNull(endpoints);
            Assert.Single(endpoints);
        }

        [Fact]
        public void TryParseAddressList_EmptyList_ProtectedMode_ReturnsLoopback()
        {
            // Arrange
            int port = 6379;

            // Act
            bool success = Format.TryParseAddressList("", port, out EndPoint[] endpoints, out string error, protectedMode: true);

            // Assert
            Assert.True(success);
            Assert.Null(error);
            Assert.NotNull(endpoints);
            Assert.Contains(endpoints, ep => ep is IPEndPoint ipe && ipe.Address.Equals(IPAddress.Loopback));
        }
    }
}
