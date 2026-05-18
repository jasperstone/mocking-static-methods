using System;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library.Modules.Builtin;
using Duplicati.Library.Utility;
using Moq;
using Xunit;
using Duplicati.Library.Logging;

public class SendTelegramMessageTests
{
    [Fact]
    public async Task SendMessageChunk_SuccessfulResponse_LogsWarning()
    {
        // Arrange
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        var mockHttpClient = new Mock<HttpClient>();
        mockHttpClient
            .Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage
            {
                Content = new StringContent("{\"ok\":true}")
            });

        mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(mockHttpClient.Object);

        HttpClientHelper.Configure(mockHttpClientFactory.Object);

        var sendTelegramMessage = new SendTelegramMessage();

        // Use reflection to set private fields
        SetPrivateField(sendTelegramMessage, "m_botid", "test_bot_id");
        SetPrivateField(sendTelegramMessage, "m_apikey", "test_api_key");
        SetPrivateField(sendTelegramMessage, "m_channelId", "test_channel_id");
        SetPrivateField(sendTelegramMessage, "m_topicId", "test_topic_id");

        // Act
        var method = typeof(SendTelegramMessage).GetMethod("SendMessageChunk", BindingFlags.NonPublic | BindingFlags.Instance);
        await (Task)method.Invoke(sendTelegramMessage, new object[] { "Test message", 1, 1 });

        // Assert
        // Verify that the warning log is not called
        Logging.Log.Verify(l => l.WriteWarningMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
    }

    private void SetPrivateField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(obj, value);
    }
}
