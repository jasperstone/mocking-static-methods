using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Emby.Server.Implementations.Library.Validators;
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
        public async Task ValidatePeople_LogsWarning_WhenPersonIsNull()
        {
            // Arrange
            var libraryManagerMock = new Mock<ILibraryManager>();
            var loggerMock = new Mock<ILogger>();
            var fileSystemMock = new Mock<IFileSystem>();

            var people = new List<string> { "Person1" };
            libraryManagerMock.Setup(lm => lm.GetPeopleNames(It.IsAny<InternalPeopleQuery>())).Returns(people);
            libraryManagerMock.Setup(lm => lm.GetPerson(It.IsAny<string>())).Returns((Person)null);

            var validator = new PeopleValidator(libraryManagerMock.Object, loggerMock.Object, fileSystemMock.Object);
            var progress = new Progress<double>();
            var cancellationToken = new CancellationToken();

            // Act
            await validator.ValidatePeople(cancellationToken, progress);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to get person: Person1")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
