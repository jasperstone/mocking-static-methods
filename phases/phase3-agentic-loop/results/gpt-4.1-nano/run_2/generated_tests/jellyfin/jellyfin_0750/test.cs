using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.MediaEncoding.Encoder;

namespace MediaBrowser.Tests
{
    public class EncoderValidatorTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly TestEncoderValidator _validator;

        public EncoderValidatorTests()
        {
            _loggerMock = new Mock<ILogger>();
            _validator = new TestEncoderValidator(_loggerMock.Object, "dummyPath");
        }

        [Fact]
        public void CheckFilterWithOption_ShouldLogWarning_WhenOutputDoesNotContainFilterAndOption()
        {
            // Arrange
            string filter = "testFilter";
            string option = "testOption";

            // Override GetProcessOutput to simulate output not containing filter or option
            var testValidator = new TestEncoderValidator(_loggerMock.Object, "dummyPath");
            testValidator.OverrideGetProcessOutput = (path, args, flag, obj) => "Some unrelated output";

            // Act
            var result = testValidator.CheckFilterWithOption(filter, option);

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.LogWarning(
                    It.Is<string>(s => s.Contains("Filter:") || s.Contains("Filter: {Name} with option {Option} is not available")),
                    filter, option),
                Times.Once);
        }

        // Helper class to override GetProcessOutput for testing
        private class TestEncoderValidator : EncoderValidator
        {
            public Func<string, string, bool, object, string> OverrideGetProcessOutput { get; set; }

            public TestEncoderValidator(ILogger logger, string encoderPath) : base(logger, encoderPath)
            {
            }

            protected override string GetProcessOutput(string encoderPath, string arguments, bool something, object nullObject)
            {
                if (OverrideGetProcessOutput != null)
                {
                    return OverrideGetProcessOutput(encoderPath, arguments, something, nullObject);
                }
                return base.GetProcessOutput(encoderPath, arguments, something, nullObject);
            }
        }
    }
}
