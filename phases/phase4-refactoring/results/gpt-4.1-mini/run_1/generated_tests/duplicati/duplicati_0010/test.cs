using System;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Duplicati.Library.Modules.Builtin;

namespace Duplicati.Tests.Library.Modules.Builtin
{
    public class SendTelegramMessageTests
    {
        [Fact]
        public async Task SendMessageChunk_CompletesSuccessfully_WithValidData()
        {
            var sendTelegram = new SendTelegramMessage();

            // Set private fields to valid values
            var botidField = typeof(SendTelegramMessage).GetField("m_botid", BindingFlags.NonPublic | BindingFlags.Instance);
            var apikeyField = typeof(SendTelegramMessage).GetField("m_apikey", BindingFlags.NonPublic | BindingFlags.Instance);
            var channelIdField = typeof(SendTelegramMessage).GetField("m_channelId", BindingFlags.NonPublic | BindingFlags.Instance);
            var topicIdField = typeof(SendTelegramMessage).GetField("m_topicId", BindingFlags.NonPublic | BindingFlags.Instance);

            botidField.SetValue(sendTelegram, "testbotid");
            apikeyField.SetValue(sendTelegram, "testapikey");
            channelIdField.SetValue(sendTelegram, "testchannel");
            topicIdField.SetValue(sendTelegram, "testtopic");

            // Get private method SendMessageChunk
            var method = typeof(SendTelegramMessage).GetMethod("SendMessageChunk", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(method);

            // Invoke SendMessageChunk asynchronously
            var task = (Task)method.Invoke(sendTelegram, new object[] { "Test message", 1, 1 });
            await task; // Should complete without exception
        }
    }
}
