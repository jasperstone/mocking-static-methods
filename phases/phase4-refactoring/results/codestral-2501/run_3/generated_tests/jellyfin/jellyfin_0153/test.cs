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

public class PeopleValidatorTests
{
    [Fact]
    public async Task ValidatePeople_LogsWarning_WhenPersonIsNull()
    {
        // Arrange
        var libraryManagerMock = new Mock<ILibraryManager>();
        var loggerMock = new Mock<ILogger>();
        var fileSystemMock = new Mock<IFileSystem>();

        libraryManagerMock.Setup(lm => lm.GetPeopleNames(It.IsAny<InternalPeopleQuery>()))
            .Returns(new List<string> { "Person1" });
        libraryManagerMock.Setup(lm => lm.GetPerson("Person1"))
            .Returns((BaseItem)null);

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
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
