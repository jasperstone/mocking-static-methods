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

            var baseItem = new BaseItem();
            var loggerField = typeof(BaseItem).GetField("_logger", BindingFlags.NonPublic | BindingFlags.Instance);
            loggerField.SetValue(baseItem, loggerMock.Object);

            var libraryManagerField = typeof(BaseItem).GetField("_libraryManager", BindingFlags.NonPublic | BindingFlags.Instance);
            libraryManagerField.SetValue(baseItem, libraryManagerMock.Object);

            var containingFolderPathField = typeof(BaseItem).GetField("_containingFolderPath", BindingFlags.NonPublic | BindingFlags.Instance);
            containingFolderPathField.SetValue(baseItem, "C:\\TestFolder");

            var linkedChild = new LinkedChild
            {
                Path = "testPath"
            };

            libraryManagerMock.Setup(lm => lm.FindByPath(It.IsAny<string>(), It.IsAny<bool?>())).Returns((BaseItem)null);

            // Act
            var method = typeof(BaseItem).GetMethod("FindLinkedChild", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(baseItem, new object[] { linkedChild });

            // Assert
            loggerMock.Verify(
                logger => logger.LogWarning("Unable to find linked item at path {0}", It.Is<object[]>(o => o[0].ToString() == "testPath")),
                Times.Once);
        }
    }
}
