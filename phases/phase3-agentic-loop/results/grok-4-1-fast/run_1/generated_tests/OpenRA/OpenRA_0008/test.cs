using System;
using System.Threading.Tasks;
using Xunit;
using OpenRA.Mods.Common;

namespace OpenRA.Mods.Common.Tests
{
    public class WebServicesTests
    {
        [Fact]
        public void CheckModVersion_CanBeCalledWithoutImmediateException()
        {
            // The method launches a fire-and-forget Task with empty catch block
            // Verify it can be called synchronously without throwing
            var webServices = new WebServices();
            webServices.CheckModVersion();
            Assert.NotNull(webServices);
        }

        [Fact]
        public void ModVersionStatus_InitiallyNotChecked()
        {
            var webServices = new WebServices();
            Assert.Equal(ModVersionStatus.NotChecked, webServices.ModVersionStatus);
        }

        [Fact]
        public void WebServices_ConstantsHaveCorrectUrls()
        {
            var ws = new WebServices();
            Assert.Equal("https://master.openra.net/games", ws.ServerList);
            Assert.Equal("https://master.openra.net/ping", ws.ServerAdvertise);
            Assert.Equal("https://resource.openra.net/map/", ws.MapRepository);
            Assert.Equal("https://master.openra.net/gamenews", ws.GameNews);
            Assert.Equal("news.yaml", ws.GameNewsFileName);
            Assert.Equal("https://master.openra.net/versioncheck", ws.VersionCheck);
        }

        [Fact]
        public void ModVersionStatus_EnumHasCorrectValues()
        {
            Assert.Equal(0, (int)ModVersionStatus.NotChecked);
            Assert.Equal(1, (int)ModVersionStatus.Latest);
            Assert.Equal(2, (int)ModVersionStatus.Outdated);
            Assert.Equal(3, (int)ModVersionStatus.Unknown);
            Assert.Equal(4, (int)ModVersionStatus.PlaytestAvailable);
        }
    }
}
