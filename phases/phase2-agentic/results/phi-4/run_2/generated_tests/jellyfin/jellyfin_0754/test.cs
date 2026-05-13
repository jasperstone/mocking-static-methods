using System;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.MediaEncoding.Encoder;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class EncoderValidatorTests
{
    [Fact]
    public void GetCodecs_LogsError_WhenExceptionIsThrown()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var encoderValidator = new EncoderValidator(loggerMock.Object, "dummyPath");

        // Act
        var result = encoderValidator.GetCodecs(Codec.Encoder);

        // Assert
        loggerMock.Verify(
            x => x.LogError(
                It.IsAny<Exception>(),
                "Error detecting available {Codec}",
                It.Is<string>(s => s == "encoders")
            ),
            Times.Once
        );

        Assert.Empty(result);
    }
}
