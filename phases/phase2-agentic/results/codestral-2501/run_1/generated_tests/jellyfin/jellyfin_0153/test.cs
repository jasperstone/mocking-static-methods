using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Emby.Server.Implementations.Library.Validators;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Library.Validators.Tests
{
    public class PeopleValidatorTests
    {
        [Fact]
        public async Task ValidatePeople_LogsWarning_WhenPersonIsNull()
        {
            // Arrange
            var libraryManagerMock = new Mock<ILibraryManager>();
            var loggerMock = new Mock<ILogger>();
            var fileSystemMock = new Mock<IFileSystem>();

            var people = new List<string> { "Person1", "Person2" };
            libraryManagerMock.Setup(lm => lm.GetPeopleNames(It.IsAny<InternalPeopleQuery>())).Returns(people);
            libraryManagerMock.Setup(lm => lm.GetPerson(It.IsAny<string>())).Returns((BaseItem)null);

            var validator = new PeopleValidator(libraryManagerMock.Object, loggerMock.Object, fileSystemMock.Object);

            // Act
            await validator.ValidatePeople(CancellationToken.None, new Progress<double>());

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to get person: Person1")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to get person: Person2")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
