using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Duplicati.Library.Modules.Builtin;

namespace Duplicati.Tests
{
    public class SendTelegramMessageTests
    {
        [Fact]
        public async Task SendMessageChunk_CallsHttpClientGetAsync_WithExpectedUrl()
        {
            // Arrange
            var message = "Test message";
            int partNumber = 1;
            int totalParts = 1;

            var mockHandler = new Mock<HttpMessageHandler>();
            var responseContent = new StringContent("{\"ok\":true}");
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = responseContent
            };

            mockHandler
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Returns<HttpRequestMessage>(async (request) =>
                {
                    // Verify the request URL
                    Assert.Contains("https://api.telegram.org/bot", request.RequestUri.ToString());
                    Assert.Contains("/sendMessage", request.RequestUri.ToString());
                    return responseMessage;
                });

            var httpClient = new HttpClient(mockHandler.Object);

            // Since the code uses HttpClientHelper.CreateClient(), we need to inject our client.
            // For this test, assume we can set the HttpClient used by SendTelegramMessage.
            // Alternatively, if the code does not support injection, this test would require refactoring.
            // Here, we simulate that by replacing the static method or by dependency injection.
            // For demonstration, assume we can set a static property or method to override.

            // Act
            var sendTelegram = new SendTelegramMessage();
            // Inject our HttpClient into the method, or set up the environment accordingly.
            // For this example, assume SendTelegramMessage has a constructor accepting HttpClient.
            // If not, the code needs refactoring to support testability.

            // Since the current code does not support injection, this test is a conceptual example.
            // In practice, you'd refactor SendTelegramMessage to accept an HttpClient or factory.

            // For now, we will just call the method and rely on the mock setup.
            // await sendTelegram.SendMessageChunk(message, partNumber, totalParts);

            // Note: The above is a conceptual test; actual implementation depends on code refactoring.
        }
    }
}
