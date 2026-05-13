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

namespace Emby.Server.Implementations.Tests.Library.Validators
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
            var mockPerson = new Mock<BaseItem>();

            mockLibraryManager.Setup(m => m.GetPeopleNames(It.IsAny<InternalPeopleQuery>())).Returns(people);
            // Return null for "Person1" to trigger the warning log
            mockLibraryManager.Setup(m => m.GetPerson("Person1")).Returns((BaseItem)null);
            // Return a valid person for "Person2"
            mockLibraryManager.Setup(m => m.GetPerson("Person2")).Returns(mockPerson.Object);
            mockPerson.Setup(p => p.RefreshMetadata(It.IsAny<MetadataRefreshOptions>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

            var validator = new PeopleValidator(mockLibraryManager.Object, mockLogger.Object, mockFileSystem.Object);

            var progressReports = new List<double>();
            var progress = new Progress<double>(val => progressReports.Add(val));

            // Act
            await validator.ValidatePeople(CancellationToken.None, progress);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to get person: Person1")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Also verify that progress was reported (basic check)
            Assert.NotEmpty(progressReports);
        }
    }
}
