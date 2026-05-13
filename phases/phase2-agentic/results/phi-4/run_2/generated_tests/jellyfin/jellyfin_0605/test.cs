using System;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.IO;

namespace MediaBrowser.Tests.Controller.Entities
{
    public class BaseItemTests
    {
        [Fact]
        public void FindLinkedChild_LogsWarning_WhenItemNotFoundByPath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var baseItem = new BaseItem
            {
                Logger = loggerMock.Object,
                LibraryManager = libraryManagerMock.Object
            };

            var linkedChild = new LinkedChild
            {
                Path = "/some/path"
            };

            libraryManagerMock
                .Setup(m => m.FindByPath(It.IsAny<string>(), null))
                .Returns((BaseItem)null);

            // Act
            baseItem.FindLinkedChild(linkedChild);

            // Assert
            loggerMock.Verify(
                l => l.LogWarning("Unable to find linked item at path {0}", linkedChild.Path),
                Times.Once);
        }
    }
}
