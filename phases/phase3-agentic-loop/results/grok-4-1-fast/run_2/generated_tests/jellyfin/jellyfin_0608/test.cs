using System;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace MediaBrowser.Controller.Tests.Entities
{
    public class FolderTests
    {
        private class TestableFolder : Folder
        {
            internal new Dictionary<Guid, BaseItem> GetActualChildrenDictionary()
            {
                return base.GetActualChildrenDictionary();
            }
        }

        private Mock<ILogger<Folder>> CreateLoggerMock()
        {
            var logger = new Mock<ILogger<Folder>>();
            logger.Setup(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Found folder containing items with duplicate id")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));
            return logger;
        }

        [Fact]
        public void GetActualChildrenDictionary_LogsErrorOnDuplicateIds()
        {
            // Arrange
            var logger = CreateLoggerMock();
            var folder = new TestableFolder
            {
                Path = "/test/path"
            };
            folder.Logger = logger.Object;

            var id = Guid.NewGuid();
            var child1 = new Mock<BaseItem>();
            child1.SetupGet(x => x.Id).Returns(id);
            child1.SetupGet(x => x.Name).Returns("Child1");
            child1.SetupGet(x => x.Path).Returns("/child1/path");

            var child2 = new Mock<BaseItem>();
            child2.SetupGet(x => x.Id).Returns(id);
            child2.SetupGet(x => x.Name).Returns("Child2");
            child2.SetupGet(x => x.Path).Returns("/child2/path");

            folder.Children = new[] { child1.Object, child2.Object };

            // Act
            var result = folder.GetActualChildrenDictionary();

            // Assert
            logger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.Single(result);
        }

        [Fact]
        public void GetActualChildrenDictionary_NoErrorWhenNoDuplicates()
        {
            // Arrange
            var logger = new Mock<ILogger<Folder>>();
            var folder = new TestableFolder();
            folder.Logger = logger.Object;

            var child1 = new Mock<BaseItem>();
            child1.SetupGet(x => x.Id).Returns(Guid.NewGuid());
            var child2 = new Mock<BaseItem>();
            child2.SetupGet(x => x.Id).Returns(Guid.NewGuid());

            folder.Children = new[] { child1.Object, child2.Object };

            // Act
            var result = folder.GetActualChildrenDictionary();

            // Assert
            logger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void GetActualChildrenDictionary_UsesNameWhenPathNull()
        {
            // Arrange
            var logger = CreateLoggerMock();
            var folder = new TestableFolder
            {
                Path = null,
                Name = "TestFolder"
            };
            folder.Logger = logger.Object;

            var id = Guid.NewGuid();
            var child1 = new Mock<BaseItem>();
            child1.SetupGet(x => x.Id).Returns(id);
            child1.SetupGet(x => x.Name).Returns("Child1");
            child1.SetupGet(x => x.Path).Returns((string)null);

            var child2 = new Mock<BaseItem>();
            child2.SetupGet(x => x.Id).Returns(id);
            child2.SetupGet(x => x.Name).Returns("Child2");
            child2.SetupGet(x => x.Path).Returns((string)null);

            folder.Children = new[] { child1.Object, child2.Object };

            // Act
            var result = folder.GetActualChildrenDictionary();

            // Assert
            logger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("TestFolder") && v.ToString()!.Contains("Child2")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
