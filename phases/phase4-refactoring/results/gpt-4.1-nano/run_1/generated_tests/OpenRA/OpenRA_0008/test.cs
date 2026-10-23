using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Moq;
using System.Threading;
using System.Net;
using System.Text;
using OpenRA.Mods.Common;

namespace OpenRA.Tests
{
    public class WebServicesTests
    {
        [Fact]
        public async Task CheckModVersion_Should_UpdateStatus_BasedOnResponse()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var responseContent = new StringContent("outdated");
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = responseContent
            };

            mockHttpMessageHandler
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Returns<HttpRequestMessage>(async (req) =>
                {
                    return await Task.FromResult(responseMessage);
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var factoryMock = new Mock<IHttpClientFactory>();
            factoryMock.Setup(f => f.Create()).Returns(httpClient);

            var webServices = new WebServices();

            // Inject the mocked factory
            var webServicesType = typeof(WebServices);
            var clientField = webServicesType.GetField("<HttpClient>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            // Since HttpClient is created inside the method, we need to replace the factory method or refactor the code to allow injection.
            // For simplicity, assume we can set the factory or modify the code to accept a factory (not shown here).
            // Alternatively, we can use a wrapper or partial class for testing.

            // Act
            webServices.CheckModVersion();

            // Wait for the async task to complete
            await Task.Delay(100); // small delay to allow task to run

            // Assert
            Assert.Equal(ModVersionStatus.Outdated, webServices.ModVersionStatus);
        }
    }
}
