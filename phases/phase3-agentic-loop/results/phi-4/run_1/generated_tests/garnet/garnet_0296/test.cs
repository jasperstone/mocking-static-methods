using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Garnet.common
{
    internal static class ExtensionMethodsInternal
    {
        internal static bool IsNullOrEmpty([NotNullWhen(false)] this string s) =>
            string.IsNullOrEmpty(s);

        internal static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string s) =>
            string.IsNullOrWhiteSpace(s);
    }

    public static class Format
    {
        private static Func<string> _getHostNameFunc = Dns.GetHostName;
        private static Func<string, Task<IPAddress[]>> _getHostAddressesAsyncFunc = Dns.GetHostAddressesAsync;

        public static Func<string> GetHostNameFunc
        {
            get => _getHostNameFunc;
            set => _getHostNameFunc = value ?? throw new ArgumentNullException(nameof(value));
        }

        public static Func<string, Task<IPAddress[]>> GetHostAddressesAsyncFunc
        {
            get => _getHostAddressesAsyncFunc;
            set => _getHostAddressesAsyncFunc = value ?? throw new ArgumentNullException(nameof(value));
        }

        static EndPoint[] DefaultBindAny(int port)
            => Socket.OSSupportsIPv6 ? [new IPEndPoint(IPAddress.Any, port), new IPEndPoint(IPAddress.IPv6Any, port)] : [new IPEndPoint(IPAddress.Any, port)];

        static EndPoint[] DefaultBindLoopBack(int port)
            => Socket.OSSupportsIPv6 ? [new IPEndPoint(IPAddress.Loopback, port), new IPEndPoint(IPAddress.IPv6Loopback, port)] : [new IPEndPoint(IPAddress.Loopback, port)];

        public static async ValueTask<EndPoint[]> TryCreateEndpointAsync(string singleAddressOrHostname, int port, bool tryConnect = false, ILogger logger = null)
        {
            if (string.IsNullOrEmpty(singleAddressOrHostname) || string.IsNullOrWhiteSpace(singleAddressOrHostname))
                return DefaultBindAny(port);

            if (singleAddressOrHostname[0] == '-')
                singleAddressOrHostname = singleAddressOrHostname.Substring(1);

            if (singleAddressOrHostname.Equals("localhost", StringComparison.CurrentCultureIgnoreCase))
                return DefaultBindLoopBack(port);

            if (IPAddress.TryParse(singleAddressOrHostname, out var ipAddress))
                return [new IPEndPoint(ipAddress, port)];

            try
            {
                var ipAddresses = await GetHostAddressesAsyncFunc(singleAddressOrHostname).ConfigureAwait(false);
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
                        var isListening = await TryConnectAsync(endpoint, logger).ConfigureAwait(false);
                        if (isListening) return [endpoint];
                    }
                }
                else
                {
                    var machineHostname = GetHostNameFunc();

                    if (!singleAddressOrHostname.Equals(machineHostname, StringComparison.OrdinalIgnoreCase))
                    {
                        logger?.LogError("Provided hostname does not match acquired machine name {addressOrHostname} {machineHostname}!", singleAddressOrHostname, machineHostname);
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
                        await tcpClient.ConnectAsync(endpoint.Address, endpoint.Port).ConfigureAwait(false);
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
    }
}
