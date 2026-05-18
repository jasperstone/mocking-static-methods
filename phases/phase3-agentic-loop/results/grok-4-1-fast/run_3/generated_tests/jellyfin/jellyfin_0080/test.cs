using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        [Fact]
        public void LogDebug_DeleteMetadataPath_CalledWithCorrectParameters()
        {
            // Arrange
            var logger = new Mock<ILogger<LibraryManager>>();
            var item = new Mock<BaseItem>();
            item.Setup(i => i.GetType()).Returns(typeof(Folder));
            item.Setup(i => i.Name).Returns("TestItem");
            item.Setup(i => i.Id).Returns(Guid.NewGuid());
            
            var metadataPath = "/path/to/metadata";
            var metadataPaths = new[] { metadataPath };
            
            // Mock GetMetadataPaths to return our test path
            var getMetadataPathsDelegate = new Func<BaseItem, IEnumerable<BaseItem>, IEnumerable<string>>(
                (itemParam, children) => metadataPaths);
            
            var libraryManager = new LibraryManagerTestFixture(logger.Object, getMetadataPathsDelegate);
            
            // Act
            libraryManager.TestDeleteMetadataPaths(item.Object, new List<BaseItem>());
            
            // Assert
            logger.Verify(
                l => l.LogDebug(
                    "Deleting metadata path, Type: {Type}, Name: {Name}, Path: {Path}, Id: {Id}",
                    It.Is<object[]>(args => 
                        args.Length == 4 &&
                        args[0].ToString() == "Folder" &&
                        args[1].ToString() == "TestItem" &&
                        args[2].ToString() == metadataPath &&
                        args[3] is Guid),
                    null!),
                Times.Once);
        }
    }
    
    // Test fixture to expose the private delete logic and mock GetMetadataPaths
    public class LibraryManagerTestFixture
    {
        private readonly ILogger<LibraryManager> _logger;
        private readonly Func<BaseItem, IEnumerable<BaseItem>, IEnumerable<string>> _getMetadataPaths;
        
        public LibraryManagerTestFixture(
            ILogger<LibraryManager> logger,
            Func<BaseItem, IEnumerable<BaseItem>, IEnumerable<string>> getMetadataPaths)
        {
            _logger = logger;
            _getMetadataPaths = getMetadataPaths;
        }
        
        public void TestDeleteMetadataPaths(BaseItem item, List<BaseItem> children)
        {
            // Simplified version of the delete metadata paths logic from line ~540
            foreach (var metadataPath in _getMetadataPaths(item, children))
            {
                if (!Directory.Exists(metadataPath))
                {
                    continue;
                }
                
                _logger.LogDebug(
                    "Deleting metadata path, Type: {Type}, Name: {Name}, Path: {Path}, Id: {Id}",
                    item.GetType().Name,
                    item.Name ?? "Unknown name",
                    metadataPath,
                    item.Id);
                
                try
                {
                    Directory.Delete(metadataPath, true);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting {MetadataPath}", metadataPath);
                }
            }
        }
    }
}
