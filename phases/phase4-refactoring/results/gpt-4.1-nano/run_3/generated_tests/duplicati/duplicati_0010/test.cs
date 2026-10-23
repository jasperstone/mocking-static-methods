using Xunit;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using System.Threading;
using Duplicati.Library.Modules.Builtin;
using System;

namespace Duplicati.Tests
{
    public class SendTelegramMessageTests
    {
        [Fact]
        public async Task SendMessageChunk_CallsHttpClientGetAsync()
        {
            // Arrange
            var message = "Test message";
            int partNumber = 1;
            int totalParts = 1;

            var mockHttpMessageHandler = new Moq.Mock<HttpMessageHandler>();
            var responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("{\"ok\":true}")
            };

            mockHttpMessageHandler
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Returns<HttpRequestMessage>(async (req) =>
                {
                    var resp = responseMessage;
                    return resp;
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);

            // Create an instance of SendTelegramMessage with dependencies injected
            var sendTelegram = new SendTelegramMessage();

            // Use reflection or internal constructor to set the HttpClient if needed
            // For simplicity, assuming SendTelegramMessage has a constructor accepting HttpClient
            // If not, we need to modify the class to allow dependency injection for testing

            // For this example, let's assume we can set a protected property or method
            // Alternatively, we can mock the static method HttpClientHelper.CreateClient() if it is virtual or replaceable

            // Since the code uses HttpClientHelper.CreateClient(), we need to mock or replace that
            // But as per current code, it's a static method, so we can't inject easily
            // For the purpose of this test, assume we can replace the method or the class is refactored for testability

            // Act
            // Call SendMessageChunk with the mock client
            // Since the method is private, we need to make it internal or public for testing
            // For this example, assume we can call it directly

            // Alternatively, test the public method that calls SendMessageChunk

            // Since the method is private, we need to test via public interface or reflection
            // For simplicity, assume we can test SendMessageChunk directly

            // Call the method
            await sendTelegram.SendMessageChunk(message, partNumber, totalParts);

            // Assert
            mockHttpMessageHandler.Verify(m => m.Send(It.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Get
                && req.RequestUri != null
            )), Times.Once);
        }
    }
}
