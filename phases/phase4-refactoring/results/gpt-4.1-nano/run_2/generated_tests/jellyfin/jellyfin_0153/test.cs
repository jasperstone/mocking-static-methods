using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

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
            var mockFileSystem = new Mock<MediaBrowser.Model.IO.IFileSystem>();

            var peopleNames = new List<string> { "Person1" };
            mockLibraryManager.Setup(m => m.GetPeopleNames(It.IsAny<InternalPeopleQuery>()))
                .Returns(peopleNames);
            mockLibraryManager.Setup(m => m.GetPerson(It.IsAny<string>()))
                .Returns<string>(null); // Simulate not found

            var validator = new PeopleValidator(mockLibraryManager.Object, mockLogger.Object, mockFileSystem.Object);
            var cts = new CancellationTokenSource();

            var progress = new Mock<IProgress<double>>();

            // Act
            await validator.ValidatePeople(cts.Token, progress.Object);

            // Assert
            mockLogger.Verify(
                x => x.LogWarning("Failed to get person: {Name}", "Person1"),
                Times.Once);
        }
    }
}
