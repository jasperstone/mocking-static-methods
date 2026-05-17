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

namespace Emby.Server.Implementations.Library.Validators.Tests
{
    public class PeopleValidatorTests
    {
        [Fact]
        public async Task ValidatePeople_ShouldLogWarning_WhenGetPersonReturnsNull()
        {
            // Arrange
            var libraryManagerMock = new Mock<ILibraryManager>();
            var loggerMock = new Mock<ILogger>();
            var fileSystemMock = new Mock<IFileSystem>();

            var people = new List<string> { "Person1" };
            libraryManagerMock.Setup(lm => lm.GetPeopleNames(It.IsAny<InternalPeopleQuery>())).Returns(people);
            libraryManagerMock.Setup(lm => lm.GetPerson(It.IsAny<string>())).Returns((Person)null);
            libraryManagerMock.Setup(lm => lm.GetItemList(It.IsAny<InternalItemsQuery>())).Returns(new List<BaseItem>());

            var validator = new PeopleValidator(libraryManagerMock.Object, loggerMock.Object, fileSystemMock.Object);
            var progress = new Progress<double>();

            // Act
            await validator.ValidatePeople(CancellationToken.None, progress);

            // Assert
            loggerMock.Verify(
                logger => logger.LogWarning("Failed to get person: {Name}", It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
