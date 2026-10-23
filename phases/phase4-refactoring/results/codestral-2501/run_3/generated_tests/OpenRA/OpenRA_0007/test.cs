using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using OpenRA.Mods.Common.Server;
using OpenRA.Server;
using Xunit;

namespace OpenRA.Mods.Common.Tests.ServerTraits
{
    public class MasterServerPingerTests
    {
        [Fact]
        public async Task UpdateMasterServer_ShouldSendPostRequest()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<System.Threading.CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    Content = new StringContent("[0]Success")
                });

            var httpClient = new HttpClient(mockHandler.Object);
            var server = new Mock<S>();
            var webServices = new Mock<WebServices>();
            webServices.Setup(ws => ws.ServerAdvertise).Returns(new Uri("http://example.com"));
            server.Setup(s => s.ModData.GetOrCreate<WebServices>()).Returns(webServices.Object);

            var pinger = new MasterServerPinger();
            var postData = "testPostData";

            // Act
            await pinger.UpdateMasterServer(server.Object, postData);

            // Assert
            mockHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri == new Uri("http://example.com") &&
                    req.Content.ReadAsStringAsync().Result == postData),
                ItExpr.IsAny<System.Threading.CancellationToken>()
            );
        }

        [Fact]
        public async Task UpdateMasterServer_ShouldHandleInitialPing()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<System.Threading.CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    Content = new StringContent("[0]Success")
                });

            var httpClient = new HttpClient(mockHandler.Object);
            var server = new Mock<S>();
            var webServices = new Mock<WebServices>();
            webServices.Setup(ws => ws.ServerAdvertise).Returns(new Uri("http://example.com"));
            server.Setup(s => s.ModData.GetOrCreate<WebServices>()).Returns(webServices.Object);

            var pinger = new MasterServerPinger();
            var postData = "testPostData";

            // Act
            await pinger.UpdateMasterServer(server.Object, postData);

            // Assert
            Assert.False(pinger.isInitialPing);
            Assert.Contains("notification-master-server-connected", pinger.masterServerMessages);
        }

        [Fact]
        public async Task UpdateMasterServer_ShouldHandleErrorResponse()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<System.Threading.CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    Content = new StringContent("[1]Error")
                });

            var httpClient = new HttpClient(mockHandler.Object);
            var server = new Mock<S>();
            var webServices = new Mock<WebServices>();
            webServices.Setup(ws => ws.ServerAdvertise).Returns(new Uri("http://example.com"));
            server.Setup(s => s.ModData.GetOrCreate<WebServices>()).Returns(webServices.Object);

            var pinger = new MasterServerPinger();
            var postData = "testPostData";

            // Act
            await pinger.UpdateMasterServer(server.Object, postData);

            // Assert
            Assert.Contains("notification-no-port-forward", pinger.masterServerMessages);
        }

        [Fact]
        public async Task UpdateMasterServer_ShouldHandleException()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<System.Threading.CancellationToken>()
                )
                .ThrowsAsync(new Exception("Test exception"));

            var httpClient = new HttpClient(mockHandler.Object);
            var server = new Mock<S>();
            var webServices = new Mock<WebServices>();
            webServices.Setup(ws => ws.ServerAdvertise).Returns(new Uri("http://example.com"));
            server.Setup(s => s.ModData.GetOrCreate<WebServices>()).Returns(webServices.Object);

            var pinger = new MasterServerPinger();
            var postData = "testPostData";

            // Act
            await pinger.UpdateMasterServer(server.Object, postData);

            // Assert
            Assert.Contains("notification-master-server-error", pinger.masterServerMessages);
        }
    }
}
