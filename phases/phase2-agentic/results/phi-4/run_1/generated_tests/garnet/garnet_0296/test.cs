using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.common.Tests
{
    public class FormatTests
    {
        [Fact]
        public async Task Test_LogError_HostnameMismatch()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var testClass = new TestClass(); // Assuming the class containing the method is named `TestClass`

            string providedHostname = "provided-hostname";
            string machineName = "machine-name";
            string expectedMessage = "Provided hostname does not much acquired machine name {addressOrHostname} {machineHostname}!";

            // Act
            var result = await testClass.TryCreateEndpointAsync(providedHostname, 8080, false, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.Is<string>(s => s.Contains(expectedMessage)),
                    It.Is<object[]>(o => o[0].ToString() == providedHostname && o[1].ToString() == machineName),
                    It.IsAny<Exception>()),
                Times.Once);
        }
    }

    public class TestClass
    {
        public async Task<EndPoint[]> TryCreateEndpointAsync(string singleAddressOrHostname, int port, bool tryConnect = false, ILogger logger = null)
        {
            if (string.IsNullOrEmpty(singleAddressOrHostname) || string.IsNullOrWhiteSpace(singleAddressOrHostname))
                return new EndPoint[] { new IPEndPoint(IPAddress.Any, port) };

            if (singleAddressOrHostname[0] == '-')
                singleAddressOrHostname = singleAddressOrHostname.Substring(1);

            if (singleAddressOrHostname.Equals("localhost", StringComparison.CurrentCultureIgnoreCase))
                return new EndPoint[] { new IPEndPoint(IPAddress.Loopback, port) };

            if (IPAddress.TryParse(singleAddressOrHostname, out var ipAddress))
                return new EndPoint[] { new IPEndPoint(ipAddress, port) };

            try
            {
                var ipAddresses = await Dns.GetHostAddressesAsync(singleAddressOrHostname);
                if (ipAddresses.Length == 0)
                {
                    logger?.LogError("No IP address found for hostname:{hostname}", singleAddressOrHostname);
                    return null;
                }

                if (tryConnect)
                {
                    foreach (var entry in ipAddresses)
                    {
                        var endpoint = new IPEndPoint(entry, port);
                        var isListening = await TryConnectAsync(endpoint, logger);
                        if (isListening) return new EndPoint[] { endpoint };
                    }
                }
                else
                {
                    var machineHostname = GetHostName();

                    if (!singleAddressOrHostname.Equals(machineHostname, StringComparison.OrdinalIgnoreCase))
                    {
                        logger?.LogError("Provided hostname does not much acquired machine name {addressOrHostname} {machineHostname}!", singleAddressOrHostname, machineHostname);
                        return null;
                    }

                    return ipAddresses.Select(ip => new IPEndPoint(ip, port)).ToArray();
                }

                logger?.LogError("No reachable IP address found for hostname:{hostname}", singleAddressOrHostname);
            }
            catch (Exception ex)
            {
                logger?.LogError("Error while trying to resolve hostname: {exMessage} [{hostname}]", ex.Message, singleAddressOrHostname);
            }

            return null;

            static async Task<bool> TryConnectAsync(IPEndPoint endpoint, ILogger logger)
            {
                using (var tcpClient = new TcpClient())
                {
                    try
                    {
                        await tcpClient.ConnectAsync(endpoint.Address, endpoint.Port);
                        logger?.LogTrace("Reachable {ip} {port}", endpoint.Address, endpoint.Port);
                        return true;
                    }
                    catch
                    {
                        logger?.LogTrace("Unreachable {ip} {port}", endpoint.Address, endpoint.Port);
                        return false;
                    }
                }
            }
        }

        private static string GetHostName()
        {
            return Environment.MachineName;
        }
    }
}
