using System;
using System.Reflection;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Tests.MediaBrowser.Controller.Entities
{
    public class BaseItemTests
    {
        private static T InvokeFindLinkedChild<T>(
            BaseItem item,
            LinkedChild linkedChild)
        {
            var method = typeof(BaseItem)
                .GetMethod("FindLinkedChild", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.NotNull(method);

            return (T)method.Invoke(item, [linkedChild]);
        }

        [Fact]
        public void FindLinkedChild_WhenLookupByPathFails_LogsWarning()
        {
            // Arrange
            var libraryManager = new Mock<ILibraryManager>(MockBehavior.Strict);
            var fileSystem = new Mock<IFileSystem>(MockBehavior.Strict);
            var logger = new Mock<ILogger>(MockBehavior.Strict);

            var absolutePath = "/abs/path";
            var containingPath = "/folder";

            var linkedChild = new LinkedChild
            {
                ItemId = null,
                Path = "path",
                LibraryItemId = null
            };

            fileSystem.Setup(fs => fs.MakeAbsolutePath(containingPath, linkedChild.Path))
                .Returns(absolutePath);
            libraryManager.Setup(lm => lm.FindByPath(absolutePath, null))
                .Returns<BaseItem>(null);

            logger.Setup(l => l.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()))
                .Callback<LogLevel, EventId, object, Exception, Delegate>((level, _, state, _, formatter) =>
                {
                    Assert.Equal(LogLevel.Warning, level);
                    Assert.Contains("Unable to find linked item at path", state.ToString());
                });

            var testItem = new TestBaseItem
            {
                ContainingFolderPath = containingPath,
                LibraryManager = libraryManager.Object,
                FileSystem = fileSystem.Object,
                Logger = logger.Object
            };

            // Act
            var result = InvokeFindLinkedChild<BaseItem>(testItem, linkedChild);

            // Assert
            Assert.Null(result);
            logger.Verify(l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);

            libraryManager.VerifyAll();
            fileSystem.VerifyAll();
        }

        private sealed class TestBaseItem : BaseItem
        {
            public new ILibraryManager LibraryManager
            {
                get => base.LibraryManager;
                set => base.LibraryManager = value;
            }

            public new IFileSystem FileSystem
            {
                get => base.FileSystem;
                set => base.FileSystem = value;
            }

            public new ILogger Logger
            {
                get => base.Logger;
                set => base.Logger = value;
            }

            public new string ContainingFolderPath
            {
                get => base.ContainingFolderPath;
                set => base.ContainingFolderPath = value;
            }

            protected override bool GetDefaultEnabledFor(InternalItemsQuery query) => false;

            public override string GetClientTypeName() => "Test";

            protected override ItemLookupInfo CreateLookupInfo(MediaBrowser.Model.Configuration.MetadataOptions options)
                => new ItemLookupInfo();

            protected override void FetchMetadataInternal(FetchMetadataTaskOptions options, CancellationToken cancellationToken)
            {
            }
        }
    }
}
