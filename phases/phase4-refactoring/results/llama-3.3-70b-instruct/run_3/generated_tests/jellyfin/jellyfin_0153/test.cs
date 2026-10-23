using Emby.Server.Implementations.Library.Validators;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Querying;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Emby.Server.Tests
{
    public class PeopleValidatorTests
    {
        [Fact]
        public async Task ValidatePeople_LogsWarningWhenPersonNotFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            libraryManagerMock.Setup(l => l.GetPeopleNames(It.IsAny<InternalPeopleQuery>())).Returns(new[] { "Person1" });
            libraryManagerMock.Setup(l => l.GetPerson("Person1")).Returns((Person)null);
            var fileSystemMock = new Mock<MediaBrowser.Model.IO.IFileSystem>();
            var peopleValidator = new PeopleValidator(libraryManagerMock.Object, loggerMock.Object, fileSystemMock.Object);
            var progressMock = new Mock<IProgress<double>>();

            // Act
            await peopleValidator.ValidatePeople(CancellationToken.None, progressMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogWarning("Failed to get person: {Name}", "Person1"), Times.Once);
        }
    }
}
