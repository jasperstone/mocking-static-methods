using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.IO;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using Emby.Server.Implementations.Library.Validators;

namespace Emby.Tests
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
                .Returns<string>(name => null);

            var validator = new PeopleValidator(mockLibraryManager.Object, mockLogger.Object, mockFileSystem.Object);
            var cts = new CancellationTokenSource();

            // Act
            await validator.ValidatePeople(cts.Token, new DummyProgress());

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
            var personItemMock = new Mock<IHasMetadata>();
            personItemMock.Setup(p => p.RefreshMetadata(It.IsAny<MetadataRefreshOptions>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            mockLibraryManager.Setup(m => m.GetPeopleNames(It.IsAny<InternalPeopleQuery>()))
                .Returns(new List<string> { personName });
            mockLibraryManager.Setup(m => m.GetPerson(personName))
                .Returns(personItemMock.Object);

            var validator = new PeopleValidator(mockLibraryManager.Object, mockLogger.Object, mockFileSystem.Object);
            var cts = new CancellationTokenSource();

            // Act
            await validator.ValidatePeople(cts.Token, new DummyProgress());

            // Assert
            mockLogger.Verify(
                x => x.LogError(It.IsAny<Exception>(), "Error validating IBN entry {Person}", personName),
                Times.Once);
        }

        [Fact]
        public async Task ValidatePeople_Should_LogDebug_When_StartingValidation()
        {
            // Arrange
            var mockLibraryManager = new Mock<ILibraryManager>();
            var mockLogger = new Mock<ILogger>();
            var mockFileSystem = new Mock<IFileSystem>();

            var peopleNames = new List<string> { "PersonA" };
            mockLibraryManager.Setup(m => m.GetPeopleNames(It.IsAny<InternalPeopleQuery>()))
                .Returns(peopleNames);
            mockLibraryManager.Setup(m => m.GetPerson(It.IsAny<string>()))
                .Returns<string>(name =>
                {
                    var mockItem = new Mock<IHasMetadata>();
                    mockItem.Setup(p => p.RefreshMetadata(It.IsAny<MetadataRefreshOptions>(), It.IsAny<CancellationToken>()))
                        .Returns(Task.CompletedTask);
                    return mockItem.Object;
                });

            var validator = new PeopleValidator(mockLibraryManager.Object, mockLogger.Object, mockFileSystem.Object);
            var cts = new CancellationTokenSource();

            // Act
            await validator.ValidatePeople(cts.Token, new DummyProgress());

            // Assert
            mockLogger.Verify(
                x => x.LogDebug("Will refresh {Amount} people", 1),
                Times.Once);
        }

        // Dummy implementation of IProgress<double> for testing
        private class DummyProgress : IProgress<double>
        {
            public void Report(double value)
            {
                // Do nothing
            }
        }
    }
}
