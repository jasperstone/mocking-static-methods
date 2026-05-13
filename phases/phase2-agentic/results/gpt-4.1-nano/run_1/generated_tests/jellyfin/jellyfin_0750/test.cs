using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.MediaEncoding.Encoder;
using System;

namespace MediaBrowser.Tests
{
    public class EncoderValidatorTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly EncoderValidator _validator;

        public EncoderValidatorTests()
        {
            _loggerMock = new Mock<ILogger>();
            _validator = new EncoderValidator(_loggerMock.Object, "dummyPath");
        }

        [Fact]
        public void CheckFilterWithOption_ShouldLogWarning_WhenOutputDoesNotContainFilterAndOption()
        {
            // Arrange
            string filter = "testFilter";
            string option = "testOption";

            // Simulate GetProcessOutput returning output that does not contain the filter or option
            // We need to mock GetProcessOutput method, but it's not public, so we assume it's virtual or accessible for testing
            // For this example, let's assume we can set up a derived class for testing
            var testValidator = new TestEncoderValidator(_loggerMock.Object, "dummyPath");
            testValidator.SetProcessOutput("Some unrelated output");

            // Act
            var result = testValidator.CheckFilterWithOption(filter, option);

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.LogWarning("Filter: {Name} with option {Option} is not available", filter, option),
                Times.Once);
        }

        [Fact]
        public void CheckBitStreamFilterWithOption_ShouldLogWarning_WhenOutputDoesNotContainFilterAndOption()
        {
            // Arrange
            string filter = "testBsf";
            string option = "testOption";

            var testValidator = new TestEncoderValidator(_loggerMock.Object, "dummyPath");
            testValidator.SetProcessOutput("Some unrelated output");

            // Act
            var result = testValidator.CheckBitStreamFilterWithOption(filter, option);

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.LogWarning("Bit stream filter: {Name} with option {Option} is not available", filter, option),
                Times.Once);
        }

        // Helper class to override GetProcessOutput for testing
        private class TestEncoderValidator : EncoderValidator
        {
            private string _output;

            public TestEncoderValidator(ILogger logger, string encoderPath) : base(logger, encoderPath)
            {
            }

            public void SetProcessOutput(string output)
            {
                _output = output;
            }

            protected override string GetProcessOutput(string path, string args, bool b, object o)
            {
                return _output;
            }
        }
    }
}
