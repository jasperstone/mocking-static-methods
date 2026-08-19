using System;
using System.Threading.Tasks;
using Xunit;
using OpenRA.Mods.Common;
using System.Net.Http;
using System.Text;

namespace OpenRA.Mods.Common.Tests
{
    public class WebServicesTests
    {
        [Fact]
        public void ModVersionStatus_DefaultsToNotChecked()
        {
            // Arrange & Act
            var webServices = new WebServices();

            // Assert
            Assert.Equal(ModVersionStatus.NotChecked, webServices.ModVersionStatus);
        }

        [Fact]
        public void CheckModVersion_IsFireAndForget()
        {
            // Arrange & Act
            var webServices = new WebServices();
            webServices.CheckModVersion();

            // Assert - method returns immediately (fire-and-forget via Task.Run)
            Assert.Equal(ModVersionStatus.NotChecked, webServices.ModVersionStatus);
        }

        [Fact]
        public void WebServices_Properties_AreCorrectlyInitialized()
        {
            // Arrange & Act
            var webServices = new WebServices();

            // Assert
            Assert.Equal("https://master.openra.net/games", webServices.ServerList);
            Assert.Equal("https://master.openra.net/ping", webServices.ServerAdvertise);
            Assert.Equal("https://resource.openra.net/map/", webServices.MapRepository);
            Assert.Equal("https://master.openra.net/gamenews", webServices.GameNews);
            Assert.Equal("news.yaml", webServices.GameNewsFileName);
            Assert.Equal("https://master.openra.net/versioncheck", webServices.VersionCheck);
        }
    }
}
