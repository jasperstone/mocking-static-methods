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

            // Mock the deadEntities query to return empty list to avoid Chunk null error
            _mockLibraryManager.Setup(m => m.GetItemList(It.Is<InternalItemsQuery>(q => 
                q.IncludeItemTypes.Contains(BaseItemKind.Person) && 
                q.IsDeadPerson == true && 
                q.IsLocked == false)))
                .Returns(new List<BaseItem>());

            var progress = new Mock<IProgress<double>>().Object;
            var cancellationToken = new CancellationToken();

            // Act
            await _validator.ValidatePeople(cancellationToken, progress);

            // Assert
            _mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString()!.Contains("Failed to get person: John Doe", StringComparison.Ordinal)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ValidatePeople_WhenPeopleFound_ProcessesWithoutWarning()
        {
            // Arrange
            var peopleNames = new List<string> { "Jane Doe" };
            var person = new Mock<Person>().Object;
            _mockLibraryManager.Setup(m => m.GetPeopleNames(It.IsAny<InternalPeopleQuery>()))
                .Returns(peopleNames);

            _mockLibraryManager.Setup(m => m.GetPerson("Jane Doe"))
                .Returns(person);

            // Mock the deadEntities query to return empty list
            _mockLibraryManager.Setup(m => m.GetItemList(It.Is<InternalItemsQuery>(q => 
                q.IncludeItemTypes.Contains(BaseItemKind.Person) && 
                q.IsDeadPerson == true && 
                q.IsLocked == false)))
                .Returns(new List<BaseItem>());

            // Don't mock RefreshMetadata - let it throw and be caught by the validator's exception handler
            var progress = new Mock<IProgress<double>>().Object;
            var cancellationToken = new CancellationToken();

            // Act
            await _validator.ValidatePeople(cancellationToken, progress);

            // Assert - no warning logged for person not found
            _mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString()!.Contains("Failed to get person", StringComparison.Ordinal)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
    }
}
