using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Folders;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Users;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Tests.Entities
{
    public class FolderTests
    {
        [Fact]
        public void GetActualChildrenDictionary_ShouldLogErrorOnDuplicateIds()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<Folder>>();
            var folder = new Folder();

            // Inject the logger extension method context if needed
            // Since the extension method is static, we need to ensure it uses the mock logger.
            // Assuming the extension method uses a static logger, we might need to set it up.
            // For this example, let's assume the extension method uses a static logger we can mock.

            // Create children with duplicate IDs
            var child1 = new BaseItem { Id = Guid.NewGuid(), Path = "path1", Name = "Child1" };
            var child2 = new BaseItem { Id = child1.Id, Path = "path2", Name = "Child2" }; // duplicate ID

            folder.Children = new List<BaseItem> { child1, child2 };

            // Act
            var dict = folder.GetActualChildrenDictionary();

            // Assert
            // Verify that LogError was called once
            mockLogger.Verify(
                x => x.LogError(
                    It.IsAny<string>(),
                    It.IsAny<object>(), 
                    It.IsAny<object>()),
                Times.Once);
        }
    }
}
