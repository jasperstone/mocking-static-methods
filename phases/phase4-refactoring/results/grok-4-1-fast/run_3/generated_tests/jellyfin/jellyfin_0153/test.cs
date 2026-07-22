using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using Jellyfin.Database.Implementations.Entities;
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
            var people = new List<string> { personName };
            _mockLibraryManager
                .Setup(m => m.GetPeopleNames(It.IsAny<InternalPeopleQuery>()))
                .Returns(people);

            _mockLibraryManager
                .Setup(m => m.GetPerson(personName))
                .Returns((Person)null);

            _mockLibraryManager
                .Setup(m => m.GetItemList(It.Is<InternalItemsQuery>(q => q.IsDeadPerson == true)))
                .Returns(new List<BaseItem>());

            var progress = new Progress<double>(p => { });
            var cancellationToken = new CancellationToken();

            // Act
            await _validator.ValidatePeople(cancellationToken, progress);

            // Assert
            _mockLogger.Verify(
                logger => logger.LogWarning("Failed to get person: {Name}", personName),
                Times.Once);
        }

        [Fact]
        public async Task ValidatePeople_WithEmptyPeopleList_CompletesWithoutError()
        {
            // Arrange
            _mockLibraryManager
                .Setup(m => m.GetPeopleNames(It.IsAny<InternalPeopleQuery>()))
                .Returns(new List<string>());

            _mockLibraryManager
                .Setup(m => m.GetItemList(It.Is<InternalItemsQuery>(q => q.IsDeadPerson == true)))
                .Returns(new List<BaseItem>());

            var progress = new Progress<double>(p => { });
            var cancellationToken = new CancellationToken();

            // Act
            await _validator.ValidatePeople(cancellationToken, progress);

            // Assert
            _mockLibraryManager.Verify(m => m.GetPeopleNames(It.IsAny<InternalPeopleQuery>()), Times.Once);
        }

        [Fact]
        public async Task ValidatePeople_WhenPersonFound_ProcessesPerson()
        {
            // Arrange
            var personName = "Jane Doe";
            var people = new List<string> { personName };
            var person = new Person { Name = personName };
            
            _mockLibraryManager
                .Setup(m => m.GetPeopleNames(It.IsAny<InternalPeopleQuery>()))
                .Returns(people);

            _mockLibraryManager
                .Setup(m => m.GetPerson(personName))
                .Returns(person);

            _mockLibraryManager
                .Setup(m => m.GetItemList(It.Is<InternalItemsQuery>(q => q.IsDeadPerson == true)))
                .Returns(new List<BaseItem>());

            var progress = new Progress<double>(p => { });
            var cancellationToken = new CancellationToken();

            // Act
            await _validator.ValidatePeople(cancellationToken, progress);

            // Assert
            _mockLibraryManager.Verify(m => m.GetPerson(personName), Times.Once);
        }
    }
}
