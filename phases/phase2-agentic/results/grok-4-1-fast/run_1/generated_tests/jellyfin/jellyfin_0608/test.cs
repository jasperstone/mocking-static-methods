using System;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class FolderTests
    {
        [Fact]
        public void GetActualChildrenDictionary_LogsError_OnDuplicateId()
        {
            // Arrange
            var logger = new Mock<ILogger<Folder>>();
            var folder = new TestFolder
            {
                Path = "/test/path",
                Logger = logger.Object
            };

            var child1 = new BaseItem { Id = Guid.NewGuid() };
            var child2 = new BaseItem { Id = child1.Id }; // Duplicate ID

            folder.SetChildren(new[] { child1, child2 });

            // Act
            folder.GetActualChildrenDictionary();

            // Assert
            logger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>((v, t) => t == "Found folder containing items with duplicate id. Path: {Path}, Child Name: {ChildName}"),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void GetActualChildrenDictionary_NoError_WhenNoDuplicates()
        {
            // Arrange
            var logger = new Mock<ILogger<Folder>>();
            var folder = new TestFolder
            {
                Path = "/test/path",
                Logger = logger.Object
            };

            var child1 = new BaseItem { Id = Guid.NewGuid() };
            var child2 = new BaseItem { Id = Guid.NewGuid() }; // Unique ID

            folder.SetChildren(new[] { child1, child2 });

            // Act
            folder.GetActualChildrenDictionary();

            // Assert
            logger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyFormat<string>>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public void GetActualChildrenDictionary_UsesPathFallback_WhenPathNull()
        {
            // Arrange
            var logger = new Mock<ILogger<Folder>>();
            var folder = new TestFolder
            {
                Path = null,
                Name = "TestFolder",
                Logger = logger.Object
            };

            var child1 = new BaseItem 
            { 
                Id = Guid.NewGuid(),
                Path = null,
                Name = "TestChild"
            };
            var child2 = new BaseItem { Id = child1.Id }; // Duplicate ID

            folder.SetChildren(new[] { child1, child2 });

            // Act
            folder.GetActualChildrenDictionary();

            // Assert
            logger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>((v, t) => t == "Found folder containing items with duplicate id. Path: {Path}, Child Name: {ChildName}"),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
                Times.Once);

            // Verify the message contains the fallback Name
            logger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>((v, t) => 
                        t.Contains("TestFolder") && t.Contains("TestChild")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
                Times.Once);
        }

        private class TestFolder : Folder
        {
            public void SetChildren(IEnumerable<BaseItem> children)
            {
                Children = children;
            }
        }
    }
}
