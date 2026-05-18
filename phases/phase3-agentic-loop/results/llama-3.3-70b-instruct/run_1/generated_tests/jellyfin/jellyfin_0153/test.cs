using Emby.Server.Implementations.Library;
using Emby.Server.Implementations.Library.Validators;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class PeopleValidatorTests
    {
        [Fact]
        public async Task ValidatePeople_LogsWarning_WhenPersonIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var peopleValidator = new PeopleValidator(libraryManagerMock.Object, loggerMock.Object, fileSystemMock.Object);

            libraryManagerMock.Setup(l => l.GetPeopleNames(It.IsAny<InternalPeopleQuery>())).Returns(new List<string> { "Person1" });
            libraryManagerMock.Setup(l => l.GetPerson("Person1")).Returns((Person)null);

            // Act
            await peopleValidator.ValidatePeople(CancellationToken.None, new Progress<double>());

            // Assert
            loggerMock.Verify(l => l.LogWarning("Failed to get person: {Name}", "Person1"), Times.Once);
        }
    }
}
