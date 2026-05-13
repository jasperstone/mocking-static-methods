using System;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Xunit;

public class BaseItemTests
{
    [Fact]
    public void FindLinkedChild_LogsWarning_WhenItemNotFoundByPath()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<BaseItem>>();
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
        var result = baseItem.FindLinkedChild(linkedChild);

        // Assert
        Assert.Null(result);
        loggerMock.Verify(
            l => l.LogWarning(It.Is<string>(s => s.Contains("Unable to find linked item at path {0}")), linkedChild.Path),
            Times.Once);
    }
}
