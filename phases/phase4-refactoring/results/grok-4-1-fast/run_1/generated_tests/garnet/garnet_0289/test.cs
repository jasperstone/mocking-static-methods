using System;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Garnet.common.tests;

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
        Assert.Contains(result, ep => ep is IPEndPoint ipEp && ipEp.Address.Equals(IPAddress.Any));
    }

    [Fact]
    public void TryCreateEndpoint_NullString_ReturnsDefaultBindAny()
    {
        // Act
        var result = Format.TryCreateEndpoint(null, 6379);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
        Assert.Contains(result, ep => ep is IPEndPoint ipEp && ipEp.Address.Equals(IPAddress.Any));
    }

    [Fact]
    public void TryCreateEndpoint_ValidIpAddress_ReturnsSingleEndpoint()
    {
        // Act
        var result = Format.TryCreateEndpoint("127.0.0.1", 6379);

        // Assert
        Assert.Single(result);
        var endpoint = Assert.IsType<IPEndPoint>(result[0]);
        Assert.Equal(IPAddress.Parse("127.0.0.1"), endpoint.Address);
        Assert.Equal(6379, endpoint.Port);
    }

    [Fact]
    public void TryCreateEndpoint_Localhost_ReturnsLoopbackEndpoints()
    {
        // Act
        var result = Format.TryCreateEndpoint("localhost", 6379);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
        Assert.Contains(result, ep => ep is IPEndPoint ipEp && ipEp.Address.Equals(IPAddress.Loopback));
    }

    [Fact]
    public void TryCreateEndpoint_UnresolvableHostname_ReturnsNull()
    {
        // Act
        var result = Format.TryCreateEndpoint("invalidhost123", 6379);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void TryParseAddressList_EmptyList_ProtectedMode_ReturnsLoopback()
    {
        // Act
        var success = Format.TryParseAddressList("", 6379, out var endpoints, out var error, protectedMode: true);

        // Assert
        Assert.True(success);
        Assert.NotNull(endpoints);
        Assert.Contains(endpoints, ep => ep is IPEndPoint ipEp && ipEp.Address.Equals(IPAddress.Loopback));
        Assert.Null(error);
    }

    [Fact]
    public void TryParseAddressList_InvalidAddress_ReturnsFalseWithError()
    {
        // Act
        var success = Format.TryParseAddressList("invalidhost", 6379, out var endpoints, out var error);

        // Assert
        Assert.False(success);
        Assert.Null(endpoints);
        Assert.Equal("invalidhost", error);
    }
}
