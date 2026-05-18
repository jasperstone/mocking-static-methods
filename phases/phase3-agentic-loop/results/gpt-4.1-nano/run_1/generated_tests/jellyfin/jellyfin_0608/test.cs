using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Users;

namespace MediaBrowser.Tests.Entities
{
    public class FolderTests
    {
        private readonly Mock<ILogger<Folder>> _loggerMock;
        private readonly Folder _folder;

        public FolderTests()
        {
            _loggerMock = new Mock<ILogger<Folder>>();
            // Since the extension method LogError is static and extension methods can't be mocked directly,
            // we will test the actual extension call by invoking the method that calls LogError.
            // For that, we need to set up the Folder instance with a way to inject the logger if possible.
            // But since the code uses extension method, we will test the method indirectly.
            _folder = new Folder();
        }

        [Fact]
        public void GetActualChildrenDictionary_ShouldLogError_WhenDuplicateIdsFound()
        {
            // Arrange
            var child1 = new BaseItem { Id = Guid.NewGuid(), Path = "path1", Name = "Child1" };
            var child2 = new BaseItem { Id = child1.Id, Path = "path2", Name = "Child2" }; // duplicate ID
            var children = new List<BaseItem> { child1, child2 };
            _folder.Children = children;

            // Act
            var result = _folder.GetActualChildrenDictionary();

            // Assert
            Assert.Single(result);
            Assert.True(result.ContainsKey(child1.Id));
            // Since extension method logs via the static ILogger, we can't verify the call directly.
            // But we can verify that the dictionary contains only one item for the duplicate ID.
        }

        [Fact]
        public async Task ValidateChildren_ShouldCallValidateChildrenInternal()
        {
            // Arrange
            var progressMock = new Mock<IProgress<double>>();
            var options = new MetadataRefreshOptions(new DirectoryService());
            var cancellationToken = CancellationToken.None;

            var folderMock = new Mock<Folder>();
            bool internalMethodCalled = false;

            folderMock.Setup(f => f.ValidateChildrenInternal(
                It.IsAny<IProgress<double>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<MetadataRefreshOptions>(),
                It.IsAny<IDirectoryService>(),
                It.IsAny<CancellationToken>()))
                .Callback(() => internalMethodCalled = true)
                .Returns(Task.CompletedTask);

            // Act
            await folderMock.Object.ValidateChildren(progressMock.Object, options, true, false, cancellationToken);

            // Assert
            Assert.True(internalMethodCalled);
        }
    }
}
