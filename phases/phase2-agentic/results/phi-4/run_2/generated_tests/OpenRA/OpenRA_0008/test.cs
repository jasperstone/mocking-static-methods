using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using OpenRA.Mods.Common;
using Xunit;

public class WebServicesTests
{
    [Fact]
    public async Task CheckModVersion_OutdatedVersion()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("outdated")
        };

        handlerMock
           .Protected()
           .Setup<Task<HttpResponseMessage>>(
               "SendAsync",
               ItExpr.IsAny<HttpRequestMessage>(),
               ItExpr.IsAny<CancellationToken>()
           )
           .ReturnsAsync(response);

        var httpClient = new HttpClient(handlerMock.Object);
        var webServices = new WebServices
        {
            // Assuming Game.EngineVersion, Game.ModData.Manifest.Id, and Game.ModData.Manifest.Metadata.Version are accessible
            // For the purpose of this test, we'll mock these values
            // You might need to adjust this part based on actual implementation
        };

        // Act
        webServices.CheckModVersion();

        // Allow some time for the async operation to complete
        await Task.Delay(100);

        // Assert
        Assert.Equal(ModVersionStatus.Outdated, webServices.ModVersionStatus);
    }
}
