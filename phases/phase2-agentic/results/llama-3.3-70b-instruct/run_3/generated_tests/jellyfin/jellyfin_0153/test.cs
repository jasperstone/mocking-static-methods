using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Emby.Server.Implementations.Library.Validators
{
    public class PeopleValidatorTests
    {
        [Fact]
        public async Task ValidatePeople_LogsWarningWhenPersonNotFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var peopleValidator = new PeopleValidator(libraryManagerMock.Object, loggerMock.Object, fileSystemMock.Object);

            libraryManagerMock.Setup(l => l.GetPeopleNames(It.IsAny<InternalPeopleQuery>())).Returns(new[] { "Person1" });
            libraryManagerMock.Setup(l => l.GetPerson("Person1")).Returns((Person)null);

            // Act
            await peopleValidator.ValidatePeople(CancellationToken.None, new Progress<double>());

            // Assert
            loggerMock.Verify(l => l.LogWarning("Failed to get person: {Name}", "Person1"), Times.Once);
        }
    }
}
