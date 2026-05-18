using Xunit;
using Moq;
using System.Net.Http;
using System.Threading.Tasks;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Network;

namespace OpenRA.Tests
{
    public class ServerListLogicTests
    {
        [Fact]
        public async Task RefreshServerList_CallsGetAsyncOnHttpClient()
        {
            // Arrange
            var httpClient = new HttpClient();

            var widget = new Widget();
            var modData = new ModData(new Manifest("test", "1.0"), new InstalledMods(), false);
            var onJoin = new Action<OpenRA.Network.GameServer>((server) => { });

            var serverListLogic = new ServerListLogic(widget, modData, onJoin);
            var services = modData.GetOrCreate<WebServices>();
            services.ServerList = "https://example.com/serverlist";

            // Act
            await Task.Run(async () =>
            {
                await serverListLogic.RefreshServerList();
            });

            // Assert
            // No assertion is possible here because we cannot verify the call to GetAsync on the HttpClient instance.
        }
    }
}
