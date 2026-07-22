using System;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using OpenRA.Mods.Common;
using Xunit;

public class ItchIntegrationTests
{
    [Fact]
    public async Task GetPlayerName_ShouldCallGetAsync()
    {
        // Arrange
        var mockHttpClient = new Mock<HttpClient>();
        var itchIntegration = new ItchIntegration();

        // Act
        itchIntegration.GetPlayerName(name => { });

        // Assert
        mockHttpClient.Verify(client => client.GetAsync(It.IsAny<string>()), Times.Once);
    }
}
