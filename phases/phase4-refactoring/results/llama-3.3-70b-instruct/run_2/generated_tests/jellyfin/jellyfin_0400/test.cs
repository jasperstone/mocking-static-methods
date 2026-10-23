using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Configuration;

namespace Jellyfin.Server.Implementations.Trickplay.Tests
{
    public class TrickplayManagerTests
    {
        [Fact]
        public async Task LogInformation_Called_When_Trickplay_Files_Created()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TrickplayManager>>();
            var trickplayManager = new TrickplayManager(loggerMock.Object, null, null, null, null, null, null, null, null);

            // Act
            await trickplayManager.RefreshTrickplayDataAsync(new Video(), false, new LibraryOptions(), default);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
