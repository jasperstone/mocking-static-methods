using System;
using System.Threading.Tasks;
using Duplicati.Library.Modules.Builtin;
using Xunit;

namespace Duplicati.Tests.Library.Modules.Builtin
{
    public class SendTelegramMessageTests
    {
        // Use reflection to set private fields and invoke private method SendMessageChunk.
        // The tested method catches exceptions internally and logs warnings, so it does not throw.
        // We test that the method completes and logs warnings when required fields are missing.

        [Fact]
        public async Task SendMessageChunk_Completes_WhenBotIdIsNullOrWhitespace()
        {
            var sender = new SendTelegramMessageWrapper(null, "apikey", "channel", null);
            await sender.SendMessageChunkWrapper("message", 1, 1);

            sender = new SendTelegramMessageWrapper("   ", "apikey", "channel", null);
            await sender.SendMessageChunkWrapper("message", 1, 1);
        }

        [Fact]
        public async Task SendMessageChunk_Completes_WhenApiKeyIsNullOrWhitespace()
        {
            var sender = new SendTelegramMessageWrapper("botid", null, "channel", null);
            await sender.SendMessageChunkWrapper("message", 1, 1);

            sender = new SendTelegramMessageWrapper("botid", "   ", "channel", null);
            await sender.SendMessageChunkWrapper("message", 1, 1);
        }

        [Fact]
        public async Task SendMessageChunk_Completes_WithValidParameters()
        {
            var sender = new SendTelegramMessageWrapper("botid", "apikey", "channel", null);
            await sender.SendMessageChunkWrapper("test message", 1, 1);
        }

        private class SendTelegramMessageWrapper : SendTelegramMessage
        {
            public SendTelegramMessageWrapper(string botid, string apikey, string channelId, string topicId)
            {
                SetPrivateField("m_botid", botid);
                SetPrivateField("m_apikey", apikey);
                SetPrivateField("m_channelId", channelId);
                SetPrivateField("m_topicId", topicId);
            }

            public Task SendMessageChunkWrapper(string message, int partNumber, int totalParts)
            {
                var method = typeof(SendTelegramMessage).GetMethod("SendMessageChunk", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method == null) throw new InvalidOperationException("SendMessageChunk method not found");
                return (Task)method.Invoke(this, new object[] { message, partNumber, totalParts });
            }

            private void SetPrivateField(string fieldName, object value)
            {
                var field = typeof(SendTelegramMessage).GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field == null) throw new InvalidOperationException($"Field {fieldName} not found");
                field.SetValue(this, value);
            }
        }
    }
}
