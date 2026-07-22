using Xunit;
using Moq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library.Modules.Builtin;

namespace Duplicati.Tests
{
    public class SendTelegramMessageTests
    {
        [Fact]
        public async Task SendMessageChunk_SendsMessageSuccessfully()
        {
            // Arrange
            var httpClient = new HttpClient();
            var sendTelegramMessage = new SendTelegramMessage();
            var message = "Test message";
            var partNumber = 1;
            var totalParts = 1;

            // Act
            await sendTelegramMessage.SendMessageChunk(message, partNumber, totalParts);

            // Assert
            // Add assertions as needed
        }

        [Fact]
        public async Task SendMessageChunk_SendsMessageWithMultipleParts()
        {
            // Arrange
            var httpClient = new HttpClient();
            var sendTelegramMessage = new SendTelegramMessage();
            var message = "Test message";
            var partNumber = 1;
            var totalParts = 2;

            // Act
            await sendTelegramMessage.SendMessageChunk(message, partNumber, totalParts);

            // Assert
            // Add assertions as needed
        }

        [Fact]
        public async Task SendMessageChunk_HandlesException()
        {
            // Arrange
            var httpClient = new HttpClient();
            var sendTelegramMessage = new SendTelegramMessage();
            var message = "Test message";
            var partNumber = 1;
            var totalParts = 1;

            // Act and Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => sendTelegramMessage.SendMessageChunk(message, partNumber, totalParts));
        }
    }
}
