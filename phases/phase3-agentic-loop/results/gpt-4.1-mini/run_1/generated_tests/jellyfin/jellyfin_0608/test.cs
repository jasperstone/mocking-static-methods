using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.Entities;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class FolderTests
    {
        [Fact]
        public void GetActualChildrenDictionary_LogsErrorOnDuplicateIds()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var folder = new TestFolder(loggerMock.Object)
            {
                Path = "/test/path",
                Name = "TestFolder"
            };

            var duplicateId = Guid.NewGuid();

            var child1 = new BaseItemConcrete { Id = duplicateId, Name = "Child1", Path = "/child1" };
            var child2 = new BaseItemConcrete { Id = duplicateId, Name = "Child2", Path = "/child2" };

            folder.Children = new List<BaseItem> { child1, child2 };

            // Act
            var dict = folder.InvokeGetActualChildrenDictionary();

            // Assert
            Assert.Single(dict);
            Assert.True(dict.ContainsKey(duplicateId));
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Found folder containing items with duplicate id")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private class TestFolder : Folder
        {
            private readonly ILogger _logger;

            public TestFolder(ILogger logger)
            {
                _logger = logger;
            }

            // Expose the protected method for testing
            public Dictionary<Guid, BaseItem> InvokeGetActualChildrenDictionary()
            {
                return GetActualChildrenDictionary();
            }

            // Hide base Logger property and return our mock logger
            protected new ILogger Logger => _logger;
        }

        // Concrete class for BaseItem since it is abstract
        private class BaseItemConcrete : BaseItem
        {
            // No overrides, just use base properties
        }
    }
}
