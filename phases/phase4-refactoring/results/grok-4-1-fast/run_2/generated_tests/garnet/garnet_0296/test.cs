using System;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.common.Tests;

public class FormatTests
{
    private const int Port = 8080;

    [Fact]
    public void TryCreateEndpoint_WhenHostnameDoesNotMatchMachineHostname_LogsError()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()))
            .Callback<LogLevel, EventId, object, Exception, Func<object, Exception, string>>((level, id, state, ex, formatter) =>
            {
                var message = formatter(state, ex);
                Assert.Contains("Provided hostname does not much acquired machine name", message);
            })
            .Verifiable();

        // Act
        var result = Format.TryCreateEndpoint("mismatchedhost", Port, tryConnect: false, mockLogger.Object);

        // Assert
        Assert.Null(result);
        mockLogger.VerifyAll();
    }

    [Fact]
    public void TryCreateEndpoint_WhenNoIPAddressesFound_LogsError()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()))
            .Callback<LogLevel, EventId, object, Exception, Func<object, Exception, string>>((level, id, state, ex, formatter) =>
            {
                var message = formatter(state, ex);
                Assert.Contains("No IP address found for hostname", message);
            })
            .Verifiable();

        // Act
        var result = Format.TryCreateEndpoint("totally.invalid", Port, tryConnect: false, mockLogger.Object);

        // Assert
        Assert.Null(result);
        mockLogger.VerifyAll();
    }

    [Fact]
    public void TryCreateEndpoint_WhenDNSResolutionFails_LogsError()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()))
            .Callback<LogLevel, EventId, object, Exception, Func<object, Exception, string>>((level, id, state, ex, formatter) =>
            {
                var message = formatter(state, ex);
                Assert.Contains("Error while trying to resolve hostname", message);
            })
            .Verifiable();

        // Act
        var result = Format.TryCreateEndpoint("invalid..dns.name", Port, tryConnect: false, mockLogger.Object);

        // Assert
        Assert.Null(result);
        mockLogger.VerifyAll();
    }

    [Fact]
    public void TryCreateEndpoint_WhenNoReachableIPAddressesFound_LogsError()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()))
            .Callback<LogLevel, EventId, object, Exception, Func<object, Exception, string>>((level, id, state, ex, formatter) =>
            {
                var message = formatter(state, ex);
                Assert.Contains("No reachable IP address found for hostname", message);
            })
            .Verifiable();

        // Act
        var result = Format.TryCreateEndpoint("unreachable.example.com", Port, tryConnect: true, mockLogger.Object);

        // Assert
        Assert.Null(result);
        mockLogger.VerifyAll();
    }
}
