using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.MediaEncoding.Encoder;
using System;
using System.Collections.Generic;
using System.Reflection;

public class EncoderValidatorTests
{
    [Fact]
    public void GetCodecs_LogsError_WhenExceptionIsThrown()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<EncoderValidator>>();
        var encoderValidator = new EncoderValidator(loggerMock.Object, "fakeEncoderPath");

        // Act
        var method = typeof(EncoderValidator).GetMethod("GetCodecs", BindingFlags.NonPublic | BindingFlags.Instance);
        var codecEnum = typeof(EncoderValidator).GetNestedType("Codec", BindingFlags.NonPublic);
        var codecValue = Enum.Parse(codecEnum, "Encoder");

        var result = method.Invoke(encoderValidator, new object[] { codecValue });

        // Assert
        loggerMock.Verify(
            x => x.LogError(
                It.IsAny<EventId>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<Exception, string>>(),
                It.IsAny<object[]>()),
            Times.Once);
    }
}
