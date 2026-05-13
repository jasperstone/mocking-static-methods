using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Emby.Server.Implementations.Library.Validators;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Library.Validators.Tests
{
    public class PeopleValidatorTests
    {
        [Fact]
        public async Task ValidatePeople_LogsWarningWhenPersonIsNull()
        {
            // Arrange
            var people = new List<string> { "Person1", "Person2" };
            var mockLibraryManager = new Mock<ILibraryManager>();
            var mockLogger = new Mock<ILogger>();
            var mockFileSystem = new Mock<IFileSystem>();
            var mockPersonItem = new Mock<BaseItem>();

            // Setup GetPeopleNames to return two people
            mockLibraryManager.Setup(m => m.GetPeopleNames(It.IsAny<InternalPeopleQuery>())).Returns(people);

            // Setup GetPerson to return null for "Person1" to trigger LogWarning, and a valid item for "Person2"
            mockLibraryManager.Setup(m => m.GetPerson("Person1")).Returns((BaseItem)null);
            mockLibraryManager.Setup(m => m.GetPerson("Person2")).Returns(mockPersonItem.Object);

            // Setup RefreshMetadata to complete successfully
            mockPersonItem.Setup(p => p.RefreshMetadata(It.IsAny<MetadataRefreshOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var validator = new PeopleValidator(mockLibraryManager.Object, mockLogger.Object, mockFileSystem.Object);

            var progressReports = new List<double>();
            var progress = new Progress<double>(val => progressReports.Add(val));

            // Act
            await validator.ValidatePeople(CancellationToken.None, progress);

            // Assert
            // Verify LogWarning was called with the expected message and person name "Person1"
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to get person: Person1")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Verify LogDebug was called at least once
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

            // Verify LogInformation was called at least once
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("People validation complete")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Verify progress was reported (at least once)
            Assert.NotEmpty(progressReports);
        }
    }
}
