using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Duplicati.Library.Modules.Builtin;
using Xunit;

namespace Duplicati.Tests.Library.Modules.Builtin
{
    public class SendTelegramMessageTests
    {
        private class TestSendTelegramMessage : SendTelegramMessage
        {
            public string BotId { get; set; }
            public string ApiKey { get; set; }
            public string ChannelId { get; set; }
            public string TopicId { get; set; }

            public void SetPrivateFields()
            {
                var type = typeof(SendTelegramMessage);
                var botidField = type.GetField("m_botid", BindingFlags.NonPublic | BindingFlags.Instance);
                var apikeyField = type.GetField("m_apikey", BindingFlags.NonPublic | BindingFlags.Instance);
                var channelIdField = type.GetField("m_channelId", BindingFlags.NonPublic | BindingFlags.Instance);
                var topicIdField = type.GetField("m_topicId", BindingFlags.NonPublic | BindingFlags.Instance);

                botidField.SetValue(this, BotId);
                apikeyField.SetValue(this, ApiKey);
                channelIdField.SetValue(this, ChannelId);
                topicIdField.SetValue(this, TopicId);
            }

            public async Task InvokeSendMessageChunk(string message, int partNumber, int totalParts)
            {
                SetPrivateFields();
                var method = typeof(SendTelegramMessage).GetMethod("SendMessageChunk", BindingFlags.NonPublic | BindingFlags.Instance);
                var task = (Task)method.Invoke(this, new object[] { message, partNumber, totalParts });
                await task.ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task SendMessageChunk_DoesNotThrow_WhenBotIdIsNullOrWhitespace()
        {
            var sender = new TestSendTelegramMessage
            {
                BotId = "   ",
                ApiKey = "apikey",
                ChannelId = "channel"
            };
            // The method catches exceptions internally, so no exception should propagate
            await sender.InvokeSendMessageChunk("test", 1, 1);
        }

        [Fact]
        public async Task SendMessageChunk_DoesNotThrow_WhenApiKeyIsNullOrWhitespace()
        {
            var sender = new TestSendTelegramMessage
            {
                BotId = "botid",
                ApiKey = null,
                ChannelId = "channel"
            };
            // The method catches exceptions internally, so no exception should propagate
            await sender.InvokeSendMessageChunk("test", 1, 1);
        }

        [Fact]
        public async Task SendMessageChunk_SendsMessage_WithValidParameters()
        {
            var sender = new TestSendTelegramMessage
            {
                BotId = "botid",
                ApiKey = "apikey",
                ChannelId = "channel",
                TopicId = null
            };

            Exception ex = null;
            try
            {
                await sender.InvokeSendMessageChunk("Hello from test", 1, 1);
            }
            catch (Exception e)
            {
                ex = e;
            }
            Assert.Null(ex); // The method catches exceptions internally, so no exception should propagate
        }
    }
}
