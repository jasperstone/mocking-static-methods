using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Network;
using OpenRA.Server;
using OpenRA.Support;
using OpenRA.Widgets;
using Xunit;

namespace OpenRA.Tests.Mods.Common.Widgets.Logic
{
    public sealed class ServerListLogicTests : IDisposable
    {
        private readonly HttpListener listener;
        private readonly Uri listenerUri;

        public ServerListLogicTests()
        {
            listener = new HttpListener();
            var prefix = $"http://localhost:{GetFreePort()}/";
            listenerUri = new Uri(prefix);
            listener.Prefixes.Add(prefix);
            listener.Start();
            Task.Run(() => HandleRequest(listener));
        }

        [Fact]
        public async Task RefreshServerList_PerformsHttpGetRequest()
        {
            using var widget = CreateWidget();
            using var modData = CreateModData(listenerUri);
            using var mre = new ManualResetEventSlim();

            var logic = new ServerListLogic(widget, modData, _ => { });
            mre.Wait(TimeSpan.FromSeconds(5));

            Assert.True(RequestReceived, "Expected HttpClient.GetAsync to reach the local listener.");
        }

        private volatile bool RequestReceived;

        private async Task HandleRequest(HttpListener listener)
        {
            while (listener.IsListening)
            {
                try
                {
                    var context = await listener.GetContextAsync();
                    RequestReceived = true;
                    using var writer = new StreamWriter(context.Response.OutputStream);
                    context.Response.ContentType = "text/plain";
                    await writer.WriteAsync(string.Empty);
                }
                catch (HttpListenerException)
                {
                    break;
                }
            }
        }

        private static Widget CreateWidget()
        {
            var widget = new Widget();
            widget.AddChild(new ScrollPanelWidget { Id = "SERVER_LIST" });
            return widget;
        }

        private static ModData CreateModData(Uri uri)
        {
            var modData = new ModData();
            modData.Register<WebServices>(() => new WebServices { ServerList = uri.ToString() });
            modData.Register(() => new Manifest());
            modData.Register(() => new ModMetadata());
            return modData;
        }

        private static int GetFreePort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        public void Dispose()
        {
            listener.Stop();
            listener.Close();
        }
    }

    internal sealed class TcpListener : IDisposable
    {
        private readonly System.Net.Sockets.TcpListener listener;

        public TcpListener(IPAddress address, int port)
        {
            listener = new System.Net.Sockets.TcpListener(address, port);
        }

        public void Start() => listener.Start();

        public void Stop() => listener.Stop();

        public EndPoint LocalEndpoint => listener.LocalEndpoint;

        public void Dispose() => listener.Stop();
    }

    internal sealed class Manifest
    {
        public ModMetadata Metadata { get; set; } = new ModMetadata();
        public string Id { get; set; } = "test-mod";
    }

    internal sealed class ModMetadata
    {
        public string Version { get; set; } = "1.0";
    }
}
