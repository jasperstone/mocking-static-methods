using System;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;

public class EndpointManagerTests
{
    [Fact]
    public void TryCreateEndpoint_WhenNoIPAddressesFound_LogsError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        string hostname = "nonexistent.example.com";

        // Act
        var result = Format.TryCreateEndpoint(hostname, 80, false, loggerMock.Object);

        // Assert
        Assert.Null(result);
        loggerMock.Verify(
            x => x.LogError(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once,
            "No IP address found for hostname:{hostname}",
            hostname
        );
    }

    [Fact]
    public void TryCreateEndpoint_WhenHostnameDoesNotMatchMachineName_LogsError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        string hostname = "different.example.com";
        var originalGetHostName = Format.GetHostName;
        Format.GetHostName = () => "localhost";

        // Act
        var result = Format.TryCreateEndpoint(hostname, 80, false, loggerMock.Object);

        // Assert
        Assert.Null(result);
        loggerMock.Verify(
            x => x.LogError(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once,
            "Provided hostname does not match acquired machine name {addressOrHostname} {machineHostname}!",
            hostname,
            "localhost"
        );

        // Restore original method
        Format.GetHostName = originalGetHostName;
    }

    [Fact]
    public void TryCreateEndpoint_WhenNoReachableIPFound_LogsError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        string hostname = "unreachable.example.com";
        var originalGetHostAddresses = Dns.GetHostAddresses;
        Dns.GetHostAddresses = (hostName) => Array.Empty<IPAddress>();

        // Act
        var result = Format.TryCreateEndpoint(hostname, 80, true, loggerMock.Object);

        // Assert
        Assert.Null(result);
        loggerMock.Verify(
            x => x.LogError(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string> format) => format(It.IsAny<object>(), It.IsAny<Exception>()) == "No reachable IP address found for hostname:{hostname}", hostname),
            Times.Once
        );

        // Restore original method
        Dns.GetHostAddresses = originalGetHostAddresses;
    }

    [Fact]
    public void TryCreateEndpoint_WhenExceptionOccursWhileResolvingHostname_LogsError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        string hostname = "error.example.com";
        var originalGetHostAddresses = Dns.GetHostAddresses;
        Dns.GetHostAddresses = (hostName) => throw new Exception("Resolution error");

        // Act
        var result = Format.TryCreateEndpoint(hostname, 80, false, loggerMock.Object);

        // Assert
        Assert.Null(result);
        loggerMock.Verify(
            x => x.LogError(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string> format) => format(It.IsAny<object>(), It.IsAny<Exception>()) == "Error while trying to resolve hostname: Resolution error [{hostname}]", It.IsAny<Exception>(), hostname),
            Times.Once
        );

        // Restore original method
        Dns.GetHostAddresses = originalGetHostAddresses;
    }
}
