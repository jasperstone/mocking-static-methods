using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Entities;

namespace Emby.Server.Implementations.Library.Validators.Tests
{
    public class PeopleValidatorTests
    {
        [Fact]
        public async Task ValidatePeople_Should_LogWarning_When_GetPersonReturnsNull()
        {
            // Arrange
            var mockLibraryManager = new Mock<ILibraryManager>();
            var mockLogger = new Mock<ILogger>();
            var mockFileSystem = new Mock<IFileSystem>();

            var peopleNames = new List<string> { "Person1", "Person2" };
            mockLibraryManager.Setup(m => m.GetPeopleNames(It.IsAny<InternalPeopleQuery>()))
                .Returns(peopleNames);

            // Return null for the first person, valid object for the second
            mockLibraryManager.SetupSequence(m => m.GetPerson(It.IsAny<string>()))
                .Returns<string>(name => null)
                .Returns<string>(name => new Mock<IHasMetadata>().Object);

            var mockMetadata = new Mock<IHasMetadata>();
            mockMetadata.Setup(m => m.RefreshMetadata(It.IsAny<MetadataRefreshOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            mockLibraryManager.Setup(m => m.GetPerson("Person2"))
                .Returns(mockMetadata.Object);

            var validator = new PeopleValidator(mockLibraryManager.Object, mockLogger.Object, mockFileSystem.Object);

            var progress = new Mock<IProgress<double>>();

            // Act
            await validator.ValidatePeople(CancellationToken.None, progress.Object);

            // Assert
            mockLogger.Verify(
                x => x.LogWarning("Failed to get person: {Name}", "Person1"),
                Times.Once);
        }
    }
}
