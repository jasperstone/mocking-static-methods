using Xunit;
using Moq;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        [Fact]
        public void GetLocalAlternateVersionIds_WhenVideoIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            var mockLinkedChildrenService = new Mock<ILinkedChildrenService>();
            var libraryManager = new LibraryManager(
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                mockLinkedChildrenService.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null
            );

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => libraryManager.GetLocalAlternateVersionIds(null));
        }

        [Fact]
        public void GetLocalAlternateVersionIds_WhenVideoHasLinkedIds_ReturnsLinkedIds()
        {
            // Arrange
            var mockLinkedChildrenService = new Mock<ILinkedChildrenService>();
            var video = new Video { Id = Guid.NewGuid() };
            var linkedIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            mockLinkedChildrenService.Setup(service => service.GetLinkedChildrenIds(video.Id, (int)MediaBrowser.Controller.Entities.LinkedChildType.LocalAlternateVersion))
                .Returns(linkedIds);

            var libraryManager = new LibraryManager(
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                mockLinkedChildrenService.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null
            );

            // Act
            var result = libraryManager.GetLocalAlternateVersionIds(video);

            // Assert
            Assert.Equal(linkedIds, result);
        }

        [Fact]
        public void GetLinkedAlternateVersions_WhenVideoIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            var mockLinkedChildrenService = new Mock<ILinkedChildrenService>();
            var libraryManager = new LibraryManager(
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                mockLinkedChildrenService.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null
            );

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => libraryManager.GetLinkedAlternateVersions(null));
        }

        [Fact]
        public void GetLinkedAlternateVersions_WhenVideoHasLinkedIds_ReturnsLinkedIds()
        {
            // Arrange
            var mockLinkedChildrenService = new Mock<ILinkedChildrenService>();
            var video = new Video { Id = Guid.NewGuid() };
            var linkedIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            mockLinkedChildrenService.Setup(service => service.GetLinkedChildrenIds(video.Id, (int)MediaBrowser.Controller.Entities.LinkedChildType.LinkedAlternateVersion))
                .Returns(linkedIds);

            var libraryManager = new LibraryManager(
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                mockLinkedChildrenService.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null
            );

            // Act
            var result = libraryManager.GetLinkedAlternateVersions(video);

            // Assert
            Assert.Equal(linkedIds, result);
        }
    }
}
