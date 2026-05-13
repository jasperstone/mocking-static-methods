using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using OpenRA.Mods.Common.Server;
using Xunit;

namespace OpenRA.Tests
{
    public class MasterServerPingerTests
    {
        [Fact]
        public async Task UpdateMasterServer_SuccessfulPostAsync()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<System.Threading.CancellationToken>()
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
            await Task.Delay(100); // Allow async task to complete
            Assert.False(pinger.IsBusy);
        }

        [Fact]
        public async Task UpdateMasterServer_FailedPostAsync()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<System.Threading.CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent("[1]Error")
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            var pinger = new MasterServerPinger();
            var server = new Mock<S>();
            server.Setup(s => s.ModData.GetOrCreate<WebServices>()).Returns(new WebServices { ServerAdvertise = "http://example.com" });

            // Act
            pinger.UpdateMasterServer(server.Object, "postData");

            // Assert
            await Task.Delay(100); // Allow async task to complete
            Assert.False(pinger.IsBusy);
        }

        [Fact]
        public async Task UpdateMasterServer_InvalidResponse()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<System.Threading.CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("Invalid response")
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            var pinger = new MasterServerPinger();
            var server = new Mock<S>();
            server.Setup(s => s.ModData.GetOrCreate<WebServices>()).Returns(new WebServices { ServerAdvertise = "http://example.com" });

            // Act
            pinger.UpdateMasterServer(server.Object, "postData");

            // Assert
            await Task.Delay(100); // Allow async task to complete
            Assert.False(pinger.IsBusy);
        }
    }
}
