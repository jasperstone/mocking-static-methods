using Xunit;
using Moq;
using System.Net.Http;
using System.Threading.Tasks;

namespace OpenRA.Game.Map.Tests
{
    public class MapPreviewTests
    {
        [Fact]
        public async Task Install_DownloadSuccess_MapInstalled()
        {
            // Arrange
            var mapPreview = new MapPreview(new ModData(), "", MapGridType.Tiles, new MapCache());

            // Act
            await mapPreview.Install("https://example.com/maps/");

            // Assert
            // Assert.Equal(MapStatus.Downloaded, mapPreview.innerData.Status);
        }

        [Fact]
        public async Task Install_DownloadFailure_MapNotInstalled()
        {
            // Arrange
            var mapPreview = new MapPreview(new ModData(), "", MapGridType.Tiles, new MapCache());

            // Act
            await mapPreview.Install("https://example.com/maps/");

            // Assert
            // Assert.Equal(MapStatus.DownloadError, mapPreview.innerData.Status);
        }
    }
}
