using System;
using System.Collections.Generic;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.MediaEncoding.Encoder;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class EncoderValidatorTests
{
    private readonly Mock<ILogger<EncoderValidator>> _loggerMock;
    private readonly Mock<ILoggerFactory> _loggerFactoryMock;
    private readonly string _encoderPath;
    private readonly EncoderValidator _validator;

    public EncoderValidatorTests()
    {
        _loggerMock = new Mock<ILogger<EncoderValidator>>();
        _loggerFactoryMock = new Mock<ILoggerFactory>();
        _loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(_loggerMock.Object);
        
        _encoderPath = "/path/to/ffmpeg";
        _validator = new EncoderValidator(_loggerMock.Object, _encoderPath);
    }

    [Fact]
    public void GetCodecs_WhenGetProcessOutputThrowsException_LogsErrorWithCodecParameter()
    {
        // Arrange
        var exception = new InvalidOperationException("Process failed");
        var expectedCodecStr = "encoders"; // for Codec.Encoder
        
        // Act
        var result = _validator.GetCodecs(Codec.Encoder);

        // Assert
        Assert.Empty(result);
        
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ContainsCodecMessage(v, expectedCodecStr)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void GetCodecs_Decoder_WhenGetProcessOutputThrowsException_LogsErrorWithCodecParameter()
    {
        // Arrange
        var exception = new InvalidOperationException("Process failed");
        var expectedCodecStr = "decoders"; // for Codec.Decoder
        
        // Act
        var result = _validator.GetCodecs(Codec.Decoder);

        // Assert
        Assert.Empty(result);
        
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ContainsCodecMessage(v, expectedCodecStr)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void GetFFmpegFilters_WhenGetProcessOutputThrowsException_LogsError()
    {
        // Arrange
        var exception = new InvalidOperationException("Process failed");
        
        // Act
        var result = _validator.GetFFmpegFilters();

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error detecting available filters")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private static bool ContainsCodecMessage(object state, string expectedCodecStr)
    {
        return state.ToString()!.Contains("Error detecting available") 
               && state.ToString()!.Contains(expectedCodecStr);
    }
}
