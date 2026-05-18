using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.common;

public class FormatTests
{
    [Fact]
    public void TryCreateEndpoint_NoIPAddressesFound_LogsError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        string hostname = "nonexistent.domain";

        // Act
        var result = Format.TryCreateEndpoint(hostname, 8080, logger: loggerMock.Object);

        // Assert
        Assert.Null(result);
        loggerMock.Verify(
            l => l.LogError(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("No IP address found for hostname:")), hostname),
            Times.Once);
    }

    [Fact]
    public void TryCreateEndpoint_HostnameDoesNotMatchMachineName_LogsError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        string hostname = "different.hostname";
        var machineHostname = "current.hostname";

        // Mock static method
        var getHostNameMock = new Mock<Func<string>>();
        getHostNameMock.Setup(f => f()).Returns(machineHostname);

        // Use a helper method to replace the static call
        Format.GetHostName = getHostNameMock.Object;

        // Act
        var result = Format.TryCreateEndpoint(hostname, 8080, logger: loggerMock.Object);

        // Assert
        Assert.Null(result);
        loggerMock.Verify(
            l => l.LogError(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("Provided hostname does not much acquired machine name")), hostname, machineHostname),
            Times.Once);
    }

    [Fact]
    public void TryCreateEndpoint_NoReachableIPAddresses_LogsError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        string hostname = "unreachable.hostname";

        // Mock static method
        var getHostAddressesMock = new Mock<Func<string, IPAddress[]>>();
        getHostAddressesMock.Setup(f => f(hostname)).Returns(new IPAddress[0]);

        // Use a helper method to replace the static call
        Format.DnsGetHostAddresses = getHostAddressesMock.Object;

        // Act
        var result = Format.TryCreateEndpoint(hostname, 8080, tryConnect: true, logger: loggerMock.Object);

        // Assert
        Assert.Null(result);
        loggerMock.Verify(
            l => l.LogError(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("No reachable IP address found for hostname:")), hostname),
            Times.Once);
    }

    [Fact]
    public void TryCreateEndpoint_ExceptionWhileResolvingHostname_LogsError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        string hostname = "error.hostname";
        Exception exception = new Exception("Resolution error");

        // Mock static method
        var getHostAddressesMock = new Mock<Func<string, IPAddress[]>>();
        getHostAddressesMock.Setup(f => f(hostname)).Throws(exception);

        // Use a helper method to replace the static call
        Format.DnsGetHostAddresses = getHostAddressesMock.Object;

        // Act
        var result = Format.TryCreateEndpoint(hostname, 8080, logger: loggerMock.Object);

        // Assert
        Assert.Null(result);
        loggerMock.Verify(
            l => l.LogError(exception, It.Is<string>(s => s.Contains("Error while trying to resolve hostname:")), hostname),
            Times.Once);
    }
}

// Helper class to replace static methods
internal static class Format
{
    public static Func<string> GetHostName { get; set; } = () => Dns.GetHostName();
    public static Func<string, IPAddress[]> DnsGetHostAddresses { get; set; } = Dns.GetHostAddresses;
}
