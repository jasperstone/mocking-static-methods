using Xunit;
using Moq;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;

namespace MediaBrowser.Controller.Tests.Entities
{
    public class BaseItemTests
    {
        [Fact]
        public void FindLinkedChild_LogsWarning_WhenItemNotFoundByPath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BaseItem>>();
            var libraryManagerMock = new Mock<ILibraryManager>();

            var baseItem = new Mock<BaseItemSubclass>();
            baseItem.Setup(b => b.Logger).Returns(loggerMock.Object);
            baseItem.Setup(b => b.LibraryManager).Returns(libraryManagerMock.Object);

            var linkedChild = new LinkedChild
            {
                Path = "some/path"
            };

            libraryManagerMock.Setup(lm => lm.FindByPath(It.IsAny<string>(), It.IsAny<bool?>())).Returns((BaseItem)null);

            // Act
            var result = baseItem.Object.FindLinkedChild(linkedChild);

            // Assert
            loggerMock.Verify(
                logger => logger.LogWarning("Unable to find linked item at path {0}", linkedChild.Path),
                Times.Once);
        }

        private class BaseItemSubclass : BaseItem
        {
            public new ILogger Logger => base.Logger;
            public new ILibraryManager LibraryManager => base.LibraryManager;
            public new BaseItem FindLinkedChild(LinkedChild info) => base.FindLinkedChild(info);
        }
    }
}
