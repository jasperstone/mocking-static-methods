using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Entities;
using MediaBrowser.Controller.Providers;
using Emby.Server.Implementations.Library.Validators;

public class PeopleValidatorTests
{
    [Fact]
    public async Task ValidatePeople_Should_LogWarning_When_GetPersonReturnsNull()
    {
        // Arrange
        var mockLibraryManager = new Mock<ILibraryManager>();
        var mockLogger = new Mock<ILogger>();
        var mockFileSystem = new Mock<IFileSystem>();

        var peopleNames = new List<string> { "Person1" };
        mockLibraryManager.Setup(m => m.GetPeopleNames(It.IsAny<InternalPeopleQuery>()))
            .Returns(peopleNames);
        mockLibraryManager.Setup(m => m.GetPerson(It.IsAny<string>()))
            .Returns<string>(name => null);

        var validator = new PeopleValidator(mockLibraryManager.Object, mockLogger.Object, mockFileSystem.Object);
        var cts = new CancellationTokenSource();
        var progress = new Mock<IProgress<double>>();

        // Act
        await validator.ValidatePeople(cts.Token, progress.Object);

        // Assert
        mockLogger.Verify(
            x => x.LogWarning("Failed to get person: {Name}", "Person1"),
            Times.Once);
    }
}
