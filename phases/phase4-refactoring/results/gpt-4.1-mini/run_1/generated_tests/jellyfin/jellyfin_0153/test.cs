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

            mockLibraryManager.Setup(m => m.GetPeopleNames(It.IsAny<InternalPeopleQuery>())).Returns(people);
            // Return null for the first person to trigger the warning log
            mockLibraryManager.SetupSequence(m => m.GetPerson(It.IsAny<string>()))
                .Returns((Person)null)
                .Returns((Person)null); // Return null again to avoid calling RefreshMetadata

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

            // Also verify progress was reported
            Assert.NotEmpty(progressReports);
        }
    }
}
