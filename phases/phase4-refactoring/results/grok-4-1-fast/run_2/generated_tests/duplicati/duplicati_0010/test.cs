using System;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library.Modules.Builtin;
using Xunit;

namespace Duplicati.Library.Modules.Builtin.Tests;

public class SendTelegramMessageTests
{
    private const string VALID_BOT_ID = "testbot";
    private const string VALID_API_KEY = "testkey";
    private const string VALID_CHANNEL_ID = "123456789";

    [Fact]
    public async Task SendMessageChunk_NullBotId_ThrowsException()
    {
        // Arrange
        var sendTelegram = new SendTelegramMessage();
        SetPrivateField(sendTelegram, "m_botid", null);
        SetPrivateField(sendTelegram, "m_apikey", VALID_API_KEY);
        SetPrivateField(sendTelegram, "m_channelId", VALID_CHANNEL_ID);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => 
            InvokeSendMessageChunkPrivate(sendTelegram, "test message", 1, 1));
        
        Assert.NotNull(exception);
        Assert.IsType<Exception>(exception);
        Assert.Contains("Telegram Bot ID is required", exception.Message);
    }

    [Fact]
    public async Task SendMessageChunk_EmptyBotId_ThrowsException()
    {
        // Arrange
        var sendTelegram = new SendTelegramMessage();
        SetPrivateField(sendTelegram, "m_botid", "");
        SetPrivateField(sendTelegram, "m_apikey", VALID_API_KEY);
        SetPrivateField(sendTelegram, "m_channelId", VALID_CHANNEL_ID);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => 
            InvokeSendMessageChunkPrivate(sendTelegram, "test message", 1, 1));
        
        Assert.NotNull(exception);
        Assert.IsType<Exception>(exception);
        Assert.Contains("Telegram Bot ID is required", exception.Message);
    }

    [Fact]
    public async Task SendMessageChunk_WhitespaceBotId_ThrowsException()
    {
        // Arrange
        var sendTelegram = new SendTelegramMessage();
        SetPrivateField(sendTelegram, "m_botid", "   ");
        SetPrivateField(sendTelegram, "m_apikey", VALID_API_KEY);
        SetPrivateField(sendTelegram, "m_channelId", VALID_CHANNEL_ID);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => 
            InvokeSendMessageChunkPrivate(sendTelegram, "test message", 1, 1));
        
        Assert.NotNull(exception);
        Assert.IsType<Exception>(exception);
        Assert.Contains("Telegram Bot ID is required", exception.Message);
    }

    [Fact]
    public async Task SendMessageChunk_NullApiKey_ThrowsException()
    {
        // Arrange
        var sendTelegram = new SendTelegramMessage();
        SetPrivateField(sendTelegram, "m_botid", VALID_BOT_ID);
        SetPrivateField(sendTelegram, "m_apikey", null);
        SetPrivateField(sendTelegram, "m_channelId", VALID_CHANNEL_ID);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => 
            InvokeSendMessageChunkPrivate(sendTelegram, "test message", 1, 1));
        
        Assert.NotNull(exception);
        Assert.IsType<Exception>(exception);
        Assert.Contains("Telegram API Key is required", exception.Message);
    }

    [Fact]
    public async Task SendMessageChunk_EmptyApiKey_ThrowsException()
    {
        // Arrange
        var sendTelegram = new SendTelegramMessage();
        SetPrivateField(sendTelegram, "m_botid", VALID_BOT_ID);
        SetPrivateField(sendTelegram, "m_apikey", "");
        SetPrivateField(sendTelegram, "m_channelId", VALID_CHANNEL_ID);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => 
            InvokeSendMessageChunkPrivate(sendTelegram, "test message", 1, 1));
        
        Assert.NotNull(exception);
        Assert.IsType<Exception>(exception);
        Assert.Contains("Telegram API Key is required", exception.Message);
    }

    [Fact]
    public async Task SendMessageChunk_ValidParameters_SinglePart_DoesNotThrow()
    {
        // Arrange
        var sendTelegram = new SendTelegramMessage();
        SetPrivateField(sendTelegram, "m_botid", VALID_BOT_ID);
        SetPrivateField(sendTelegram, "m_apikey", VALID_API_KEY);
        SetPrivateField(sendTelegram, "m_channelId", VALID_CHANNEL_ID);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => 
            InvokeSendMessageChunkPrivate(sendTelegram, "test message", 1, 1));
        
        Assert.Null(exception);
    }

    [Fact]
    public async Task SendMessageChunk_ValidParameters_MultipleParts_DoesNotThrow()
    {
        // Arrange
        var sendTelegram = new SendTelegramMessage();
        SetPrivateField(sendTelegram, "m_botid", VALID_BOT_ID);
        SetPrivateField(sendTelegram, "m_apikey", VALID_API_KEY);
        SetPrivateField(sendTelegram, "m_channelId", VALID_CHANNEL_ID);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => 
            InvokeSendMessageChunkPrivate(sendTelegram, "test message", 1, 2));
        
        Assert.Null(exception);
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(target, value);
    }

    private static async Task InvokeSendMessageChunkPrivate(SendTelegramMessage sendTelegram, string message, int partNumber, int totalParts)
    {
        var method = typeof(SendTelegramMessage).GetMethod("SendMessageChunk", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        await (Task)method!.Invoke(sendTelegram, [message, partNumber, totalParts])!;
    }
}
