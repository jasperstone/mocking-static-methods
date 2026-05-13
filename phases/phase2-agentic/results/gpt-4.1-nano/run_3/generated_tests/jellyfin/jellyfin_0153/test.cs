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

            var peopleNames = new List<string> { "Person1" };
            mockLibraryManager.Setup(m => m.GetPeopleNames(It.IsAny<InternalPeopleQuery>()))
                .Returns(peopleNames);
            mockLibraryManager.Setup(m => m.GetPerson(It.IsAny<string>()))
                .Returns<string>(null);

            var validator = new PeopleValidator(mockLibraryManager.Object, mockLogger.Object, mockFileSystem.Object);
            var cts = new CancellationTokenSource();

            // Act
            await validator.ValidatePeople(cts.Token, new Progress<double>());

            // Assert
            mockLogger.Verify(
                x => x.LogWarning("Failed to get person: {Name}", "Person1"),
                Times.Once);
        }

        [Fact]
        public async Task ValidatePeople_Should_LogError_When_ExceptionThrown()
        {
            // Arrange
            var mockLibraryManager = new Mock<ILibraryManager>();
            var mockLogger = new Mock<ILogger>();
            var mockFileSystem = new Mock<IFileSystem>();

            var personName = "Person2";
            mockLibraryManager.Setup(m => m.GetPeopleNames(It.IsAny<InternalPeopleQuery>()))
                .Returns(new List<string> { personName });
            var mockPerson = new Mock<IHasMetadata>();
            mockLibraryManager.Setup(m => m.GetPerson(personName))
                .Returns(mockPerson.Object);
            mockPerson.Setup(p => p.RefreshMetadata(It.IsAny<MetadataRefreshOptions>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            var validator = new PeopleValidator(mockLibraryManager.Object, mockLogger.Object, mockFileSystem.Object);
            var cts = new CancellationTokenSource();

            // Act
            await validator.ValidatePeople(cts.Token, new Progress<double>());

            // Assert
            mockLogger.Verify(
                x => x.LogError(It.IsAny<Exception>(), "Error validating IBN entry {Person}", personName),
                Times.Once);
        }

        [Fact]
        public async Task ValidatePeople_Should_ReportProgressCorrectly()
        {
            // Arrange
            var mockLibraryManager = new Mock<ILibraryManager>();
            var mockLogger = new Mock<ILogger>();
            var mockFileSystem = new Mock<IFileSystem>();

            var peopleNames = new List<string> { "Person1", "Person2" };
            mockLibraryManager.Setup(m => m.GetPeopleNames(It.IsAny<InternalPeopleQuery>()))
                .Returns(peopleNames);
            var mockPerson = new Mock<IHasMetadata>();
            mockLibraryManager.Setup(m => m.GetPerson(It.IsAny<string>()))
                .Returns(mockPerson.Object);
            mockPerson.Setup(p => p.RefreshMetadata(It.IsAny<MetadataRefreshOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var progressReports = new List<double>();
            var progress = new Progress<double>(val => progressReports.Add(val));

            var validator = new PeopleValidator(mockLibraryManager.Object, mockLogger.Object, mockFileSystem.Object);
            var cts = new CancellationTokenSource();

            // Act
            await validator.ValidatePeople(cts.Token, progress);

            // Assert
            Assert.Contains(50, progressReports);
            Assert.Contains(100, progressReports);
        }
    }
}
