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

        private class TestEncoderValidator : EncoderValidator
        {
            private readonly string _testOutput;

            public TestEncoderValidator(ILogger logger, string encoderPath, string testOutput) : base(logger, encoderPath)
            {
                _testOutput = testOutput;
            }

            protected override string GetProcessOutput(string encoderPath, string args, bool b, object o)
            {
                return _testOutput;
            }
        }

        [Fact]
        public void CheckFilterWithOption_ShouldLogWarning_WhenFilterAndOptionNotFound()
        {
            // Arrange
            var testOutput = "Some output without the filter or option";
            var validator = new TestEncoderValidator(_loggerMock.Object, _encoderPath, testOutput);
            string filter = "nonexistent_filter";
            string option = "nonexistent_option";

            // Act
            var result = validator.CheckFilterWithOption(filter, option);

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning("Filter: {Name} with option {Option} is not available", filter, option),
                Times.Once);
            Assert.False(result);
        }
    }
}
