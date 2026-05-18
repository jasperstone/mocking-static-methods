using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;

public class TestBaseItem : BaseItem
{
    public TestBaseItem()
    {
        Logger = Mock.Of<ILogger>();
        LibraryManager = Mock.Of<ILibraryManager>();
    }

    public async Task<BaseItem> FindLinkedChildAsync(LinkedChild info)
    {
        if (info.ItemId.HasValue && !info.ItemId.Value.Equals(Guid.Empty))
        {
            var item = LibraryManager.GetItemById(info.ItemId.Value);
            if (item is not null)
            {
                return item;
            }

            Logger.LogWarning("Unable to find linked item by ItemId {0}", info.ItemId);
        }

        var path = info.Path;
        if (!string.IsNullOrEmpty(path))
        {
            path = FileSystem.MakeAbsolutePath(ContainingFolderPath, path);

            var itemByPath = await LibraryManager.FindByPathAsync(path, CancellationToken.None);

            if (itemByPath is null)
            {
                Logger.LogWarning("Unable to find linked item at path {0}", info.Path);
            }

            return itemByPath;
        }

        if (!string.IsNullOrEmpty(info.LibraryItemId))
        {
            var item = LibraryManager.GetItemById(info.LibraryItemId);

            if (item is null)
            {
                Logger.LogWarning("Unable to find linked item by LibraryItemId {0}", info.LibraryItemId);
            }

            return item;
        }

        return null;
    }
}

public class BaseItemTests
{
    [Fact]
    public async Task FindLinkedChild_LogsWarning_WhenItemIdNotFound()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var libraryManagerMock = new Mock<ILibraryManager>();
        var baseItem = new TestBaseItem
        {
            Logger = loggerMock.Object,
            LibraryManager = libraryManagerMock.Object
        };

        var linkedChild = new LinkedChild
        {
            ItemId = Guid.NewGuid(),
            Path = null,
            LibraryItemId = null
        };

        libraryManagerMock.Setup(m => m.GetItemById(linkedChild.ItemId.Value)).Returns((BaseItem)null);

        // Act
        await baseItem.FindLinkedChildAsync(linkedChild);

        // Assert
        loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains("Unable to find linked item by ItemId")), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task FindLinkedChild_LogsWarning_WhenPathNotFound()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var libraryManagerMock = new Mock<ILibraryManager>();
        var baseItem = new TestBaseItem
        {
            Logger = loggerMock.Object,
            LibraryManager = libraryManagerMock.Object
        };

        var linkedChild = new LinkedChild
        {
            ItemId = null,
            Path = "some/path",
            LibraryItemId = null
        };

        libraryManagerMock.Setup(m => m.FindByPathAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((BaseItem)null);

        // Act
        await baseItem.FindLinkedChildAsync(linkedChild);

        // Assert
        loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains("Unable to find linked item at path")), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task FindLinkedChild_LogsWarning_WhenLibraryItemIdNotFound()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var libraryManagerMock = new Mock<ILibraryManager>();
        var baseItem = new TestBaseItem
        {
            Logger = loggerMock.Object,
            LibraryManager = libraryManagerMock.Object
        };

        var linkedChild = new LinkedChild
        {
            ItemId = null,
            Path = null,
            LibraryItemId = "some-id"
        };

        libraryManagerMock.Setup(m => m.GetItemById(linkedChild.LibraryItemId)).Returns((BaseItem)null);

        // Act
        await baseItem.FindLinkedChildAsync(linkedChild);

        // Assert
        loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains("Unable to find linked item by LibraryItemId")), It.IsAny<object[]>()), Times.Once);
    }
}
