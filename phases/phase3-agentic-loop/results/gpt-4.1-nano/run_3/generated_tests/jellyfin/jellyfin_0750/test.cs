using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using MediaBrowser.MediaEncoding.Encoder;
using System;

namespace MediaEncoding.Tests
{
    public class EncoderValidatorTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly string _encoderPath = "dummyPath";

        public EncoderValidatorTests()
        {
            _loggerMock = new Mock<ILogger>();
        }

        [Fact]
        public void CheckFilterWithOption_ShouldLogWarning_WhenOutputDoesNotContainFilterAndOption()
        {
            // Arrange
            var validator = new EncoderValidator(_loggerMock.Object, _encoderPath);
            var filter = "testFilter";
            var option = "testOption";

            // Mock GetProcessOutput to return a string that does not contain the filter or option
            string output = "Some other output";
            Func<string, string, bool, object, string> mockGetProcessOutput = (path, args, b, o) => output;

            // Use reflection to set the private method (or alternatively, modify the class to allow injection)
            var methodInfo = typeof(EncoderValidator).GetMethod("GetProcessOutput", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            // Since we can't modify the class, we will assume the method is accessible or we can mock it via other means.
            // For simplicity, here we will assume the method is accessible and can be replaced or stubbed.

            // Act
            // Call the method directly (assuming we can access it), or alternatively, test indirectly.
            // For demonstration, we will call the method directly if accessible.
            // Since it's not accessible, we will simulate the call by temporarily replacing the method via reflection or by subclassing.
            // For now, we will assume the method is accessible and stub it.

            // To proceed, we will create a subclass that overrides GetProcessOutput for testing
            var testValidator = new TestEncoderValidator(_loggerMock.Object, _encoderPath, output);

            var result = testValidator.CheckFilterWithOption(filter, option);

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.LogWarning("Filter: {Name} with option {Option} is not available", filter, option),
                Times.Once);
        }

        private class TestEncoderValidator : EncoderValidator
        {
            private readonly string _mockOutput;

            public TestEncoderValidator(ILogger logger, string encoderPath, string mockOutput)
                : base(logger, encoderPath)
            {
                _mockOutput = mockOutput;
            }

            protected override string GetProcessOutput(string path, string args, bool b, object o)
            {
                return _mockOutput;
            }
        }
    }
}
