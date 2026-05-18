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

            var progress = new Progress<double>(p => { });
            var cancellationToken = new CancellationToken();

            // Act
            await _validator.ValidatePeople(cancellationToken, progress);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to get person: John Doe")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ValidatePeople_WhenPersonFound_DoesNotLogWarning()
        {
            // Arrange
            var peopleNames = new List<string> { "Jane Doe" };
            var mockPerson = new Mock<Person>();
            mockPerson.Setup(p => p.RefreshMetadata(
                It.IsAny<MetadataRefreshOptions>(), 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(ItemUpdateType.Complete);

            _mockLibraryManager.Setup(m => m.GetPeopleNames(It.IsAny<InternalPeopleQuery>()))
                .Returns(peopleNames);

            _mockLibraryManager.Setup(m => m.GetPerson("Jane Doe"))
                .Returns(mockPerson.Object);

            var progress = new Progress<double>(p => { });
            var cancellationToken = new CancellationToken();

            // Act
            await _validator.ValidatePeople(cancellationToken, progress);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
    }
}
