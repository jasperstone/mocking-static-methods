using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using MediaBrowser.Controller.Entities;
using System;

namespace MediaBrowser.Tests
{
    public class BaseItemLoggerExtensionsTests
    {
        [Fact]
        public void LogWarning_Should_CallLogger_WithExpectedMessage_When_LoggingWarning()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var linkedChild = new LinkedChild
            {
                Path = "somepath",
                ItemId = Guid.NewGuid()
            };
            var baseItem = new TestBaseItem();
            baseItem.Logger = mockLogger.Object;

            // Act
            baseItem.FindLinkedChild(linkedChild);

            // Assert
            mockLogger.Verify(
                x => x.LogWarning("Unable to find linked item at path {0}", linkedChild.Path),
                Times.Once);
        }
    }

    // A minimal concrete implementation for testing
    public class TestBaseItem : BaseItem
    {
        public ILogger Logger { get; set; }

        // Override the Logger property to return our mock
        public override ILogger LoggerInstance => Logger ?? base.LoggerInstance;
    }
}
