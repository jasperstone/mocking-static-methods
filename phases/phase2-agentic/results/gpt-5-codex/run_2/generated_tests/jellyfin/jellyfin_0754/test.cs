using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MediaBrowser.MediaEncoding.Encoder;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Tests.MediaEncoding
{
    public class EncoderValidatorTests
    {
        [Fact]
        public void GetCodecs_LogsErrorWhenProcessOutputThrows()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var encoderPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "ffmpeg");
            var validator = new EncoderValidator(loggerMock.Object, encoderPath);

            var validatorType = typeof(EncoderValidator);
            var codecType = validatorType.GetNestedType("Codec", BindingFlags.NonPublic)!;
            var getCodecsMethod = validatorType.GetMethod("GetCodecs", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var encoderValue = Enum.Parse(codecType, "Encoder");

            // Act
            var resultObject = getCodecsMethod.Invoke(validator, new object?[] { encoderValue });
            var result = Assert.IsAssignableFrom<IEnumerable<string>>(resultObject);

            // Assert
            Assert.Empty(result);

            var errorInvocation = loggerMock.Invocations.Single(invocation =>
                invocation.Method.Name == nameof(ILogger.Log)
                && invocation.Arguments[0] is LogLevel level
                && level == LogLevel.Error);

            var state = Assert.IsAssignableFrom<IEnumerable<KeyValuePair<string, object?>>>(errorInvocation.Arguments[2]);
            var exception = Assert.IsAssignableFrom<Exception>(errorInvocation.Arguments[3]);
            Assert.NotNull(exception);

            var stateList = state.ToList();
            Assert.Contains(stateList, kvp => kvp.Key == "{OriginalFormat}" && (string?)kvp.Value == "Error detecting available {Codec}");
            Assert.Contains(stateList, kvp => kvp.Key == "Codec" && (string?)kvp.Value == "encoders");

            var formatter = Assert.IsAssignableFrom<Delegate>(errorInvocation.Arguments[4]);
            var formattedMessage = (string)formatter.DynamicInvoke(errorInvocation.Arguments[2], errorInvocation.Arguments[3])!;
            Assert.Equal("Error detecting available encoders", formattedMessage);
        }
    }
}
