using Emby.Server.Implementations.Library.Validators;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Emby.Server.Tests
{
    public class PeopleValidatorTests
    {
        [Fact]
        public async Task ValidatePeople_LogsWarning_WhenPersonIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            libraryManagerMock.Setup(l => l.GetPerson(It.IsAny<string>())).Returns((MediaBrowser.Controller.Entities.Person?)null);
            libraryManagerMock.Setup(l => l.GetPeopleNames(It.IsAny<MediaBrowser.Controller.Queries.InternalPeopleQuery>())).Returns(new List<string> { "Person1" });
            var peopleValidator = new PeopleValidator(libraryManagerMock.Object, loggerMock.Object, null);

            // Act
            await peopleValidator.ValidatePeople(CancellationToken.None, new Progress<double>(p => { }));

            // Assert
            loggerMock.Verify(l => l.LogWarning("Failed to get person: {Name}", "Person1"), Times.Once);
        }

        [Fact]
        public async Task ValidatePeople_RefreshesMetadata_WhenPersonIsNotNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var personMock = new Mock<MediaBrowser.Controller.Entities.Person>();
            personMock.Setup(p => p.RefreshMetadata(It.IsAny<MediaBrowser.Model.Configuration.MetadataRefreshOptions>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            libraryManagerMock.Setup(l => l.GetPerson(It.IsAny<string>())).Returns(personMock.Object);
            libraryManagerMock.Setup(l => l.GetPeopleNames(It.IsAny<MediaBrowser.Controller.Queries.InternalPeopleQuery>())).Returns(new List<string> { "Person1" });
            var peopleValidator = new PeopleValidator(libraryManagerMock.Object, loggerMock.Object, null);

            // Act
            await peopleValidator.ValidatePeople(CancellationToken.None, new Progress<double>(p => { }));

            // Assert
            personMock.Verify(p => p.RefreshMetadata(It.IsAny<MediaBrowser.Model.Configuration.MetadataRefreshOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
