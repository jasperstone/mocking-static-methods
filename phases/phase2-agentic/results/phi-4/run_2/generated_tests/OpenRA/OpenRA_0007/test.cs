using Moq;
using Moq.Protected;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using OpenRA.Mods.Common.Server;

namespace OpenRA.Tests
{
    public class MasterServerPingerTests
    {
        [Fact]
        public async Task UpdateMasterServer_SuccessfulPostAsync_ReturnsConnectedMessage()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("[0]Success")
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            var pinger = new MasterServerPinger();
            var server = new Mock<S>();
            server.Setup(s => s.ModData.GetOrCreate<WebServices>()).Returns(new WebServices { ServerAdvertise = "http://example.com" });

            // Act
            pinger.UpdateMasterServer(server.Object, "postData");

            // Assert
            await Task.Delay(100); // Wait for the async operation to complete
            Assert.Contains("Connected", pinger.MasterServerMessages);
        }

        [Fact]
        public async Task UpdateMasterServer_PostAsyncWithErrorCode_ReturnsErrorMessage()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("[1]Port Forward Required")
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            var pinger = new MasterServerPinger();
            var server = new Mock<S>();
            server.Setup(s => s.ModData.GetOrCreate<WebServices>()).Returns(new WebServices { ServerAdvertise = "http://example.com" });

            // Act
            pinger.UpdateMasterServer(server.Object, "postData");

            // Assert
            await Task.Delay(100); // Wait for the async operation to complete
            Assert.Contains("notification-no-port-forward", pinger.MasterServerMessages);
        }

        [Fact]
        public async Task UpdateMasterServer_PostAsyncThrowsException_ReturnsError()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException());

            var client = new HttpClient(mockHttpMessageHandler.Object);
            var pinger = new MasterServerPinger();
            var server = new Mock<S>();
            server.Setup(s => s.ModData.GetOrCreate<WebServices>()).Returns(new WebServices { ServerAdvertise = "http://example.com" });

            // Act
            pinger.UpdateMasterServer(server.Object, "postData");

            // Assert
            await Task.Delay(100); // Wait for the async operation to complete
            Assert.Contains("notification-master-server-error", pinger.MasterServerMessages);
        }
    }
}
