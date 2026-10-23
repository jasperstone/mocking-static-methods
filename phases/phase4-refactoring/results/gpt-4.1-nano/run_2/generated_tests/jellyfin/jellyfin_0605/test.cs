using System;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;

namespace MediaBrowser.Tests
{
    public class BaseItemTests
    {
        private class TestItem : BaseItem
        {
            public ILogger Logger { get; }

            public TestItem(ILogger logger)
            {
                Logger = logger;
            }

            // Expose the method for testing
            public BaseItem FindLinkedChildWrapper(LinkedChild info)
            {
                // Call the protected method
                return FindLinkedChild(info);
            }
        }

        [Fact]
        public void FindLinkedChild_ShouldLogWarning_WhenItemNotFoundByPath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var item = new TestItem(loggerMock.Object);
            var linkedChild = new LinkedChild
            {
                ItemId = null,
                Path = "nonexistent/path",
                LibraryItemId = null
            };

            // Act
            var result = item.FindLinkedChildWrapper(linkedChild);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning("Unable to find linked item at path {0}", linkedChild.Path),
                Times.Once);
        }
    }
}
