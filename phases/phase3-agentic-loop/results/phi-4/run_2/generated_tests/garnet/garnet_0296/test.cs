public static class Format
{
    private static IDnsResolver DnsResolver { get; set; } = new DnsResolver();

    public static EndPoint[] TryCreateEndpoint(string singleAddressOrHostname, int port, bool tryConnect = false, ILogger logger = null, IDnsResolver dnsResolver = null)
    {
        DnsResolver = dnsResolver ?? DnsResolver;

        if (string.IsNullOrEmpty(singleAddressOrHostname) || string.IsNullOrWhiteSpace(singleAddressOrHostname))
            return defaultBindAny(port);

        if (singleAddressOrHostname[0] == '-')
            singleAddressOrHostname = singleAddressOrHostname.Substring(1);

        if (singleAddressOrHostname.Equals("localhost", StringComparison.CurrentCultureIgnoreCase))
            return defaultBindLoopBack(port);

        if (IPAddress.TryParse(singleAddressOrHostname, out var ipAddress))
            return [new IPEndPoint(ipAddress, port)];

        try
        {
            var ipAddresses = DnsResolver.GetHostAddresses(singleAddressOrHostname);
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
                    var IsListening = TryConnect(endpoint);
                    if (IsListening) return [endpoint];
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

        bool TryConnect(IPEndPoint endpoint)
        {
            using (var tcpClient = new TcpClient())
            {
                try
                {
                    tcpClient.Connect(endpoint.Address, endpoint.Port);
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

    private static EndPoint[] defaultBindAny(int port) => new[] { new IPEndPoint(IPAddress.Any, port) };
    private static EndPoint[] defaultBindLoopBack(int port) => new[] { new IPEndPoint(IPAddress.Loopback, port) };
    private static string GetHostName() => Dns.GetHostName();
}
