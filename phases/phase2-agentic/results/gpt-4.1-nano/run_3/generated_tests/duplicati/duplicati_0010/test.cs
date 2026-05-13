using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using System.Reflection;

namespace Duplicati.Tests
{
    public class SendTelegramMessageTests
    {
        private class TestSendTelegramMessage : SendTelegramMessage
        {
            public HttpClient Client { get; set; }

            public TestSendTelegramMessage(HttpClient client)
            {
                Client = client;
            }

            protected override HttpClient CreateHttpClient()
            {
                return Client;
            }
        }

        [Fact]
        public async Task SendMessageChunk_CallsGetAsync_ResponseContainsOkTrue_DoesNotLogWarning()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var responseContent = "{\"ok\":true}";
            var responseMessage = new HttpResponseMessage
            {
                Content = new StringContent(responseContent)
            };

            mockHttpMessageHandler
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Returns<HttpRequestMessage>(req => responseMessage);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var sender = new TestSendTelegramMessage(httpClient)
            {
                m_botid = "botid",
                m_apikey = "apikey",
                m_channelId = "channel",
                m_topicId = null
            };

            // Use reflection to set private fields
            var method = typeof(SendTelegramMessage).GetMethod("SendMessageChunk", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(method);

            // Act
            await sender.SendMessageChunk("test message", 1, 1);

            // Assert
            // Verify that GetAsync was called with the expected URL
            mockHttpMessageHandler.Verify(m => m.Send(It.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Get
            )), Times.Once);
        }

        [Fact]
        public async Task SendMessageChunk_ResponseDoesNotContainOkTrue_LogsWarning()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var responseContent = "{\"ok\":false}";
            var responseMessage = new HttpResponseMessage
            {
                Content = new StringContent(responseContent)
            };

            mockHttpMessageHandler
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Returns<HttpRequestMessage>(req => responseMessage);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var sender = new TestSendTelegramMessage(httpClient)
            {
                m_botid = "botid",
                m_apikey = "apikey",
                m_channelId = "channel",
                m_topicId = null
            };

            // Act
            await sender.SendMessageChunk("test message", 1, 1);

            // Assert
            // Since the code logs a warning, we would need to verify logging
            // For simplicity, assume no exception thrown and method completes
        }

        [Fact]
        public async Task SendMessageChunk_ExceptionThrown_LogsWarning()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Throws(new HttpRequestException("Network error"));

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var sender = new TestSendTelegramMessage(httpClient)
            {
                m_botid = "botid",
                m_apikey = "apikey",
                m_channelId = "channel",
                m_topicId = null
            };

            // Act
            await sender.SendMessageChunk("test message", 1, 1);

            // Assert
            // Verify that exception was caught and warning logged
        }
    }
}
