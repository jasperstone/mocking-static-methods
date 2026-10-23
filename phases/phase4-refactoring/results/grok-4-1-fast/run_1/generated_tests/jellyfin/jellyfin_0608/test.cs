using System;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class TestableFolder : Folder
    {
        public new Dictionary<Guid, BaseItem> GetActualChildrenDictionary()
        {
            return base.GetActualChildrenDictionary();
        }
    }

    public class FolderTests
    {
        private readonly Mock<ILogger<Folder>> _loggerMock;

        public FolderTests()
        {
            _loggerMock = new Mock<ILogger<Folder>>();
        }

        [Fact]
        public void GetActualChildrenDictionary_LogsError_OnDuplicateIds()
        {
            // Arrange
            var folder = new TestableFolder
            {
                Path = "/test/path",
                Name = "TestFolder"
            };
            BaseItem.Logger = _loggerMock.Object;

            var child1 = new Mock<BaseItem>();
            child1.Setup(c => c.Id).Returns(Guid.NewGuid());
            child1.Setup(c => c.Path).Returns("child1.path");
            child1.Setup(c => c.Name).Returns("Child1");

            var child2 = new Mock<BaseItem>();
            child2.Setup(c => c.Id).Returns(child1.Object.Id);
            child2.Setup(c => c.Path).Returns("child2.path");
            child2.Setup(c => c.Name).Returns("Child2");

            folder.Children = new[] { child1.Object, child2.Object };

            // Act
            folder.GetActualChildrenDictionary();

            // Assert
            _loggerMock.Verify(
                l => l.LogError(
                    "Found folder containing items with duplicate id. Path: {Path}, Child Name: {ChildName}",
                    "/test/path",
                    "child2.path"),
                Times.Once);
        }

        [Fact]
        public void GetActualChildrenDictionary_NoError_WhenNoDuplicates()
        {
            // Arrange
            var folder = new TestableFolder
            {
                Path = "/test/path",
                Name = "TestFolder"
            };
            BaseItem.Logger = _loggerMock.Object;

            var child1 = new Mock<BaseItem>();
            child1.Setup(c => c.Id).Returns(Guid.NewGuid());
            var child2 = new Mock<BaseItem>();
            child2.Setup(c => c.Id).Returns(Guid.NewGuid());

            folder.Children = new[] { child1.Object, child2.Object };

            // Act
            folder.GetActualChildrenDictionary();

            // Assert
            _loggerMock.Verify(
                l => l.LogError(
                    It.IsAny<string>(),
                    It.IsAny<object[]>(),
                    It.IsAny<Exception>()),
                Times.Never);
        }

        [Fact]
        public void GetActualChildrenDictionary_UsesChildName_WhenChildPathNull()
        {
            // Arrange
            var folder = new TestableFolder
            {
                Path = "/test/path",
                Name = "TestFolder"
            };
            BaseItem.Logger = _loggerMock.Object;

            var child1 = new Mock<BaseItem>();
            child1.Setup(c => c.Id).Returns(Guid.NewGuid());
            child1.Setup(c => c.Path).Returns((string)null);
            child1.Setup(c => c.Name).Returns("Child1");

            var child2 = new Mock<BaseItem>();
            child2.Setup(c => c.Id).Returns(child1.Object.Id);
            child2.Setup(c => c.Path).Returns((string)null);
            child2.Setup(c => c.Name).Returns("Child2");

            folder.Children = new[] { child1.Object, child2.Object };

            // Act
            folder.GetActualChildrenDictionary();

            // Assert
            _loggerMock.Verify(
                l => l.LogError(
                    "Found folder containing items with duplicate id. Path: {Path}, Child Name: {ChildName}",
                    "/test/path",
                    "Child2"),
                Times.Once);
        }

        [Fact]
        public void GetActualChildrenDictionary_UsesFolderName_WhenFolderPathNull()
        {
            // Arrange
            var folder = new TestableFolder
            {
                Path = null,
                Name = "TestFolder"
            };
            BaseItem.Logger = _loggerMock.Object;

            var child1 = new Mock<BaseItem>();
            child1.Setup(c => c.Id).Returns(Guid.NewGuid());
            var child2 = new Mock<BaseItem>();
            child2.Setup(c => c.Id).Returns(child1.Object.Id);
            child2.Setup(c => c.Path).Returns("child2.path");
            child2.Setup(c => c.Name).Returns("Child2");

            folder.Children = new[] { child1.Object, child2.Object };

            // Act
            folder.GetActualChildrenDictionary();

            // Assert
            _loggerMock.Verify(
                l => l.LogError(
                    "Found folder containing items with duplicate id. Path: {Path}, Child Name: {ChildName}",
                    "TestFolder",
                    "child2.path"),
                Times.Once);
        }
    }
}
