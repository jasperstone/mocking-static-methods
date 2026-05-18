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
            var personName = "John Doe";
            var peopleNames = new List<string> { personName };
            _mockLibraryManager
                .Setup(m => m.GetPeopleNames(It.IsAny<InternalPeopleQuery>()))
                .Returns(peopleNames);
            _mockLibraryManager
                .Setup(m => m.GetPerson(personName))
                .Returns((Person)null);

            var progress = new Progress<double>(p => { });
            var cancellationToken = new CancellationToken();

            // Act
            await _validator.ValidatePeople(cancellationToken, progress);

            // Assert
            _mockLogger.Verify(
                x => x.LogWarning(
                    "Failed to get person: {Name}", 
                    personName),
                Times.Once);
        }

        [Fact]
        public async Task ValidatePeople_WhenPersonFound_DoesNotLogWarning()
        {
            // Arrange
            var personName = "Jane Doe";
            var peopleNames = new List<string> { personName };
            var mockPerson = new Mock<Person>();
            mockPerson.Setup(p => p.RefreshMetadata(It.IsAny<MetadataRefreshOptions>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.FromResult(ItemUpdateType.None));
            var person = mockPerson.Object;

            _mockLibraryManager
                .Setup(m => m.GetPeopleNames(It.IsAny<InternalPeopleQuery>()))
                .Returns(peopleNames);
            _mockLibraryManager
                .Setup(m => m.GetPerson(personName))
                .Returns(person);

            var progress = new Progress<double>(p => { });
            var cancellationToken = new CancellationToken();

            // Act
            await _validator.ValidatePeople(cancellationToken, progress);

            // Assert
            _mockLogger.Verify(
                x => x.LogWarning(
                    "Failed to get person: {Name}", 
                    It.IsAny<string>()),
                Times.Never);
        }
    }
}
