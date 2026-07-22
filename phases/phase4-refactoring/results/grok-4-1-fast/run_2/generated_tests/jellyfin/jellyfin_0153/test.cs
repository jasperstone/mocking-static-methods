using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Querying;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Library.Validators.Tests
{
    public class PeopleValidatorTests
    {
        private readonly Mock<ILibraryManager> _mockLibraryManager;
        private readonly Mock<ILogger<PeopleValidator>> _mockLogger;
        private readonly Mock<IFileSystem> _mockFileSystem;
        private readonly PeopleValidator _validator;

        public PeopleValidatorTests()
        {
            _mockLibraryManager = new Mock<ILibraryManager>();
            _mockLogger = new Mock<ILogger<PeopleValidator>>();
            _mockFileSystem = new Mock<IFileSystem>();

            _validator = new PeopleValidator(
                _mockLibraryManager.Object,
                _mockLogger.Object,
                _mockFileSystem.Object);
        }

        [Fact]
        public async Task ValidatePeople_WhenPersonNotFound_LogsWarning()
        {
            // Arrange
            var peopleNames = new List<string> { "John Doe" };
            _mockLibraryManager.Setup(m => m.GetPeopleNames(It.IsAny<InternalPeopleQuery>()))
                .Returns(peopleNames);

            _mockLibraryManager.Setup(m => m.GetPerson("John Doe"))
                .Returns((Person)null);

            _mockLibraryManager.Setup(m => m.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(new List<BaseItem>());

            var progress = new Progress<double>(p => { });
            var cancellationToken = new CancellationToken();

            // Act
            await _validator.ValidatePeople(cancellationToken, progress);

            // Assert
            _mockLogger.Verify(
                x => x.LogWarning(
                    "Failed to get person: {Name}",
                    "John Doe"),
                Times.Once);
        }

        [Fact]
        public async Task ValidatePeople_WhenPersonFound_DoesNotLogWarning()
        {
            // Arrange
            var peopleNames = new List<string> { "Jane Doe" };
            var person = new Mock<Person>().Object;
            _mockLibraryManager.Setup(m => m.GetPeopleNames(It.IsAny<InternalPeopleQuery>()))
                .Returns(peopleNames);

            _mockLibraryManager.Setup(m => m.GetPerson("Jane Doe"))
                .Returns(person);

            _mockLibraryManager.Setup(m => m.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(new List<BaseItem>());

            Mock.Get(person).Setup(p => p.RefreshMetadata(It.IsAny<MetadataRefreshOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(ItemUpdateType.None));

            var progress = new Progress<double>(p => { });
            var cancellationToken = new CancellationToken();

            // Act
            await _validator.ValidatePeople(cancellationToken, progress);

            // Assert
            _mockLogger.Verify(
                x => x.LogWarning(
                    "Failed to get person: {Name}",
                    It.IsAny<object[]>()),
                Times.Never);
        }
    }
}
