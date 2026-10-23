using System;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using OpenRA.Mods.Common.Server;
using OpenRA.Server;
using Xunit;

public class MasterServerPingerTests
{
    [Fact]
    public async Task UpdateMasterServer_ShouldSendPostRequest()
    {
        // Arrange
        var serverMock = new Mock<S>();
        var httpClientMock = new Mock<HttpClient>();
        var responseMock = new Mock<HttpResponseMessage>();
        var contentMock = new Mock<HttpContent>();

        var postData = "testPostData";
        var endpoint = new Uri("http://example.com");

        serverMock.Setup(s => s.ModData.GetOrCreate<WebServices>().ServerAdvertise).Returns(endpoint);
        httpClientMock.Setup(c => c.PostAsync(endpoint, It.IsAny<HttpContent>())).ReturnsAsync(responseMock.Object);
        responseMock.Setup(r => r.Content).Returns(contentMock.Object);
        contentMock.Setup(c => c.ReadAsStringAsync()).ReturnsAsync("testResponse");

        var pinger = new MasterServerPinger();

        // Act
        await pinger.UpdateMasterServer(serverMock.Object, postData);

        // Assert
        httpClientMock.Verify(c => c.PostAsync(endpoint, It.IsAny<HttpContent>()), Times.Once);
    }
}
